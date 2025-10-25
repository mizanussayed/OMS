using System.Windows.Input;

namespace OMS.Components;

public partial class ProductCard : ContentView
{
 public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(ProductCard), default(string));
 public static readonly BindableProperty PriceProperty = BindableProperty.Create(nameof(Price), typeof(string), typeof(ProductCard), default(string));
 public static readonly BindableProperty ImageUrlProperty = BindableProperty.Create(nameof(ImageUrl), typeof(string), typeof(ProductCard), default(string));
 public static readonly BindableProperty AddToCartCommandProperty = BindableProperty.Create(nameof(AddToCartCommand), typeof(ICommand), typeof(ProductCard));

 public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
 public string Price { get => (string)GetValue(PriceProperty); set => SetValue(PriceProperty, value); }
 public string ImageUrl { get => (string)GetValue(ImageUrlProperty); set => SetValue(ImageUrlProperty, value); }
 public ICommand AddToCartCommand { get => (ICommand)GetValue(AddToCartCommandProperty); set => SetValue(AddToCartCommandProperty, value); }

 public ProductCard() 
 { 
  InitializeComponent();
  BindingContext = this;
 }
}
