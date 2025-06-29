namespace SearchTool_ServerSide.Models
{
    public class DrugEPCMOA
    {
        public int Id { get; set; }
        public int DrugId { get; set; }
        public int EPCMOAClassId { get; set; }
        public Drug Drug { get; set; }
        public EPCMOAClass EPCMOAClass { get; set; }
    }
}