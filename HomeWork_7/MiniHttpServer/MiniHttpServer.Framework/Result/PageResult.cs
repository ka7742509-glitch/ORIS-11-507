using System.Net;
using MiniHttpServer.HtmlTemplating;

namespace MiniHttpServer.Framework.Results;

public sealed class PageResult : IHttpResult
{
    private readonly string _templatePath;
    private readonly object _model;
    private readonly int _statusCode;
    private readonly IHtmlTemplateRenderer _renderer;

    public PageResult(string templatePath, object model, int statusCode = 200, IHtmlTemplateRenderer? renderer = null)
    {
        _templatePath = templatePath;
        _model = model;
        _statusCode = statusCode;
        _renderer = renderer ?? new HtmlTemplateRenderer();
    }

    public async Task ExecuteAsync(HttpListenerResponse response, CancellationToken cancellationToken = default)
    {
        string fullPath = Path.Combine(AppContext.BaseDirectory, "Templates", _templatePath);
        string html = _renderer.RenderFromFile(fullPath, _model);

        await new HtmlResult(html, _statusCode).ExecuteAsync(response, cancellationToken);
    }
}