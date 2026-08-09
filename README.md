# XLink Viewer

A C# WPF desktop application that scans Project folders containing AutoCAD DWG files to build reverse lookup tables showing external reference (Xref) relationships between drawings.

## Features

- **Recursive DWG Scanning**: Automatically finds all DWG files in selected project folders
- **External Reference Detection**: Uses ACadSharp library to read DWG files and extract Xref information without requiring AutoCAD
- **Reverse Lookup**: Shows which drawings reference a selected file (reverse dependency mapping)
- **Multi-Drawing Selection**: Select multiple drawings using Ctrl+click to analyze combined impact
- **Interactive UI**: Browse folders, view file lists, and explore reference relationships
- **Real-time Progress**: Progress tracking with cancellation support during scanning
- **Error Handling**: Graceful handling of corrupt or inaccessible DWG files
- **Graph Visualization**: Interactive MSAGL diagram showing drawing relationships and connections
- **Revision Checklists**: Generate text or Markdown checklists for single or multiple drawing revisions

## Technology Stack

- **Framework**: .NET 10.0 WPF (Windows Presentation Foundation)
- **Architecture**: MVVM (Model-View-ViewModel) pattern
- **CAD Library**: ACadSharp 3.4.2 - Pure C# library for reading DWG files
- **UI Framework**: WPF with Community Toolkit MVVM for commanding and property notifications
- **Graph Visualization**: Microsoft MSAGL (Automatic Graph Layout)

## Project Structure

```
XLinkViewer/
├── Models/
│   ├── DwgFileInfo.cs           # Data model for DWG file information
│   └── XrefRelationship.cs      # Data model for parent-child relationships
├── Services/
│   └── XrefScanner.cs           # Core service using ACadSharp to scan DWG files
├── ViewModels/
│   └── MainViewModel.cs         # MVVM ViewModel with UI logic and commands
├── Views/
│   ├── ChecklistWindow.xaml     # Revision checklist builder and exporter
│   └── ChecklistWindow.xaml.cs
├── GraphViewer/                 # Standalone companion app (separate project)
│   ├── GraphViewerApp.csproj    # Force-directed graph viewer, WebView2 + D3
│   └── wwwroot/                 # HTML/JS/CSS for the graph canvas
├── MainWindow.xaml              # Main UI layout with file lists and controls
├── MainWindow.xaml.cs           # Code-behind with DataContext setup
├── Converters.cs                # UI converters for data binding
├── App.xaml / App.xaml.cs       # Application definition, startup, CLI argument handling
└── XLinkViewer.csproj           # Project file with NuGet dependencies
```

`GraphViewer/` is an independent prototype with its own project file and its own
`README.md`. It is not referenced by `XLinkViewer.csproj` and is built separately.

## Getting Started

### Prerequisites

- Windows 10/11
- .NET 10.0 Runtime or SDK
- VS Code or Visual Studio (recommended for development)

### Installation

1. Clone or download the project
2. Restore NuGet packages:
   ```bash
   dotnet restore
   ```
3. Build the project:
   ```bash
   dotnet build
   ```
4. Run the application:
   ```bash
   dotnet run
   ```

### Running with VS Code

1. Open the project folder in VS Code
2. Press `Ctrl+Shift+P` and run "Tasks: Run Task"
3. Select "Run XLink Viewer"

## Usage Guide

### Basic Workflow

1. **Select Project Folder**: Click "Browse..." to choose folder containing DWG files
2. **Start Scanning**: Click "Start Scan" to recursively analyze all DWG files
3. **View Results**: Browse the DWG files list in the left panel
4. **Explore References**: Select a file to see which other drawings reference it
5. **Create Checklists**: Use single or multi-selection to generate revision checklists
6. **Review Details**: Hover over files for tooltip information about references

### Multi-Drawing Checklist Workflow

1. **Single Drawing Checklist**: Select one drawing and click "Create Checklist"
2. **Multiple Drawing Checklist**: Hold Ctrl and click to select multiple drawings, then click "Create Checklist"
3. **Review Impact**: The checklist window shows all drawings that reference your selected source drawing(s)
4. **Select Items**: Choose which dependent drawings to include in your checklist (all selected by default)
5. **Choose Format**: Select Text (.txt) or Markdown (.md) format
6. **Generate File**: Save your revision checklist as a downloadable file

### Graph Visualization

Click **"Visualise Connection"** after scanning to open an MSAGL-rendered diagram of the
drawing relationships. Nodes are colour-coded by how heavily each drawing is referenced:

