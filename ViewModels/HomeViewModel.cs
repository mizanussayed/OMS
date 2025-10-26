using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OMS.Models;
using OMS.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace OMS.ViewModels;

public partial class HomeViewModel : ObservableObject
{
 private readonly IDataService _dataService;

 public HomeViewModel()
 {
  _dataService = new MockDataService();
 }

 [ObservableProperty]
 private ObservableCollection<string> categories = new() { "All", "Men", "Women", "Kids" };

 [ObservableProperty]
 private ObservableCollection<Cloth> cloths = new();

 [ObservableProperty]
 private string selectedCategory = "All";

 [RelayCommand]
 private async Task LoadData()
 {
  var clothsList = await _dataService.GetClothsAsync();
  Cloths = new ObservableCollection<Cloth>(clothsList);
 }

 [RelayCommand]
 private void SelectCategory(string category)
 {
  SelectedCategory = category;
  // Filter cloths if needed
 }

 public ICommand AddToCartCommand => new RelayCommand<Cloth>(AddToCart);

 private void AddToCart(Cloth cloth)
 {
  // Add to cart logic
 }
}