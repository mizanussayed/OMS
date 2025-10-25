using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OMS.Models;
using OMS.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace OMS.ViewModels;

public partial class HomeViewModel : ObservableObject
{
 private readonly IProductService _productService;

 public HomeViewModel()
 {
  _productService = new MockProductService();
 }

 [ObservableProperty]
 private ObservableCollection<string> categories = new() { "All", "Men", "Women", "Kids" };

 [ObservableProperty]
 private ObservableCollection<Product> popularProducts = new();

 [ObservableProperty]
 private string selectedCategory = "All";

 [RelayCommand]
 private async Task LoadData()
 {
  var products = await _productService.GetPopularAsync();
  PopularProducts = new ObservableCollection<Product>(products);
 }

 [RelayCommand]
 private void SelectCategory(string category)
 {
  SelectedCategory = category;
  // Filter products if needed
 }

 public ICommand AddToCartCommand => new RelayCommand<Product>(AddToCart);

 private void AddToCart(Product product)
 {
  // Add to cart logic
 }
}