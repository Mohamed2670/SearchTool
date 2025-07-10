using ServerSide.Model;

namespace SearchTool_ServerSide.Models
{
    public class ClassType : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<ClassInfo> ClassInfos { get; set; } 
        public ICollection<MainCompany> MainCompanies { get; set; }
    }
}