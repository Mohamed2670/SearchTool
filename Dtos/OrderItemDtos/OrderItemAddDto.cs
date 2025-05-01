namespace SearchTool_ServerSide.Dtos.OrderItemDtos
{
    public class OrderItemAddDto
    {
        public int DrugId { get; set; }
        public decimal NetPrice { get; set; }
        public decimal PatientPay { get; set; }
        public decimal InsurancePay { get; set; }
        public decimal AcquisitionCost { get; set; }
        public decimal AdditionalCost { get; set; }
        public int InsuranceRxId { get; set; }
        public int Amount { get; set; }
    }
}