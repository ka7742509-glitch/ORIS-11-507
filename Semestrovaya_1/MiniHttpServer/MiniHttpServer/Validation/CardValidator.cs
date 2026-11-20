using System.Text.RegularExpressions;

namespace MiniHttpServer.Validation;

public static class CardValidator
{
    private static readonly Regex NamePattern = new(@"^[A-Za-z0-9\s\-',.]{3,100}$", RegexOptions.Compiled);
    private static readonly Regex ManaCostPattern = new(@"^(\d|[WUBRGCwubrgc]){0,10}$", RegexOptions.Compiled);

    public static ValidationResult ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new ValidationResult(false, "Название карты обязательно.");

        if (!NamePattern.IsMatch(name))
            return new ValidationResult(false, "Название: 3–100 символов, только буквы, цифры, пробелы и дефисы.");

        return new ValidationResult(true);
    }

    public static ValidationResult ValidateManaCost(string? manaCost)
    {
        if (string.IsNullOrWhiteSpace(manaCost))
            return new ValidationResult(true);

        if (!ManaCostPattern.IsMatch(manaCost))
            return new ValidationResult(false, "Мана-стоимость: только цифры и символы WUBRGC.");

        return new ValidationResult(true);
    }

    public static ValidationResult ValidateTypeLine(string typeLine)
    {
        if (string.IsNullOrWhiteSpace(typeLine))
            return new ValidationResult(false, "Тип карты обязателен.");

        if (typeLine.Length > 200)
            return new ValidationResult(false, "Тип карты: не более 200 символов.");

        return new ValidationResult(true);
    }
}

public sealed class ValidationResult
{
    public bool IsValid { get; }
    public string? ErrorMessage { get; }

    public ValidationResult(bool isValid, string? errorMessage = null)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }
}