- **Green**: standalone files (not referenced by anything)
- **Blue**: moderately referenced
- **Red**: heavily referenced — changing these has the widest impact

A separate force-directed graph viewer (physics simulation, drag, zoom and pan, dark theme)
lives in `GraphViewer/` as a standalone prototype. See [GraphViewer/README.md](GraphViewer/README.md).

### Understanding the Interface

- **Left Panel**: List of all discovered DWG files in the project (supports multi-selection with Ctrl+click)
- **Right Panel**: Shows combined files that reference the currently selected drawing(s)
- **Control Buttons**: Browse, Start Scan, Cancel, Visualise Connection, and Create Checklist actions
- **Status Bar**: Displays scan progress, file counts, and error information
- **Progress Bar**: Animated indicator during scanning operations

### Interpreting Results

- **Referenced by 0 files**: The selected drawing is not used as an Xref by any other drawings
- **Referenced by X files**: Shows how many drawings include this file as an external reference
- **Tooltip Information**: Hover over files to see full path, Xref count, and reference statistics

## How It Works

### DWG File Scanning Process

1. **File Discovery**: Recursively searches directory for `*.dwg` files
2. **ACadSharp Integration**: Opens each DWG file using the ACadSharp library
3. **Block Record Analysis**: Examines Block Table Records for external reference flags
4. **Xref Path Extraction**: Extracts file paths from blocks marked with `XRef` flags
5. **Reverse Mapping**: Builds lookup tables showing which files reference each drawing

### Architecture Design

- **MVVM Pattern**: Separation of UI (View), business logic (ViewModel), and data (Model)
- **Async Operations**: Non-blocking file scanning with cancellation support
- **Error Resilience**: Continues scanning even if individual files fail to open
- **Memory Efficient**: Streams DWG files without loading entire contents into memory

## Dependencies

### NuGet Packages

- **ACadSharp** (3.4.2): Core library for reading AutoCAD DWG files
- **CommunityToolkit.Mvvm** (8.4.0): MVVM helpers for commands and property notifications
- **Microsoft.Msagl** (1.1.6): Graph layout engine
- **Microsoft.Msagl.GraphViewerGDI** (1.1.7): WinForms graph rendering surface, hosted in WPF

### Framework Dependencies

- **.NET 10.0**: Modern .NET framework with Windows specific features
- **WPF**: Windows Presentation Foundation for desktop UI

## Development Notes

### Key Classes

- **XrefScanner**: Core service that uses ACadSharp to read DWG files and extract Xref paths
- **MainViewModel**: Handles UI logic, file browsing, scanning operations, and data binding
- **DwgFileInfo**: Model representing a DWG file with its Xrefs and reverse references
- **XrefRelationship**: Model representing parent-child relationships between drawings

### Error Handling

The application handles common scenarios gracefully:
- **Corrupt DWG Files**: Logs error and continues with remaining files
- **Access Denied**: Shows error message for permission-restricted files  
- **Network Paths**: Works with UNC paths and mapped drives
- **Large Projects**: Cancellation support for very large folder scans

### Future Enhancement Ideas

- Export dependency reports to CSV/Excel
- Graph visualization of the entire project structure
- Support for DXF files in addition to DWG
- Integration with AutoCAD if available for additional metadata
- Filtering and search capabilities for large projects

## Troubleshooting

### Common Issues

**Application won't start**
- Ensure .NET 10.0 runtime is installed
- Check Windows version compatibility (Windows 10+ required)

**DWG files not being detected**
- Verify files have `.dwg` extension
- Check folder permissions and accessibility
- Ensure files are not corrupted or locked

**Scanning stops with errors**
- Check the status bar for specific error messages
- Try scanning a smaller subset of files first
- Verify DWG files are valid AutoCAD format

**Performance issues**
- Large projects with hundreds of DWG files may take time
- Use the Cancel button to stop long-running operations
- Consider scanning subfolders individually for huge projects

## Contributing

This project uses standard C# coding practices:
- MVVM architectural pattern
- Async/await for I/O operations  
- Proper exception handling and logging
- WPF data binding and commanding

## License

Released under the [MIT License](LICENSE).

Third-party dependencies carry their own licenses — ACadSharp (MIT), CommunityToolkit.Mvvm
(MIT), and Microsoft MSAGL (MIT). Review these terms for commercial use.

## Support

For issues related to:
- **DWG File Reading**: Check ACadSharp documentation and GitHub repository
- **Application Bugs**: Review error messages in the status bar
- **Performance**: Monitor system resources during large scans