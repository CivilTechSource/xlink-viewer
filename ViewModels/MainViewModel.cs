using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using XLinkViewer.Models;
using XLinkViewer.Services;
using XLinkViewer.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.Integration;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;

namespace XLinkViewer.ViewModels
{
    /// <summary>
    /// Main ViewModel for the DWG Xref Relationship Mapper application
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly XrefScanner _xrefScanner;
        private Dictionary<string, DwgFileInfo> _scannedFiles = new();
        private CancellationTokenSource? _cancellationTokenSource;

        /// <summary>
        /// Optional filter - if set, only these files will be shown in the list after scanning
        /// </summary>
        public HashSet<string>? FilteredFilePaths { get; set; }

        [ObservableProperty]
        private string _selectedDirectory = string.Empty;

        [ObservableProperty]
        private string _progressText = "Ready to scan...";

        [ObservableProperty]
        private bool _isScanning = false;

        [ObservableProperty]
        private ObservableCollection<DwgFileInfo> _dwgFiles = new();

        [ObservableProperty]
        private ObservableCollection<string> _referencingFiles = new();

        [ObservableProperty]
        private DwgFileInfo? _selectedFile;

        private readonly ObservableCollection<DwgFileInfo> _selectedFiles = new();
        public ObservableCollection<DwgFileInfo> SelectedFiles => _selectedFiles;

        [ObservableProperty]
        private string _statusText = "Select a folder to begin scanning DWG files";

        public MainViewModel()
        {
            _xrefScanner = new XrefScanner();
            _xrefScanner.ProgressChanged += OnProgressChanged;
            _xrefScanner.ErrorOccurred += OnErrorOccurred;
        }

