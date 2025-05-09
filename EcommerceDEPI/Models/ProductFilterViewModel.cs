using EcommerceDEPI.Models;

public class ProductFilterViewModel
{
    public List<Product> Products { get; set; }  // المنتجات بعد الفلترة
    public List<string> Categories { get; set; } // كل  
    public string SelectedCategory { get; set; } //    
    public decimal? MinPrice { get; set; }       //   
    public decimal? MaxPrice { get; set; }       //   
}
