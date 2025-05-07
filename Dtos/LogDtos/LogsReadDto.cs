namespace SearchTool_ServerSide.Dtos.LogDtos
{
    public class LogsReadDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public DateTime Date { get; set; }
    }
}