        [RelayCommand]
        public void BrowseForFolder()
        {
            try
            {
                var dialog = new OpenFolderDialog
                {
                    Title = "Select Project Folder",
                    Multiselect = false
                };

                if (dialog.ShowDialog() == true)
                {
                    SelectedDirectory = dialog.FolderName;
                    StatusText = $"Selected: {SelectedDirectory}";
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error selecting folder: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanStartScan))]
        public async Task StartScanAsync()
        {
            if (string.IsNullOrEmpty(SelectedDirectory) || !Directory.Exists(SelectedDirectory))
            {
                System.Windows.MessageBox.Show("Please select a valid directory first.", "Invalid Directory", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsScanning = true;
                StatusText = "Scanning in progress...";
                _cancellationTokenSource = new CancellationTokenSource();

                _scannedFiles = await _xrefScanner.ScanDirectoryAsync(SelectedDirectory, _cancellationTokenSource.Token);
                
                // Update the UI with results
                DwgFiles.Clear();
                
                // Apply filter if specified (e.g., when launched from right-click context menu)
                foreach (var fileInfo in _scannedFiles.Values.OrderBy(f => f.FileName))
                {
                    // If filter is set, only show files in the filter list
                    if (FilteredFilePaths == null || FilteredFilePaths.Count == 0 || 
                        FilteredFilePaths.Contains(fileInfo.FilePath))
                    {
                        DwgFiles.Add(fileInfo);
                    }
                }

                StatusText = $"Scan complete. Found {DwgFiles.Count} DWG files.";
                ProgressText = $"Found {DwgFiles.Count} DWG files with {_scannedFiles.Values.Sum(f => f.XrefPaths.Count)} total external references.";
                
                // Enable the graph visualization and checklist buttons now that we have data
                ShowGraphViewCommand.NotifyCanExecuteChanged();
                CreateChecklistCommand.NotifyCanExecuteChanged();
            }
            catch (OperationCanceledException)
            {
                StatusText = "Scan cancelled.";
                ProgressText = "Scan was cancelled by user.";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error during scan: {ex.Message}", "Scan Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText = "Scan failed.";
            }
            finally
            {
                IsScanning = false;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        [RelayCommand(CanExecute = nameof(CanCancelScan))]
        public void CancelScan()
        {
            _cancellationTokenSource?.Cancel();
        }

        [RelayCommand(CanExecute = nameof(CanShowGraph))]
        public void ShowGraphView()
        {
            try
            {
                if (_scannedFiles.Count == 0)
                {
                    System.Windows.MessageBox.Show("Please scan DWG files first before visualizing connections.", "No Data Available", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var graph = BuildRelationshipGraph();
                ShowGraphInNewWindow(graph);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error creating graph visualization: {ex.Message}", "Graph Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanCreateChecklist))]
        public void CreateChecklist()
        {
            try
            {
                var filesToProcess = SelectedFiles.Any() ? SelectedFiles.ToList() : 
                    (SelectedFile != null ? new List<DwgFileInfo> { SelectedFile } : new List<DwgFileInfo>());
                
                if (!filesToProcess.Any())
                {
                    System.Windows.MessageBox.Show("Please select one or more drawings to create a revision checklist.", 
                        "No Drawings Selected", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Get all dependent drawings (files that reference any of the selected files)
                var allDependentDrawings = new HashSet<string>();
                foreach (var file in filesToProcess)
                {
                    foreach (var dependent in file.ReferencedBy)
                    {
                        allDependentDrawings.Add(dependent);
                    }
                }

                var sourceDrawingPaths = filesToProcess.Select(f => f.FilePath).ToList();

                var checklistWindow = new ChecklistWindow(sourceDrawingPaths, allDependentDrawings.ToList())
                {
                    Owner = System.Windows.Application.Current.MainWindow
                };

                if (checklistWindow.ShowDialog() == true)
                {
                    var fileNames = string.Join(", ", filesToProcess.Select(f => f.FileName));
                    StatusText = filesToProcess.Count == 1 ? 
                        $"Revision checklist created for {fileNames}" : 
                        $"Revision checklist created for {filesToProcess.Count} drawings";
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error creating checklist: {ex.Message}", "Checklist Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        partial void OnSelectedDirectoryChanged(string value)
        {
            // Notify that the StartScan command's CanExecute state may have changed
            StartScanCommand.NotifyCanExecuteChanged();
        }

        partial void OnIsScanningChanged(bool value)
        {
            // Notify both commands that their CanExecute state has changed
            StartScanCommand.NotifyCanExecuteChanged();
            CancelScanCommand.NotifyCanExecuteChanged();
            ShowGraphViewCommand.NotifyCanExecuteChanged();

            CreateChecklistCommand.NotifyCanExecuteChanged();
        }
        
        partial void OnSelectedFileChanged(DwgFileInfo? value)
        {
            // Update the referencing files display when single selection changes
            UpdateReferencingFilesDisplay();
            CreateChecklistCommand.NotifyCanExecuteChanged();
        }

        public void UpdateSelectedFiles(List<DwgFileInfo> selectedFiles)
        {
            _selectedFiles.Clear();
            foreach (var file in selectedFiles)
            {
                _selectedFiles.Add(file);
            }
            
            UpdateReferencingFilesDisplay();
            CreateChecklistCommand.NotifyCanExecuteChanged();
        }
        
        private void UpdateReferencingFilesDisplay()
        {
            ReferencingFiles.Clear();
            
            var filesToShow = SelectedFiles.Any() ? SelectedFiles.ToList() : 
                (SelectedFile != null ? new List<DwgFileInfo> { SelectedFile } : new List<DwgFileInfo>());
            
            var allReferencingFiles = new HashSet<string>();
            foreach (var file in filesToShow)
            {
                foreach (var referencingFile in file.ReferencedBy)
                {
                    allReferencingFiles.Add(referencingFile);
                }
            }
            
            foreach (var referencingFile in allReferencingFiles.OrderBy(f => Path.GetFileName(f)))
            {
                var fileName = Path.GetFileName(referencingFile);
                ReferencingFiles.Add(fileName);
            }

            if (filesToShow.Any())
            {
                var count = filesToShow.Count;
                var refCount = allReferencingFiles.Count;
                
                StatusText = count == 1 ? 
                    (refCount > 0 ? $"{filesToShow.First().FileName} is referenced by {refCount} file(s)" : 
                                   $"{filesToShow.First().FileName} is not referenced by any other files") :
                    $"{count} drawings selected - {refCount} total referencing files";
            }
        }

        private bool CanStartScan() => !IsScanning && !string.IsNullOrEmpty(SelectedDirectory);
        
        private bool CanCancelScan() => IsScanning;

        private bool CanShowGraph() => !IsScanning && _scannedFiles.Count > 0;

        private bool CanCreateChecklist() => !IsScanning && 
            ((SelectedFiles.Any() || SelectedFile != null) && _scannedFiles.Count > 0);

        private Graph BuildRelationshipGraph()
        {
            var graph = new Graph("DWG External Reference Diagram");
            
            // Add nodes for each DWG file
            foreach (var fileInfo in _scannedFiles.Values)
            {
                var node = graph.AddNode(fileInfo.FileName);
                node.LabelText = fileInfo.FileName;
                
                // Color coding based on reference count
                if (fileInfo.ReferencedBy.Count == 0)
                {
                    node.Attr.FillColor = Microsoft.Msagl.Drawing.Color.LightGreen; // Standalone files
                }
                else if (fileInfo.ReferencedBy.Count > 3)
                {
                    node.Attr.FillColor = Microsoft.Msagl.Drawing.Color.LightCoral; // Heavily referenced
                }
                else
                {
                    node.Attr.FillColor = Microsoft.Msagl.Drawing.Color.LightBlue; // Moderately referenced
                }
                
                // Add tooltip information
                node.LabelText = $"{fileInfo.FileName}\n({fileInfo.ReferencedBy.Count} refs)";
            }
            
            // Add edges for Xref relationships
            foreach (var fileInfo in _scannedFiles.Values)
            {
                foreach (var xrefPath in fileInfo.XrefPaths)
                {
                    var xrefFileName = Path.GetFileNameWithoutExtension(xrefPath);
                    
                    // Find the target file by matching filename
                    var targetFile = _scannedFiles.Values.FirstOrDefault(f => 
                        f.FileName.Equals(xrefFileName, StringComparison.OrdinalIgnoreCase));
                    
                    if (targetFile != null)
                    {
                        var edge = graph.AddEdge(fileInfo.FileName, targetFile.FileName);
                        edge.LabelText = "xref";
                        edge.Attr.Color = Microsoft.Msagl.Drawing.Color.Blue;
                        edge.Attr.ArrowheadAtTarget = ArrowStyle.Normal;
                    }
                }
            }
            
            return graph;
        }

        private void ShowGraphInNewWindow(Graph graph)
        {
            var viewer = new GViewer();
            viewer.Graph = graph;
            
            var host = new WindowsFormsHost();
            host.Child = viewer;
            
            var window = new Window
            {
                Title = "DWG Relationship Diagram",
                Width = 1000,
                Height = 700,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = host
            };
            
            window.Show();
        }

        private void OnProgressChanged(string progress)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                ProgressText = progress;
            });
        }

        private void OnErrorOccurred(string fileName, string error)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // You could show this in a separate error log window or status area
                StatusText = $"Error scanning {fileName}: {error}";
            });
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
        }
    }
}