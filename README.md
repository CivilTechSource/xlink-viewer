# XLink Viewer V2 - With Integrated Graph Visualization

This is version 2 of XLink Viewer that integrates the modern Cytoscape.js graph visualization directly into the main application.

## What's New in V2

### Integrated Graph Viewer
- **Modern Visual Graph**: Uses Cytoscape.js via WebView2 for beautiful, interactive graphs
- **Replaces MSAGL**: The old Microsoft.Msagl graph view has been replaced with a much more visually appealing solution
- **Same Workflow**: All existing functionality remains - scanning, file selection, checklist generation
- **Enhanced Visualization**: Multiple layout algorithms, interactive highlighting, search, and export capabilities

###Features Retained from V1
- DWG file scanning with ACadSharp
- External reference (Xref) detection
- Reverse lookup (shows which files reference a selected drawing)
- Multi-file selection support
- Context menu integration (right-click on DWG files)
- Checklist generation for drawing revisions
- Self-contained deployment

### New Graph Features
- **6 Layout Algorithms**: Force-directed, hierarchical, circular, grid, concentric, breadthfirst
- **Interactive Nodes**: Click to highlight connections, hover for details
- **Search**: Find specific files in the graph
- **Color Coding**: Nodes colored by discipline (Architectural, Structural, MEP)
- **Dynamic Sizing**: Node size based on reference count
- **Export**: Save graph as high-resolution PNG
- **Smooth Animations**: Professional transitions and effects

## Project Structure

```
XLink V2/
├── Models/              # Data models (DwgFileInfo, XrefRelationship)
├── Services/            # XrefScanner service
├── ViewModels/          # MainViewModel with MVVM pattern  
├── Views/               # MainWindow and ChecklistWindow
├── GraphViewer/         # Integrated graph visualization folder
│   ├── ViewModels/      # GraphViewModel, GraphData models
│   ├── Services/        # GraphDataConverter
│   └── wwwroot/         # HTML/CSS/JS for Cytoscape.js
├── XLinkViewer.csproj   # Main project with WebView2 package
└── README.md            # This file
```

## How to Test

### Current Status
The base XLinkViewer app compiles and runs with all existing features intact.

```powershell
cd "XLink V2"
dotnet run --project XLinkViewer.csproj
```

### Next Steps
Once integration is complete, you'll be able to:
1. Scan DWG files as usual
2. Click **"Show Graph View"** button
3. See beautiful interactive graph of relationships
4. All existing features (checklist, multi-select) continue working

## Comparison: V1 vs V2

| Feature | V1 (Original) | V2 (Integrated) |
|---------|---------------|-----------------|
| Graph Visualization | MSAGL (basic) | Cytoscape.js (modern) |
| Visual Quality | ⭐⭐ | ⭐⭐⭐⭐⭐ |
| Interactivity | Limited | Rich (click, hover, search) |
| Layout Options | 1 | 6 algorithms |
| Export | Screenshot | High-res PNG |
| All V1 Features | ✅ | ✅ Maintained |

## Dependencies

### NuGet Packages
- **ACadSharp** (3.4.2): DWG file reading
- **CommunityToolkit.Mvvm** (8.4.0): MVVM helpers
- **Microsoft.Web.WebView2** (1.0.2792.45): Web content embedding
- **Microsoft.Msagl** (1.1.6): Still included for reference, may be removed later

### Framework
- **.NET 10.0 Windows**: Modern .NET with WPF
- **WebView2 Runtime**: Usually pre-installed on Windows 10/11

## Notes

- Original version remains intact in parent folder
- This version is for testing the integrated graph viewer
- Once validated, this can become the main version
- The GraphViewer folder contains standalone components that are integrated into the main  app

## Questions?

See the GraphViewer folder for detailed documentation about the graph visualization component.
