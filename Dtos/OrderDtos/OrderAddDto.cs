namespace SearchTool_ServerSide.Dtos.OrderDtos
{
    public class OrderAddDto
    {
        
        public string UserEmail { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalNet { get; set; }= 0;
        public decimal TotalPatientPay { get; set; } =  0;
        public decimal TotalInsurancePay { get; set; }=  0;
        public decimal TotalAcquisitionCost { get; set; } = 0;
        public decimal AdditionalCost { get; set; } = 0;
    }
}