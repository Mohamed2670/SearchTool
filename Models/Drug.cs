using ServerSide.Model;

namespace SearchTool_ServerSide.Models
{
    public class Drug : IEntity
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string NDC { get; set; }
        public string? Form { get; set; }
        public string? Strength { get; set; }
        public int ClassId { get; set; }
        public DrugClass? DrugClass { get; set; }
        public decimal ACQ { get; set; }
        public decimal AWP { get; set; }
        public decimal? Rxcui { get; set; }
    }
}