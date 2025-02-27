namespace SearchTool_ServerSide.Models
{
    public class Specialty
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public ICollection<MainCompany> MainCompanies { get; set; }
    }
}