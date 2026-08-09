using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using XLinkViewer.ViewModels;
using XLinkViewer.Models;

namespace XLinkViewer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
    
    private void DwgFilesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            var listBox = sender as System.Windows.Controls.ListBox;
            var selectedFiles = listBox?.SelectedItems.Cast<DwgFileInfo>().ToList() ?? new List<DwgFileInfo>();
            viewModel.UpdateSelectedFiles(selectedFiles);
        }
    }
    
    protected override void OnClosed(EventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.Dispose();
        }
        base.OnClosed(e);
    }
}