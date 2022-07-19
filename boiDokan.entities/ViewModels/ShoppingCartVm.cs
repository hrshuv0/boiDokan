namespace boiDokan.entities.ViewModels;

public class ShoppingCartVm
{
    public IEnumerable<ShoppingCart>? CartList { get; set; }
    public double CartTotal { get; set; }
    public OrderHeader? OrderHeader { get; set; }
    
}