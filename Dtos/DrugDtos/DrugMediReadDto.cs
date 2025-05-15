namespace SearchTool_ServerSide.Dtos.DrugDtos
{
    public class DrugMediReadDto
    {
        public int DrugId { get; set; }
        public string DrugNDC { get; set; }
        public string DrugName { get; set; }
        public string PriorAuthorization { get; set; }
        public string ExtendedDuration { get; set; }
        public string CostCeilingTier { get; set; }
        public string NonCapitatedDrugIndicator { get; set; }
        public string CCSPanelAuthority { get; set; }
    }
}