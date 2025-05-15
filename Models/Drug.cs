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
        public int DrugClassId { get; set; }
        public DrugClass? DrugClass { get; set; }
        public int DrugClassV2Id { get; set; }
        public DrugClassV2? DrugClassV2 { get; set; }
        public decimal ACQ { get; set; }
        public decimal AWP { get; set; }
        public decimal? Rxcui { get; set; }
        public string? Route { get; set; }
        public string? TECode { get; set; }
        public string? Ingrdient { get; set; }
        public string? ApplicationNumber { get; set; }
        public string? ApplicationType { get; set; }
        public string? StrengthUnit { get; set; }
        public string? Type { get; set; }
    }
}