using ServerSide.Model;

namespace SearchTool_ServerSide.Models
{
    public class Order : IEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalNet { get; set; }
        public decimal TotalPatientPay { get; set; }
        public decimal TotalInsurancePay { get; set; }
        public decimal TotalAcquisitionCost { get; set; }
        public decimal AddtionalCost { get; set; }
        public User User { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    }
}