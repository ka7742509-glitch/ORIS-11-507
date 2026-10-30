using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace MiniTemplateEngine;

public class HtmlTemplateRenderer : IHtmlTemplateRenderer
{
    private static readonly Regex VariableRegex = new(@"\$\{([^}]+)\}", RegexOptions.Compiled);
    
    private static readonly Regex IfBlockRegex = new(
        @"\$if\s*\(([^)]+)\)([\s\S]*?)(?:\$else([\s\S]*?))?\$endif", 
        RegexOptions.Multiline | RegexOptions.Compiled);
        
    private static readonly Regex ForeachBlockRegex = new(
        @"\$foreach\s*\(\s*var\s+(\w+)\s+in\s+([^\)]+)\s*\)([\s\S]*?)\$endfor", 
        RegexOptions.Multiline | RegexOptions.Compiled);

    public string RenderFromString(string htmlTemplate, object dataModel)
    {
        if (string.IsNullOrEmpty(htmlTemplate)) return "";
        
        string result = htmlTemplate;

        result = ProcessLoops(result, dataModel);
        result = ProcessConditions(result, dataModel);
        result = ProcessVariables(result, dataModel);

        return result;
    }

    public string RenderFromFile(string filePath, object dataModel)
    {
        if (!File.Exists(filePath)) throw new FileNotFoundException($"Шаблон не найден: {filePath}");
        string template = File.ReadAllText(filePath);
        return RenderFromString(template, dataModel);
    }

    public string RenderToFile(string inputFilePath, string outputFilePath, object dataModel)
    {
        string renderedHtml = RenderFromFile(inputFilePath, dataModel);
        var dir = Path.GetDirectoryName(outputFilePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(outputFilePath, renderedHtml);
        return renderedHtml;
    }

    #region Private Logic

    private string ProcessLoops(string text, object rootModel)
    {
        MatchCollection matches = ForeachBlockRegex.Matches(text);
        
        for (int i = matches.Count - 1; i >= 0; i--)
        {
            var match = matches[i];
            string iteratorName = match.Groups[1].Value.Trim();
            string collectionExpr = match.Groups[2].Value.Trim();
            string loopBody = match.Groups[3].Value;

            // Получаем коллекцию из корневой модели
            IEnumerable<object>? items = GetEnumerable(rootModel, collectionExpr);

            if (items != null)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in items)
                {
                    string processedBody = ReplaceIteratorPrefix(loopBody, iteratorName);
                    
                    processedBody = ProcessLoops(processedBody, item);
                    processedBody = ProcessConditions(processedBody, item);
                    processedBody = ProcessVariables(processedBody, item);
                    
                    sb.Append(processedBody);
                }
                
                text = text.Remove(match.Index, match.Length).Insert(match.Index, sb.ToString());
            }
            else
            {
                text = text.Remove(match.Index, match.Length);
            }
        }
        return text;
    }

    private string ReplaceIteratorPrefix(string body, string prefix)
    {
        return body.Replace("${" + prefix + ".", "${");
    }

    private IEnumerable<object>? GetEnumerable(object model, string path)
    {
        var val = GetValue(model, path);
        if (val is System.Collections.IEnumerable enumerable && !(val is string))
        {
            return enumerable.Cast<object>();
        }
        return null;
    }

    private string ProcessConditions(string text, object model)
    {
        MatchCollection matches = IfBlockRegex.Matches(text);
        for (int i = matches.Count - 1; i >= 0; i--)
        {
            var match = matches[i];
            string conditionStr = match.Groups[1].Value.Trim();
            string trueBranch = match.Groups[2].Value;
            string falseBranch = match.Groups[3].Success ? match.Groups[3].Value : "";

            bool condResult = EvaluateCondition(conditionStr, model);
            
            string replacement = condResult ? trueBranch : falseBranch;
            
            replacement = ProcessLoops(replacement, model);
            replacement = ProcessConditions(replacement, model);
            replacement = ProcessVariables(replacement, model);

            text = text.Remove(match.Index, match.Length).Insert(match.Index, replacement);
        }
        return text;
    }

    private bool EvaluateCondition(string expr, object model)
    {
        var opMatch = Regex.Match(expr, @"^(.+?)\s*(>=|<=|==|!=|>|<)\s*(.+)$");
        if (!opMatch.Success)
        {
            var val = GetValue(model, expr.Trim());
            if (val is bool b) return b;
            return val != null; 
        }

        string leftSide = opMatch.Groups[1].Value.Trim();
        string op = opMatch.Groups[2].Value;
        string rightSideRaw = opMatch.Groups[3].Value.Trim();

        object? leftVal = GetValue(model, leftSide);
        
        object? rightVal = IsLiteral(rightSideRaw) ? ParseLiteral(rightSideRaw) : GetValue(model, rightSideRaw);

        if (leftVal == null || rightVal == null) return false;

        try 
        {
            if (leftVal is IComparable comparableLeft)
            {
                int cmp = comparableLeft.CompareTo(Convert.ChangeType(rightVal, leftVal.GetType()));
                return op switch
                {
                    "==" => cmp == 0,
                    "!=" => cmp != 0,
                    ">"  => cmp > 0,
                    "<"  => cmp < 0,
                    ">=" => cmp >= 0,
                    "<=" => cmp <= 0,
                    _ => false
                };
            }
        }
        catch { /* Ignoring conversion errors */ }

        return Equals(leftVal, rightVal);
    }

    private bool IsLiteral(string val)
    {
        if (double.TryParse(val, out _)) return true;
        if ((val.StartsWith("\"") && val.EndsWith("\"")) || (val.StartsWith("'") && val.EndsWith("'"))) return true;
        if (bool.TryParse(val, out _)) return true;
        return false;
    }

    private object ParseLiteral(string val)
    {
        if (double.TryParse(val, out double d)) return d;
        if (bool.TryParse(val, out bool b)) return b;
        if ((val.StartsWith("\"") && val.EndsWith("\"")) || (val.StartsWith("'") && val.EndsWith("'")))
            return val.Substring(1, val.Length - 2);
        return val;
    }

    private string ProcessVariables(string text, object model)
    {
        return VariableRegex.Replace(text, match =>
        {
            string path = match.Groups[1].Value.Trim();
            object? value = GetValue(model, path);
            return value?.ToString() ?? "";
        });
    }

    private object? GetValue(object? obj, string propertyPath)
    {
        if (obj == null || string.IsNullOrEmpty(propertyPath)) return null;

        string[] parts = propertyPath.Split('.');
        object? current = obj;

        foreach (var part in parts)
        {
            if (current == null) return null;
            
            Type type = current.GetType();
            
            // Ищем свойство
            PropertyInfo? prop = type.GetProperty(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop != null)
            {
                current = prop.GetValue(current);
                continue;
            }

            FieldInfo? field = type.GetField(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (field != null)
            {
                current = field.GetValue(current);
                continue;
            }
            
            return null;
        }

        return current;
    }

    #endregion
}