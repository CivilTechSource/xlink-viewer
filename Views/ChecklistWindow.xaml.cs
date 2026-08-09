using XLinkViewer.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace XLinkViewer.Views
{
    /// <summary>
    /// Interaction logic for ChecklistWindow.xaml
    /// </summary>
    public partial class ChecklistWindow : Window
    {
        private readonly List<string> _sourceDrawings;
        private readonly List<string> _dependentDrawings;
        private readonly List<System.Windows.Controls.CheckBox> _checkBoxes;
        
        public ChecklistWindow(List<string> sourceDrawings, List<string> dependentDrawings)
        {
            InitializeComponent();
            
            _sourceDrawings = sourceDrawings ?? throw new ArgumentNullException(nameof(sourceDrawings));
            _dependentDrawings = dependentDrawings;
            _checkBoxes = new List<System.Windows.Controls.CheckBox>();
            
            InitializeWindow();
        }
        
        private void InitializeWindow()
        {
            // Set header information
            if (_sourceDrawings.Count == 1)
            {
                var sourceName = Path.GetFileNameWithoutExtension(_sourceDrawings[0]);
                SourceDrawingText.Text = $"Source Drawing: {sourceName}";
            }
            else
            {
                var sourceNames = _sourceDrawings.Select(Path.GetFileNameWithoutExtension).ToList();
                if (sourceNames.Count <= 3)
                {
                    SourceDrawingText.Text = $"Source Drawings: {string.Join(", ", sourceNames)}";
                }
                else
                {
                    SourceDrawingText.Text = $"Source Drawings: {sourceNames.Count} drawings selected";
                }
            }
            
            // Create checkboxes for each dependent drawing
            if (_dependentDrawings.Count == 0)
            {
                DrawingSelectionPanel.Visibility = Visibility.Collapsed;
                NoItemsMessage.Visibility = Visibility.Visible;
                GenerateButton.IsEnabled = false;
            }
            else
            {
                foreach (var drawingPath in _dependentDrawings)
                {
                    var drawingName = Path.GetFileNameWithoutExtension(drawingPath);
                    var checkBox = new System.Windows.Controls.CheckBox
                    {
                        Content = drawingName,
                        Tag = drawingPath,
                        Margin = new Thickness(5),
                        IsChecked = true // Start with all selected
                    };
                    
                    checkBox.Checked += CheckBox_Changed;
                    checkBox.Unchecked += CheckBox_Changed;
                    
                    _checkBoxes.Add(checkBox);
                    DrawingSelectionPanel.Children.Add(checkBox);
                }
                
                UpdateSelectionCount();
            }
        }
        
        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            UpdateSelectionCount();
        }
        
        private void UpdateSelectionCount()
        {
            var selectedCount = _checkBoxes.Count(cb => cb.IsChecked == true);
            var totalCount = _checkBoxes.Count;
            
            SelectionCount.Text = $"{selectedCount} of {totalCount} selected";
            GenerateButton.IsEnabled = selectedCount > 0;
        }
        
        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var checkBox in _checkBoxes)
            {
                checkBox.IsChecked = true;
            }
        }
        
        private void ClearAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var checkBox in _checkBoxes)
            {
                checkBox.IsChecked = false;
            }
        }
        
        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedDrawings = _checkBoxes
                    .Where(cb => cb.IsChecked == true)
                    .Select(cb => cb.Content?.ToString() ?? "")
                    .ToList();
                
                if (selectedDrawings.Count == 0)
                {
                    System.Windows.MessageBox.Show("Please select at least one drawing to include in the checklist.", 
                        "No Drawings Selected", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                
                var isMarkdown = MdFormat.IsChecked == true;
                var includeTimestamp = IncludeTimestamp.IsChecked == true;
                
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = isMarkdown ? "Markdown files (*.md)|*.md|All files (*.*)|*.*" : "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                    DefaultExt = isMarkdown ? "md" : "txt",
                    FileName = GenerateFileName(includeTimestamp, isMarkdown)
                };
                
                if (saveDialog.ShowDialog() == true)
                {
                    var content = GenerateChecklistContent(selectedDrawings, isMarkdown);
                    File.WriteAllText(saveDialog.FileName, content);
                    
                    System.Windows.MessageBox.Show($"Checklist generated successfully:\\n{saveDialog.FileName}", 
                        "Checklist Created", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error generating checklist: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private string GenerateFileName(bool includeTimestamp, bool isMarkdown)
        {
            string baseName;
            if (_sourceDrawings.Count == 1)
            {
                baseName = Path.GetFileNameWithoutExtension(_sourceDrawings[0]);
            }
            else
            {
                baseName = $"MultipleDrawings_{_sourceDrawings.Count}";
            }
            
            var extension = isMarkdown ? "md" : "txt";
            var timestamp = includeTimestamp ? $"_{DateTime.Now:yyyyMMdd_HHmm}" : "";
            
            return $"RevisionChecklist_{baseName}{timestamp}.{extension}";
        }
        
        private string GenerateChecklistContent(List<string> selectedDrawings, bool isMarkdown)
        {
            var content = new StringBuilder();
            var sourceDescription = GenerateSourceDescription();
            
            if (isMarkdown)
            {
                // Markdown format
                content.AppendLine($"# Drawing Revision Checklist");
                content.AppendLine();
                content.AppendLine(sourceDescription);
                content.AppendLine($"**Created:** {DateTime.Now:MMMM dd, yyyy HH:mm}");
                content.AppendLine($"**Created By:** {Environment.UserName}");
                content.AppendLine();
                content.AppendLine($"## Impact Assessment");
                content.AppendLine();
                
                if (_sourceDrawings.Count == 1)
                {
                    var sourceName = Path.GetFileNameWithoutExtension(_sourceDrawings[0]);
                    content.AppendLine($"The following drawings reference **{sourceName}** and should be reviewed for potential impact:");
                }
                else
                {
                    content.AppendLine($"The following drawings reference one or more of the source drawings and should be reviewed for potential impact:");
                }
                
                content.AppendLine();
                content.AppendLine($"### Drawings to Review ({selectedDrawings.Count} items)");
                content.AppendLine();
                
                foreach (var drawing in selectedDrawings)
                {
                    content.AppendLine($"- [ ] **{drawing}**");
                }
                
                content.AppendLine();
                content.AppendLine($"### Review Instructions");
                content.AppendLine();
                content.AppendLine($"For each drawing above:");
                
                if (_sourceDrawings.Count == 1)
                {
                    var sourceName = Path.GetFileNameWithoutExtension(_sourceDrawings[0]);
                    content.AppendLine($"1. Open the drawing and review how it references {sourceName}");
                    content.AppendLine($"2. Assess if your changes to {sourceName} will impact this drawing");
                }
                else
                {
                    content.AppendLine($"1. Open the drawing and review how it references the source drawing(s)");
                    content.AppendLine($"2. Assess if your changes to the source drawings will impact this drawing");
                }
                
                content.AppendLine($"3. Update the drawing if necessary");
                content.AppendLine($"4. Check off the item when review is complete");
                content.AppendLine();
                content.AppendLine($"### Notes");
                content.AppendLine();
                content.AppendLine($"_Add any additional notes about the revision impact here..._");
            }
            else
            {
                // Plain text format
                content.AppendLine("DRAWING REVISION CHECKLIST");
                content.AppendLine("==========================");
                content.AppendLine();
                content.AppendLine(sourceDescription.Replace("**", "").Replace(":", ":"));
                content.AppendLine($"Created: {DateTime.Now:MMMM dd, yyyy HH:mm}");
                content.AppendLine($"Created By: {Environment.UserName}");
                content.AppendLine();
                content.AppendLine("IMPACT ASSESSMENT");
                content.AppendLine("-----------------");
                content.AppendLine();
                
                if (_sourceDrawings.Count == 1)
                {
                    var sourceName = Path.GetFileNameWithoutExtension(_sourceDrawings[0]);
                    content.AppendLine($"The following drawings reference '{sourceName}' and should be reviewed");
                }
                else
                {
                    content.AppendLine($"The following drawings reference one or more of the source drawings and should be reviewed");
                }
                
                content.AppendLine("for potential impact:");
                content.AppendLine();
                content.AppendLine($"DRAWINGS TO REVIEW ({selectedDrawings.Count} items):");
                content.AppendLine();
                
                int counter = 1;
                foreach (var drawing in selectedDrawings)
                {
                    content.AppendLine($"  [ ] {counter}. {drawing}");
                    counter++;
                }
                
                content.AppendLine();
                content.AppendLine("REVIEW INSTRUCTIONS:");
                content.AppendLine();
                content.AppendLine("For each drawing above:");
                
                if (_sourceDrawings.Count == 1)
                {
                    var sourceName = Path.GetFileNameWithoutExtension(_sourceDrawings[0]);
                    content.AppendLine($"1. Open the drawing and review how it references {sourceName}");
                    content.AppendLine($"2. Assess if your changes to {sourceName} will impact this drawing");
                }
                else
                {
                    content.AppendLine($"1. Open the drawing and review how it references the source drawing(s)");
                    content.AppendLine($"2. Assess if your changes to the source drawings will impact this drawing");
                }
                
                content.AppendLine("3. Update the drawing if necessary");
                content.AppendLine("4. Check off the item when review is complete");
                content.AppendLine();
                content.AppendLine("NOTES:");
                content.AppendLine("______________________________________________________________________");
                content.AppendLine();
                content.AppendLine("Add any additional notes about the revision impact here...");
                content.AppendLine();
                content.AppendLine("______________________________________________________________________");
            }
            
            return content.ToString();
        }
        
        private string GenerateSourceDescription()
        {
            if (_sourceDrawings.Count == 1)
            {
                var sourceName = Path.GetFileNameWithoutExtension(_sourceDrawings[0]);
                return $"**Source Drawing:** {sourceName}";
            }
            else
            {
                var sourceNames = _sourceDrawings.Select(Path.GetFileNameWithoutExtension).ToList();
                if (sourceNames.Count <= 5)
                {
                    return $"**Source Drawings:** {string.Join(", ", sourceNames)}";
                }
                else
                {
                    return $"**Source Drawings:** {string.Join(", ", sourceNames.Take(3))} and {sourceNames.Count - 3} more";
                }
            }
        }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}