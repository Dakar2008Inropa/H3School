namespace ThirdWebApi.Models.Dtos
{
    public class ProductCreateDto
    {
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int StockQuantity { get; set; }
        public string SupplierName { get; set; }
        public decimal Weight { get; set; }
    }
}