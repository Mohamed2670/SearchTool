namespace SearchTool_ServerSide.Models
{
    public class Insurance
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        public string? Description { get; set; }
        public string? Bin { get; set; }
        public string? Pcn { get; set; }
        public string? HelpDeskNumber { get; set; }
    }

}