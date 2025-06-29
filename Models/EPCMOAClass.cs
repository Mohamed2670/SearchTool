namespace SearchTool_ServerSide.Models
{
    public class EPCMOAClass
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public ICollection<DrugEPCMOA> DrugEPCMOAs { get; set; } = new List<DrugEPCMOA>();
    }
    public class ClassInfo
    {
        public string ClassName { get; set; }
        public string ClassType { get; set; }
    }

}