using ServerSide.Model;

namespace SearchTool_ServerSide.Models
{
    public class MainCompany : IEntity
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int SpecialtyId { get; set; }
        public int? ClassTypeId { get; set; }
        public ClassType? ClassType { get; set; }
        public ICollection<Branch> Branches { get; set; }
        public Specialty Specialty { get; set; }

    }
}