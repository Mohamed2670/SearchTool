namespace SearchTool_ServerSide.Dtos.DrugDtos
{
    public class DrugModal
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Ndc { get; set; }
        public string Form { get; set; }
        public string Strength { get; set; }
        public int ClassId { get; set; }
        public string ClassType { get; set; }
        public string ClassName { get; set; }
        public decimal Acq { get; set; }
        public decimal Awp { get; set; }
        public decimal Rxcui { get; set; }
        public string Route { get; set; }
        public string TeCode { get; set; }
        public string Ingrdient { get; set; }
        public string ApplicationNumber { get; set; }
        public string ApplicationType { get; set; }
        public string StrengthUnit { get; set; }
        public string Type { get; set; }
    }

}