using System.Net;
using System.Text;

namespace samostoyatelnaya_rabota
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Priority { get; set; } = "low";
        public string Status { get; set; } = "new";
        
        public int PrioRank => Priority switch 
        { 
            "high" => 0, 
            "medium" => 1, 
            _ => 2 
        };
    }

    class Program
    {
        static List<TaskItem> _tasks = new List<TaskItem>();
        static int _nextId = 1;

        static async Task Main(string[] args)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8080/");
            listener.Start();
            Console.WriteLine("Server running on http://localhost:8080/tasks");
            Console.WriteLine("Press Ctrl+C to stop.");

            while (true)
            {
                try
                {
                    var context = await listener.GetContextAsync();
                    HandleRequest(context);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        static string Enc(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }

        static void HandleRequest(HttpListenerContext ctx)
        {
            var req = ctx.Request;
            var resp = ctx.Response;
            string path = req.Url.AbsolutePath.ToLower();
            string method = req.HttpMethod.ToUpper();

            try
            {
                if (path == "/tasks" && method == "GET")
                {
                    HandleGetTasks(req, resp);
                    return;
                }

                if (path == "/tasks/details" && method == "GET")
                {
                    HandleGetDetails(req, resp);
                    return;
                }

                if (path == "/tasks/create" && method == "POST")
                {
                    HandleCreateTask(req, resp);
                    return;
                }

                if (path.StartsWith("/tasks/update") && method == "PUT")
                {
                    HandleUpdateTask(req, resp);
                    return;
                }

                SendText(resp, 404, "Not Found");
            }
            catch (Exception ex)
            {
                SendText(resp, 500, $"Internal Server Error: {ex.Message}");
            }
            finally
            {
                resp.Close();
            }
        }

        static void HandleGetTasks(HttpListenerRequest req, HttpListenerResponse resp)
        {
            string filterPriority = req.QueryString["priority"] ?? "";
            
            IEnumerable<TaskItem> list = _tasks;
            
            if (!string.IsNullOrEmpty(filterPriority))
                list = list.Where(t => t.Priority == filterPriority);

            list = list.OrderBy(t => t.PrioRank).ThenByDescending(t => t.Id);

            string html = RenderList(list.ToList(), filterPriority);
            SendHtml(resp, 200, html);
        }

        static void HandleGetDetails(HttpListenerRequest req, HttpListenerResponse resp)
        {
            if (!int.TryParse(req.QueryString["id"], out int id))
            {
                SendText(resp, 400, "Bad Request: Invalid ID");
                return;
            }

            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
            {
                SendText(resp, 404, "Not Found: Task does not exist");
                return;
            }

            string html = RenderDetail(task);
            SendHtml(resp, 200, html);
        }

        static void HandleCreateTask(HttpListenerRequest req, HttpListenerResponse resp)
        {
            using var reader = new StreamReader(req.InputStream);
            string body = reader.ReadToEnd();
            var form = System.Web.HttpUtility.ParseQueryString(body);

            string title = form["title"] ?? "";
            string priority = form["priority"] ?? "low";

            if (title.Length < 3 || title.Length > 80)
            {
                SendText(resp, 400, "Bad Request: Title must be 3-80 chars");
                return;
            }
            if (priority != "low" && priority != "medium" && priority != "high")
            {
                SendText(resp, 400, "Bad Request: Invalid Priority");
                return;
            }

            var newTask = new TaskItem { Id = _nextId++, Title = title, Priority = priority };
            _tasks.Add(newTask);

            resp.StatusCode = 303;
            resp.Headers["Location"] = $"/tasks/details?id={newTask.Id}";
        }

        static void HandleUpdateTask(HttpListenerRequest req, HttpListenerResponse resp)
        {
            if (!int.TryParse(req.QueryString["id"], out int id))
            {
                SendText(resp, 400, "Bad Request: Invalid ID");
                return;
            }

            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
            {
                SendText(resp, 404, "Not Found: Task does not exist");
                return;
            }

            using var reader = new StreamReader(req.InputStream);
            string body = reader.ReadToEnd();
            var form = System.Web.HttpUtility.ParseQueryString(body);

            string? newTitle = form["title"];
            string? newPriority = form["priority"];
            string? newStatus = form["status"];

            if (!string.IsNullOrEmpty(newTitle))
            {
                if (newTitle.Length < 3 || newTitle.Length > 80)
                {
                    SendText(resp, 400, "Bad Request: Title invalid length");
                    return;
                }
                task.Title = newTitle;
            }

            if (!string.IsNullOrEmpty(newPriority))
            {
                if (newPriority != "low" && newPriority != "medium" && newPriority != "high")
                {
                    SendText(resp, 400, "Bad Request: Invalid Priority");
                    return;
                }
                task.Priority = newPriority;
            }

            if (!string.IsNullOrEmpty(newStatus))
            {
                if (newStatus != "new" && newStatus != "completed")
                {
                    SendText(resp, 400, "Bad Request: Invalid Status");
                    return;
                }
                task.Status = newStatus;
            }

            resp.StatusCode = 204;
        }

        static string RenderList(List<TaskItem> tasks, string currentFilter)
        {
            var sb = new StringBuilder();
            sb.Append("<html><head><style>body{font-family:sans-serif;margin:20px;} table{width:100%;border-collapse:collapse;} td,th{border:1px solid #ccc;padding:8px;text-align:left;} .btn{padding:5px 10px;background:#007bff;color:white;border:none;cursor:pointer;}</style></head><body>");
            
            sb.Append("<h1>Tasks</h1>");
            
            sb.Append("<form action='/tasks' method='GET'>");
            sb.Append("<label>Filter: </label><select name='priority'>");
            sb.Append($"<option value='' {(currentFilter==""?"selected":"")}>All</option>");
            sb.Append($"<option value='high' {(currentFilter=="high"?"selected":"")}>High</option>");
            sb.Append($"<option value='medium' {(currentFilter=="medium"?"selected":"")}>Medium</option>");
            sb.Append($"<option value='low' {(currentFilter=="low"?"selected":"")}>Low</option>");
            sb.Append("</select> <button type='submit' class='btn'>Go</button></form><br/>");

            sb.Append("<table><tr><th>ID</th><th>Title</th><th>Prio</th><th>Status</th><th>Action</th></tr>");
            
            if (tasks.Count == 0)
            {
                sb.Append("<tr><td colspan='5' style='text-align:center;color:#666;'>Нет задач. Создайте первую!</td></tr>");
            }
            else
            {
                foreach (var t in tasks)
                {
                    sb.Append($"<tr><td>{t.Id}</td><td>{Enc(t.Title)}</td><td>{t.Priority}</td><td>{t.Status}</td>");
                    sb.Append($"<td><a href='/tasks/details?id={t.Id}' class='btn'>Edit</a></td></tr>");
                }
            }
            sb.Append("</table>");

            sb.Append("<hr/><h2>Create New</h2>");
            sb.Append("<form action='/tasks/create' method='POST'>");
            sb.Append("<input type='text' name='title' placeholder='Title (3-80 chars)' required minlength='3' maxlength='80'/><br/>");
            sb.Append("<select name='priority'><option value='low'>Low</option><option value='medium'>Medium</option><option value='high'>High</option></select><br/>");
            sb.Append("<button type='submit' class='btn'>Create</button>");
            sb.Append("</form>");
            sb.Append("</body></html>");
            return sb.ToString();
        }

        static string RenderDetail(TaskItem task)
        {
            var sb = new StringBuilder();
            sb.Append("<html><head><style>body{font-family:sans-serif;margin:20px;} label{display:block;margin-top:10px;} input,select{width:100%;padding:5px;}</style></head><body>");
            
            sb.Append($"<h1>Edit Task #{task.Id}</h1>");
            sb.Append("<form id='editForm'>");
            sb.Append($"<label>Title:</label><input type='text' name='title' value='{Enc(task.Title)}'/>");
            sb.Append($"<label>Priority:</label><select name='priority'>");
            sb.Append($"<option value='low' {(task.Priority=="low"?"selected":"")}>Low</option>");
            sb.Append($"<option value='medium' {(task.Priority=="medium"?"selected":"")}>Medium</option>");
            sb.Append($"<option value='high' {(task.Priority=="high"?"selected":"")}>High</option>");
            sb.Append("</select>");
            sb.Append($"<label>Status:</label><select name='status'>");
            sb.Append($"<option value='new' {(task.Status=="new"?"selected":"")}>New</option>");
            sb.Append($"<option value='completed' {(task.Status=="completed"?"selected":"")}>Completed</option>");
            sb.Append("</select>");
            sb.Append("</form>");
            
            sb.Append("<br/><a href='/tasks'>Back to List</a>");
            sb.AppendLine("<hr/>");
            sb.Append("</body></html>");
            return sb.ToString();
        }

        static void SendHtml(HttpListenerResponse resp, int code, string html)
        {
            byte[] buf = Encoding.UTF8.GetBytes(html);
            resp.StatusCode = code;
            resp.ContentType = "text/html; charset=utf-8";
            resp.ContentLength64 = buf.Length;
            resp.OutputStream.Write(buf, 0, buf.Length);
        }

        static void SendText(HttpListenerResponse resp, int code, string msg)
        {
            byte[] buf = Encoding.UTF8.GetBytes(msg);
            resp.StatusCode = code;
            resp.ContentType = "text/plain; charset=utf-8";
            resp.ContentLength64 = buf.Length;
            resp.OutputStream.Write(buf, 0, buf.Length);
        }
    }
}