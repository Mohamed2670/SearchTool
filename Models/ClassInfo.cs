using ServerSide.Model;

namespace SearchTool_ServerSide.Models
{
    public class ClassInfo : IEntity
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int ClassTypeId { get; set; }
        public ClassType ClassType { get; set; }
        public ICollection<DrugClass> DrugClasses { get; set; } = new List<DrugClass>();
    }
}