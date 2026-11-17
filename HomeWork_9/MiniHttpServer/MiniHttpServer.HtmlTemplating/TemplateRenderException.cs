namespace MiniHttpServer.HtmlTemplating;

public sealed class TemplateRenderException : Exception
{
    public TemplateRenderException(string message) : base(message)
    {
    }
}