using SearchTool_ServerSide.Models;

namespace ServerSide.Models
{
    public class Script
    {
         public int Id { get; set; }
        public DateTime Date { get; set; }
        public string ScriptCode { get; set; }
        public string RxNumber { get; set; }
        public string? User { get; set; }
        public List<ScriptItem> ScriptItems { get; set; } = new List<ScriptItem>();

    }
}
