using System.Collections.Specialized;

namespace MiniHttpServer.Framework.Routing;

public static class FormDataParser
{
    public static NameValueCollection Parse(string body)
    {
        return System.Web.HttpUtility.ParseQueryString(body);
    }

    public static NameValueCollection Merge(NameValueCollection query, NameValueCollection form)
    {
        var result = new NameValueCollection();
        foreach (string? key in query.AllKeys)
            if (key is not null) result[key] = query[key];
        foreach (string? key in form.AllKeys)
            if (key is not null) result[key] = form[key];
        return result;
    }
}