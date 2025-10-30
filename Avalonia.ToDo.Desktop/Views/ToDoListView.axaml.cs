using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;

namespace Avalonia.ToDo.Desktop.Views;

public partial class ToDoListView : UserControl
{
    public ToDoListView()
    {
        InitializeComponent();
        
        var style = new StyleInclude(new Uri("avares://Avalonia.ToDo.Desktop/"))
        {
            Source = new Uri("avares://Avalonia.ToDo.Desktop/Styles/CustomButtonTheme.axaml")
        };
        Resources.MergedDictionaries.Add(style);
    }
}