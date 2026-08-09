using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using XLinkViewer.ViewModels;

namespace XLinkViewer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    /// <summary>
    /// Gets the startup path from command-line arguments
    /// </summary>
    public string StartupPath { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the list of selected DWG files to highlight after scanning
    /// </summary>
    public List<string> SelectedDwgFiles { get; private set; } = new List<string>();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Check if we have command-line arguments
        if (e.Args.Length > 0)
        {
            // Collect all DWG files from arguments
            SelectedDwgFiles = e.Args
                .Where(arg => File.Exists(arg) && Path.GetExtension(arg).Equals(".dwg", StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Use the first argument to determine the scan directory
            StartupPath = e.Args[0];
            
            // Wait for the main window to be created and shown
            this.Dispatcher.BeginInvoke(async () =>
            {
                await HandleStartupPath();
            }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }
    }

    private async Task HandleStartupPath()
    {
        if (string.IsNullOrEmpty(StartupPath))
            return;

        // Find the main window and get its view model
        var mainWindow = Current.MainWindow as MainWindow;
        if (mainWindow?.DataContext is not MainViewModel viewModel)
            return;

        string targetDirectory;

        try
        {
            // Check if the startup path is a file or directory
            if (File.Exists(StartupPath))
            {
                // If it's a .dwg file, go up 2 levels from its directory
                if (Path.GetExtension(StartupPath).Equals(".dwg", StringComparison.OrdinalIgnoreCase))
                {
                    var dwgDirectory = Path.GetDirectoryName(StartupPath);
                    if (!string.IsNullOrEmpty(dwgDirectory))
                    {
                        // Go up 2 levels
                        var parentDirectory = Directory.GetParent(dwgDirectory);
                        if (parentDirectory != null)
                        {
                            var grandParentDirectory = Directory.GetParent(parentDirectory.FullName);
                            targetDirectory = grandParentDirectory?.FullName ?? parentDirectory.FullName;
                        }
                        else
                        {
                            targetDirectory = dwgDirectory;
                        }
                    }
                    else
                    {
                        targetDirectory = string.Empty;
                    }
                }
                else
                {
                    // For other file types, use the directory
                    targetDirectory = Path.GetDirectoryName(StartupPath) ?? string.Empty;
                }
            }
            else if (Directory.Exists(StartupPath))
            {
                // It's a directory - use it as-is
                targetDirectory = StartupPath;
            }
            else
            {
                // Path doesn't exist
                System.Windows.MessageBox.Show($"The specified path does not exist: {StartupPath}", "Invalid Path", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!string.IsNullOrEmpty(targetDirectory))
            {
                // Set the selected directory in the view model
                viewModel.SelectedDirectory = targetDirectory;
                
                // If specific DWG files were selected, set them as a filter
                if (SelectedDwgFiles.Count > 0)
                {
                    viewModel.FilteredFilePaths = new HashSet<string>(
                        SelectedDwgFiles, 
                        StringComparer.OrdinalIgnoreCase);
                }
                
                // Automatically start the scan
                await viewModel.StartScanAsync();

                // After scanning, select the DWG files that were passed as arguments
                if (SelectedDwgFiles.Count > 0)
                {
                    SelectDwgFilesInViewModel(viewModel);
                }
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error processing startup path: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SelectDwgFilesInViewModel(MainViewModel viewModel)
    {
        // Find and select the DWG files in the scanned results
        foreach (var dwgPath in SelectedDwgFiles)
        {
            var fileInfo = viewModel.DwgFiles.FirstOrDefault(f => 
                f.FilePath.Equals(dwgPath, StringComparison.OrdinalIgnoreCase));
            
            if (fileInfo != null)
            {
                fileInfo.IsSelected = true;
            }
        }
    }
}

