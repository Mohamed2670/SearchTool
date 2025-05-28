namespace SearchTool_ServerSide.Dtos.LogDtos
{
    public class AllLogsAddDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}