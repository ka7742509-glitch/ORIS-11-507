namespace MiniTemplateEngine;

/// <summary>
/// Интерфейс для рендера HTML-шаблонов.
/// </summary>
public interface IHtmlTemplateRenderer
{
    /// <summary>
    /// Рендерит строковый шаблон с данными модели.
    /// </summary>
    /// <param name="htmlTemplate">Строка с HTML-шаблоном.</param>
    /// <param name="dataModel">Объект с данными для подстановки.</param>
    /// <returns>Готовая HTML-строка.</returns>
    string RenderFromString(string htmlTemplate, object dataModel);

    /// <summary>
    /// Читает шаблон из файла и рендерит его.
    /// </summary>
    /// <param name="filePath">Путь к файлу шаблона.</param>
    /// <param name="dataModel">Объект с данными.</param>
    /// <returns>Готовая HTML-строка.</returns>
    string RenderFromFile(string filePath, object dataModel);

    /// <summary>
    /// Рендерит шаблон и сохраняет результат в файл.
    /// </summary>
    /// <param name="inputFilePath">Путь к исходному шаблону.</param>
    /// <param name="outputFilePath">Путь для сохранения результата.</param>
    /// <param name="dataModel">Объект с данными.</param>
    /// <returns>Готовая HTML-строка (для проверки).</returns>
    string RenderToFile(string inputFilePath, string outputFilePath, object dataModel);
}