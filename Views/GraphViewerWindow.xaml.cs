using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using XLinkViewer.Services;
using MessageBox = System.Windows.MessageBox;

namespace XLinkViewer.Views;

/// <summary>
/// Graph visualization window using WebView2 and Cytoscape.js
/// </summary>
public partial class GraphViewerWindow : Window
{
    private bool _isWebViewInitialized = false;
    private GraphData? _currentGraphData;

    public GraphViewerWindow()
    {
        InitializeComponent();
        Loaded += GraphViewerWindow_Loaded;
    }

    private async void GraphViewerWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await InitializeWebView();
    }

    private async Task InitializeWebView()
    {
        try
        {
            await GraphWebView.EnsureCoreWebView2Async();
            
            // Get the path to the wwwroot folder
            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var wwwrootPath = Path.Combine(appDirectory, "GraphViewer", "wwwroot");
            var indexPath = Path.Combine(wwwrootPath, "index.html");

            if (File.Exists(indexPath))
            {
                GraphWebView.Source = new Uri($"file:///{indexPath.Replace("\\", "/")}");
                _isWebViewInitialized = true;
                StatusText.Text = "Graph viewer ready";
            }
            else
            {
                StatusText.Text = $"Error: Could not find index.html at {indexPath}";
                MessageBox.Show($"Could not find index.html at:\n{indexPath}\n\nPlease ensure the GraphViewer/wwwroot folder is copied to the output directory.",
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

    /// <summary>
    /// Loads graph data and displays it
    /// </summary>
    public async Task LoadGraphData(GraphData graphData)
    {
        if (!_isWebViewInitialized)
        {
            // Wait a bit for WebView to initialize
            await Task.Delay(500);
        }

        if (!_isWebViewInitialized)
        {
            StatusText.Text = "Error: WebView not initialized";
            return;
        }

        try
        {
            _currentGraphData = graphData;
            
            var json = JsonSerializer.Serialize(graphData, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });
            
            var script = $"loadGraphData({json});";
            await GraphWebView.ExecuteScriptAsync(script);

            // Update initial node and edge counts
            NodeCountText.Text = graphData.Nodes.Count.ToString();
            EdgeCountText.Text = graphData.Edges.Count.ToString();
            StatusText.Text = $"Graph loaded: {graphData.Nodes.Count} nodes, {graphData.Edges.Count} edges";
            
            // Calculate and display statistics
            await Task.Delay(100); // Small delay to ensure graph is rendered
            await UpdateStatistics();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading graph:\n{ex.Message}", "Load Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            StatusText.Text = $"Error: {ex.Message}";
        }
    }

    private async void LayoutComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isWebViewInitialized || _currentGraphData == null) return;

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

        try
        {
            await GraphWebView.ExecuteScriptAsync("exportPng();");
            StatusText.Text = "PNG export initiated (check browser download)";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error exporting:\n{ex.Message}", "Export Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void SearchTextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (!_isWebViewInitialized) return;

        var searchTerm = SearchTextBox.Text;
        var escapedTerm = searchTerm.Replace("'", "\\'").Replace("\"", "\\\"");
        await GraphWebView.ExecuteScriptAsync($"searchNodes('{escapedTerm}');");
    }

    private async void HighlightMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isWebViewInitialized || HighlightModeComboBox == null) return;

        var mode = ((ComboBoxItem)HighlightModeComboBox.SelectedItem)?.Content.ToString()?.ToLowerInvariant();
        if (string.IsNullOrEmpty(mode)) return;

        try
        {
            await GraphWebView.ExecuteScriptAsync($"setHighlightMode('{mode}');");
            StatusText.Text = $"Highlight mode set to {mode}";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error setting highlight mode: {ex.Message}";
        }
    }

    private async void DisciplineFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isWebViewInitialized || DisciplineFilterComboBox == null) return;

        var discipline = ((ComboBoxItem)DisciplineFilterComboBox.SelectedItem)?.Content.ToString();
        if (string.IsNullOrEmpty(discipline)) return;

        try
        {
            await GraphWebView.ExecuteScriptAsync($"filterByDiscipline('{discipline}');");
            StatusText.Text = discipline == "All" ? "Filter cleared" : $"Filtered to {discipline}";
            await UpdateStatistics();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error applying filter: {ex.Message}";
        }
    }

    private async void ToggleTheme_Click(object sender, RoutedEventArgs e)
    {
        if (!_isWebViewInitialized) return;

        try
        {
            await GraphWebView.ExecuteScriptAsync("toggleTheme();");
            
            // Update button text based on current theme
            var currentTheme = await GraphWebView.ExecuteScriptAsync("currentTheme");
            currentTheme = currentTheme?.Trim('"');
            
            if (ThemeToggleButton != null)
            {
                ThemeToggleButton.Content = currentTheme == "dark" ? "☀️ Light" : "🌙 Dark";
            }
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error toggling theme: {ex.Message}";
        }
    }

    private async void ShowOrphans_Click(object sender, RoutedEventArgs e)
    {
        if (!_isWebViewInitialized) return;

        try
        {
            await GraphWebView.ExecuteScriptAsync("showOrphans();");
            StatusText.Text = "Orphaned nodes shown";
            await UpdateStatistics();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error showing orphans: {ex.Message}";
        }
    }

    private async void HideOrphans_Click(object sender, RoutedEventArgs e)
    {
        if (!_isWebViewInitialized) return;

        try
        {
            await GraphWebView.ExecuteScriptAsync("hideOrphans();");
            StatusText.Text = "Orphaned nodes hidden";
            await UpdateStatistics();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error hiding orphans: {ex.Message}";
        }
    }

    private async void ResetFilters_Click(object sender, RoutedEventArgs e)
    {
        if (!_isWebViewInitialized) return;

        try
        {
            await GraphWebView.ExecuteScriptAsync("resetFilters();");
            
            // Reset UI controls
            if (DisciplineFilterComboBox != null)
            {
                DisciplineFilterComboBox.SelectedIndex = 0; // Select "All"
            }
            
            StatusText.Text = "All filters cleared";
            await UpdateStatistics();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error resetting filters: {ex.Message}";
        }
    }

    private async void ExportSvg_Click(object sender, RoutedEventArgs e)
    {
        if (!_isWebViewInitialized) return;

        try
        {
            await GraphWebView.ExecuteScriptAsync("exportSvg({ includeBackground: true });");
            StatusText.Text = "SVG export initiated (check browser download)";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error exporting:\n{ex.Message}", "Export Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void EdgeLabels_Changed(object sender, RoutedEventArgs e)
    {
        if (!_isWebViewInitialized || EdgeLabelsCheckBox == null) return;

        try
        {
            bool showLabels = EdgeLabelsCheckBox.IsChecked == true;
            await GraphWebView.ExecuteScriptAsync($"toggleEdgeLabels({showLabels.ToString().ToLower()});");
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error toggling edge labels: {ex.Message}";
        }
    }

    /// <summary>
    /// Updates the statistics display by calling JavaScript calculateStatistics
    /// </summary>
    private async Task UpdateStatistics()
    {
        if (!_isWebViewInitialized) return;

        try
        {
            var statsJson = await GraphWebView.ExecuteScriptAsync("JSON.stringify(calculateStatistics());");
            
            if (!string.IsNullOrEmpty(statsJson) && statsJson != "null")
            {
                var stats = JsonSerializer.Deserialize<GraphStatistics>(statsJson);
                
                if (stats != null)
                {
                    NodeCountText.Text = stats.VisibleNodes.ToString();
                    EdgeCountText.Text = stats.VisibleEdges.ToString();
                    
                    if (!string.IsNullOrEmpty(stats.MostReferenced))
                    {
                        MostRefText.Text = $"{stats.MostReferenced} ({stats.MaxReferences} refs)";
                    }
                    
                    AvgRefText.Text = $"{stats.AverageReferences:F1} refs/node";
                }
            }
        }
        catch (Exception ex)
        {
            // Silently fail - statistics are nice-to-have
            System.Diagnostics.Debug.WriteLine($"Error updating statistics: {ex.Message}");
        }
    }

    /// <summary>
    /// Statistics data model matching JavaScript return value
    /// </summary>
    private class GraphStatistics
    {
        public int VisibleNodes { get; set; }
        public int VisibleEdges { get; set; }
        public string MostReferenced { get; set; } = "";
        public string LeastReferenced { get; set; } = "";
        public int MaxReferences { get; set; }
        public int MinReferences { get; set; }
        public double AverageReferences { get; set; }
    }
}
