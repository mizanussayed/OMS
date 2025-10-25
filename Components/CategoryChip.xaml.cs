using System.Windows.Input;

namespace OMS.Components;

public partial class CategoryChip : ContentView
{
 public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(CategoryChip));
 public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(CategoryChip));
 public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(CategoryChip));
 public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(CategoryChip));

 public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
 public bool IsSelected { get => (bool)GetValue(IsSelectedProperty); set => SetValue(IsSelectedProperty, value); }
 public ICommand Command { get => (ICommand)GetValue(CommandProperty); set => SetValue(CommandProperty, value); }
 public object CommandParameter { get => GetValue(CommandParameterProperty); set => SetValue(CommandParameterProperty, value); }

 public CategoryChip() 
 { 
  InitializeComponent();
 }
}
