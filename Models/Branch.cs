namespace SearchTool_ServerSide.Models
{
    public class Branch
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Code { get; set; }
        public int MainCompanyId { get; set; }
        public MainCompany MainCompany { get; set; }
        public ICollection<User> Users { get; set; }
    }
}