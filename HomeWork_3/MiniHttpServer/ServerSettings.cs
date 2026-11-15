namespace MiniHttpServer
{
    public class ServerSettings
    {
        public int Port { get; set; } = 8888;
        public string Path { get; set; } = "";
        public string PublicDirectoryPath { get; set; } = "static";
        public string DefaultPage { get; set; } = "index.html";
    
        public string BasePath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Path)) return "/";
                string p = Path.Trim('/');
                return "/" + p + "/";
            }
        }
    
        public string Prefix => $"http://localhost:{Port}{BasePath}";
    }
}
