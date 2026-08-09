using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GraphViewerApp.ViewModels;
using Microsoft.Win32;

namespace GraphViewerApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly GraphViewModel _viewModel;
    private bool _isWebViewInitialized = false;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new GraphViewModel();
        DataContext = _viewModel;

        InitializeWebView();
    }

    private async void InitializeWebView()
    {
        try
        {
            await GraphWebView.EnsureCoreWebView2Async();
            
            // Get the path to the wwwroot folder
            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var wwwrootPath = Path.Combine(appDirectory, "wwwroot");
            var indexPath = Path.Combine(wwwrootPath, "index.html");

            if (File.Exists(indexPath))
            {
                GraphWebView.Source = new Uri($"file:///{indexPath.Replace("\\", "/")}");
                _isWebViewInitialized = true;
                StatusText.Text = "Graph viewer initialized. Load data to begin.";
            }
            else
            {
                StatusText.Text = $"Error: Could not find index.html at {indexPath}";
                MessageBox.Show($"Could not find index.html at:\n{indexPath}\n\nPlease ensure the wwwroot folder is in the application directory.",
                    "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error initializing WebView2: {ex.Message}";
            MessageBox.Show($"Error initializing WebView2:\n{ex.Message}\n\nPlease ensure WebView2 Runtime is installed.",
                "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void LoadSampleData_Click(object sender, RoutedEventArgs e)
    {
        if (!_isWebViewInitialized) return;

        var sampleData = _viewModel.GenerateSampleData();
        await LoadGraphData(sampleData);
    }

    private async void LoadJsonFile_Click(object sender, RoutedEventArgs e)
    {
        if (!_isWebViewInitialized) return;

        var openFileDialog = new OpenFileDialog
        {
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            Title = "Load Graph Data"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                var json = await File.ReadAllTextAsync(openFileDialog.FileName);
                var graphData = JsonSerializer.Deserialize<GraphData>(json);
                
                if (graphData != null)
                {
                    await LoadGraphData(graphData);
                    StatusText.Text = $"Loaded: {Path.GetFileName(openFileDialog.FileName)}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading file:\n{ex.Message}", "Load Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async Task LoadGraphData(GraphData graphData)
    {
        try
        {
            _viewModel.CurrentGraphData = graphData;
            
            var json = JsonSerializer.Serialize(graphData, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });
            
            var script = $"loadGraphData({json});";
            await GraphWebView.ExecuteScriptAsync(script);

            NodeCountText.Text = graphData.Nodes.Count.ToString();
            EdgeCountText.Text = graphData.Edges.Count.ToString();
            StatusText.Text = $"Graph loaded: {graphData.Nodes.Count} nodes, {graphData.Edges.Count} edges";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading graph:\n{ex.Message}", "Load Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void LayoutComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isWebViewInitialized || _viewModel.CurrentGraphData == null) return;

        var layoutName = ((ComboBoxItem)LayoutComboBox.SelectedItem).Content.ToString();
        var layoutKey = layoutName switch
        {
            "Force Directed (CoSE)" => "cose",
            "Hierarchical" => "dagre",
            "Circular" => "circle",
            "Grid" => "grid",
            "Concentric" => "concentric",
            "Breadthfirst" => "breadthfirst",
            _ => "cose"
        };

        try
        {
            await GraphWebView.ExecuteScriptAsync($"changeLayout('{layoutKey}');");
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error changing layout: {ex.Message}";
        }
    }

    private async void FitToView_Click(object sender, RoutedEventArgs e)
    {
        if (!_isWebViewInitialized) return;
        await GraphWebView.ExecuteScriptAsync("fitToView();");
    }

    private async void ResetZoom_Click(object sender, RoutedEventArgs e)
    {
        if (!_isWebViewInitialized) return;
        await GraphWebView.ExecuteScriptAsync("resetZoom();");
    }

    private async void ExportPng_Click(object sender, RoutedEventArgs e)
    {
        if (!_isWebViewInitialized) return;

        var saveFileDialog = new SaveFileDialog
        {
            Filter = "PNG files (*.png)|*.png",
            Title = "Export Graph as PNG",
            FileName = "xlink-graph.png"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                await GraphWebView.ExecuteScriptAsync("exportPng();");
                StatusText.Text = "Export initiated (check browser download)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting:\n{ex.Message}", "Export Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void SearchTextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (!_isWebViewInitialized) return;

        var searchTerm = SearchTextBox.Text;
        var escapedTerm = searchTerm.Replace("'", "\\'").Replace("\"", "\\\"");
        await GraphWebView.ExecuteScriptAsync($"searchNodes('{escapedTerm}');");
    }
}
