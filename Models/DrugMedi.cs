using ServerSide.Model;

namespace SearchTool_ServerSide.Models
{
    public class DrugMedi : IEntity
    {
        public int Id { get; set; }
        public int DrugId { get; set; }
        public string DrugNDC { get; set; }
        public Drug? Drug { get; set; }
        public string PriorAuthorization { get; set; }
        public string ExtendedDuration { get; set; }
        public string CostCeilingTier { get; set; }
        public string NonCapitatedDrugIndicator { get; set; }
        public string CCSPanelAuthority { get; set; }


    }
}