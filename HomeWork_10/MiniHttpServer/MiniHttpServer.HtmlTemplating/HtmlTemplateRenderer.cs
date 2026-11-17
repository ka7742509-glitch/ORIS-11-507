using System.Collections;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace MiniHttpServer.HtmlTemplating;

public sealed class HtmlTemplateRenderer : IHtmlTemplateRenderer
{
    private static readonly Regex VariableRegex = new(@"\$\{([^}]+)\}", RegexOptions.Compiled);
    private static readonly Regex ConditionRegex = new(@"^(.+?)\s*(>=|<=|==|!=|>|<)\s*(.+)$", RegexOptions.Compiled);
    private static readonly Regex ForeachHeaderRegex = new(@"^\s*var\s+(\w+)\s+in\s+([A-Za-z_][\w.]*)\s*$", RegexOptions.Compiled);
    
    public string RenderFromString(string htmlTemplate, object dataModel)
    {
        if (string.IsNullOrEmpty(htmlTemplate))
            return string.Empty;

        return RenderBlock(htmlTemplate, new Scope(dataModel));
    }

    public string RenderFromFile(string filePath, object dataModel)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Файл шаблона не найден: {filePath}");

        string template = File.ReadAllText(filePath);
        return RenderFromString(template, dataModel);
    }

    public string RenderToFile(string inputFilePath, string outputFilePath, object dataModel)
    {
        string result = RenderFromFile(inputFilePath, dataModel);

        string? directory = Path.GetDirectoryName(outputFilePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(outputFilePath, result);
        return result;
    }

    private string RenderBlock(string text, Scope scope)
    {
        var output = new StringBuilder();
        int position = 0;

        while (position < text.Length)
        {
            int ifIndex = text.IndexOf("$if(", position, StringComparison.Ordinal);
            int forIndex = text.IndexOf("$foreach(", position, StringComparison.Ordinal);

            int nextIndex;
            if (ifIndex == -1) nextIndex = forIndex;
            else if (forIndex == -1) nextIndex = ifIndex;
            else nextIndex = Math.Min(ifIndex, forIndex);

            if (nextIndex == -1)
            {
                output.Append(text.AsSpan(position));
                break;
            }

            output.Append(text.AsSpan(position, nextIndex - position));

            if (nextIndex == ifIndex)
            {
                var (conditionEnd, elseIndex, blockEnd) = ParseIf(text, nextIndex);
                string condition = text.Substring(nextIndex + 4, conditionEnd - nextIndex - 4);

                int trueEnd = elseIndex >= 0 ? elseIndex : blockEnd - 6;
                string trueBranch = text.Substring(conditionEnd + 1, trueEnd - conditionEnd - 1);
                string falseBranch = elseIndex >= 0
                    ? text.Substring(elseIndex + 5, blockEnd - 6 - elseIndex - 5)
                    : string.Empty;

                string branch = EvaluateCondition(condition, scope) ? trueBranch : falseBranch;
                output.Append(RenderBlock(branch, scope));
                position = blockEnd;
            }
            else
            {
                var (headerEnd, blockEnd) = ParseForeach(text, nextIndex);
                string header = text.Substring(nextIndex + 9, headerEnd - nextIndex - 9);

                Match headerMatch = ForeachHeaderRegex.Match(header);
                if (!headerMatch.Success)
                    throw new TemplateRenderException($"Некорректная конструкция $foreach: '{header}'. Ожидается 'var имя in Коллекция'.");

                string iteratorName = headerMatch.Groups[1].Value;
                string collectionPath = headerMatch.Groups[2].Value;
                string body = text.Substring(headerEnd + 1, blockEnd - 7 - headerEnd - 1);

                object? collectionValue = ResolvePath(collectionPath, scope);

                if (collectionValue is null)
                {
                    position = blockEnd;
                    continue;
                }

                if (collectionValue is string || collectionValue is not IEnumerable enumerable)
                    throw new TemplateRenderException($"Выражение '{collectionPath}' не является коллекцией.");

                foreach (object? item in enumerable)
                {
                    output.Append(RenderBlock(body, scope.Child(iteratorName, item)));
                }

                position = blockEnd;
            }
        }

        return SubstituteVariables(output.ToString(), scope);
    }

    private static (int conditionEnd, int elseIndex, int blockEnd) ParseIf(string text, int start)
    {
        int conditionEnd = text.IndexOf(')', start + 4);
        if (conditionEnd == -1)
            throw new TemplateRenderException("В конструкции $if не закрыта скобка условия.");

        int depth = 1;
        int elseIndex = -1;
        int position = conditionEnd + 1;

        while (position < text.Length)
        {
            if (StartsWith(text, position, "$if("))
            {
                depth++;
                position += 4;
                continue;
            }

            if (StartsWith(text, position, "$endif"))
            {
                depth--;
                if (depth == 0)
                    return (conditionEnd, elseIndex, position + 6);
                position += 6;
                continue;
            }

            if (depth == 1 && StartsWith(text, position, "$else"))
            {
                elseIndex = position;
                position += 5;
                continue;
            }

            position++;
        }

        throw new TemplateRenderException("Отсутствует закрывающий $endif.");
    }

    private static (int headerEnd, int blockEnd) ParseForeach(string text, int start)
    {
        int headerEnd = text.IndexOf(')', start + 9);
        if (headerEnd == -1)
            throw new TemplateRenderException("В конструкции $foreach не закрыта скобка заголовка.");

        int depth = 1;
        int position = headerEnd + 1;

        while (position < text.Length)
        {
            if (StartsWith(text, position, "$foreach("))
            {
                depth++;
                position += 9;
                continue;
            }
            
            if (StartsWith(text, position, "$endfor"))
            {
                depth--;
                if (depth == 0)
                    return (headerEnd, position + 7);
                position += 7;
                continue;
            }

            position++;
        }

        throw new TemplateRenderException("Отсутствует закрывающий $endfor.");
    }

    private static bool StartsWith(string text, int position, string token)
    {
        return position + token.Length <= text.Length
            && string.CompareOrdinal(text, position, token, 0, token.Length) == 0;
    }

    private string SubstituteVariables(string text, Scope scope)
    {
        return VariableRegex.Replace(text, match =>
        {
            object? value = ResolvePath(match.Groups[1].Value.Trim(), scope);
            if (value is null)
                return string.Empty;

            return WebUtility.HtmlEncode(value.ToString() ?? string.Empty);
        });
    }

    private object? ResolvePath(string path, Scope scope)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new TemplateRenderException("Пустое имя переменной в конструкции ${...}.");

        if (IsScalar(scope.Model))
            return scope.Model;

        string[] parts = path.Split('.');
        object? current;
        int startIndex;

        if (scope.TryResolve(parts[0], out current))
        {
            startIndex = 1;
        }
        else
        {
            current = scope.Model;
            startIndex = 0;
        }

        for (int i = startIndex; i < parts.Length; i++)
        {
            if (current is null)
                return null;

            Type type = current.GetType();

            if (IsScalar(type))
                throw new TemplateRenderException($"Свойство '{parts[i]}' не найдено у типа '{type.Name}'.");

            PropertyInfo? property = type.GetProperty(parts[i], BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property is not null)
            {
                current = property.GetValue(current);
                continue;
            }

            FieldInfo? field = type.GetField(parts[i], BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (field is not null)
            {
                current = field.GetValue(current);
                continue;
            }

            throw new TemplateRenderException($"Свойство '{parts[i]}' не найдено у типа '{type.Name}'.");
        }

        return current;
    }

    private bool EvaluateCondition(string condition, Scope scope)
    {
        Match match = ConditionRegex.Match(condition.Trim());

        if (!match.Success)
        {
            object? value = ResolvePath(condition.Trim(), scope);
            return IsTruthy(value);
        }

        object? left = ResolvePath(match.Groups[1].Value.Trim(), scope);
        string rightRaw = match.Groups[3].Value.Trim();
        object? right = IsLiteral(rightRaw) ? ParseLiteral(rightRaw) : ResolvePath(rightRaw, scope);

        int comparison = Compare(left, right);

        return match.Groups[2].Value switch
        {
            "==" => comparison == 0,
            "!=" => comparison != 0,
            ">" => comparison > 0,
            "<" => comparison < 0,
            ">=" => comparison >= 0,
            "<=" => comparison <= 0,
            _ => false
        };
    }

    private static bool IsTruthy(object? value)
    {
        return value switch
        {
            null => false,
            bool b => b,
            string s => s.Length > 0,
            _ => Convert.ToDouble(value, CultureInfo.InvariantCulture) != 0
        };
    }

    private static bool IsLiteral(string raw)
    {
        if (raw.Length >= 2 && ((raw[0] == '"' && raw[^1] == '"') || (raw[0] == '\'' && raw[^1] == '\'')))
            return true;

        return double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out _)
            || bool.TryParse(raw, out _);
    }

    private static object ParseLiteral(string raw)
    {
        if (raw.Length >= 2 && (raw[0] == '"' || raw[0] == '\''))
            return raw[1..^1];

        if (bool.TryParse(raw, out bool b))
            return b;

        if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int i))
            return i;

        return double.Parse(raw, NumberStyles.Any, CultureInfo.InvariantCulture);
    }

    private static int Compare(object? left, object? right)
    {
        if (left is null && right is null) return 0;
        if (left is null) return -1;
        if (right is null) return 1;

        if (left is string ls && right is string rs)
            return string.Compare(ls, rs, StringComparison.Ordinal);

        try
        {
            double l = Convert.ToDouble(left, CultureInfo.InvariantCulture);
            double r = Convert.ToDouble(right, CultureInfo.InvariantCulture);
            return l.CompareTo(r);
        }
        catch
        {
            return string.Compare(left.ToString(), right.ToString(), StringComparison.Ordinal);
        }
    }

    private static bool IsScalar(object value) => IsScalar(value.GetType());

    private static bool IsScalar(Type type)
    {
        return type.IsPrimitive
            || type.IsEnum
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(DateTime)
            || type == typeof(Guid);
    }

    private sealed class Scope
    {
        private readonly Scope? _parent;
        private readonly string? _name;
        private readonly object? _value;

        public object Model { get; }

        public Scope(object model)
        {
            Model = model;
        }

        private Scope(Scope parent, string name, object? value)
        {
            _parent = parent;
            _name = name;
            _value = value;
            Model = parent.Model;
        }

        public Scope Child(string name, object? value) => new(this, name, value);

        public bool TryResolve(string name, out object? value)
        {
            for (Scope? scope = this; scope is not null; scope = scope._parent)
            {
                if (scope._name == name)
                {
                    value = scope._value;
                    return true;
                }
            }

            value = null;
            return false;
        }
    }
}