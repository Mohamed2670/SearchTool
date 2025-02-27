namespace SearchTool_ServerSide.Models
{
    public class MainCompany
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int SpecialtyId { get; set; }
        public ICollection<Branch> Branches { get; set; }
        public Specialty Specialty { get; set; }

    }
}