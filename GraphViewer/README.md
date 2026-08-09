# XLink Graph Viewer

A modern, interactive graph visualization tool for viewing DWG Xref relationships using WPF, WebView2, and Cytoscape.js.

## Features

### 🎨 Visual Appeal
- **Modern UI**: Clean, professional interface with gradient backgrounds
- **Interactive Graphs**: Drag nodes, zoom, pan with smooth animations
- **Color-Coded Nodes**: Automatically colored by discipline (Architectural, Structural, MEP) or reference count
- **Dynamic Node Sizing**: Nodes scale based on how many files reference them
- **Hover Tooltips**: Rich information display on mouse hover
- **Click Highlighting**: Click nodes to see connected relationships

### 📊 Layout Options
- **Force Directed (CoSE)**: Physics-based organic layout
- **Hierarchical (Dagre)**: Top-down dependency tree
- **Circular**: Nodes arranged in concentric circles
- **Grid**: Structured grid arrangement
- **Concentric**: Arranged by importance (reference count)
- **Breadthfirst**: Tree-like directed layout

### 🔧 Tools & Features
- **Search**: Real-time node search with highlighting
- **Export**: Save graph as high-quality PNG
- **Fit to View**: Auto-zoom to show entire graph
- **Reset Zoom**: Return to default zoom level
- **Load JSON**: Import graph data from JSON files
- **Sample Data**: Built-in sample for testing

## Technology Stack

- **WPF**: Windows Presentation Foundation (.NET 10.0)
- **WebView2**: Microsoft Edge WebView2 for web content
- **Cytoscape.js**: Professional graph visualization library
- **JavaScript**: Interactive graph manipulation
- **HTML5/CSS3**: Modern web technologies

## Project Structure

```
GraphViewer/
├── GraphViewerApp.csproj       # Project file with dependencies
├── App.xaml                    # Application entry point
├── App.xaml.cs
├── MainWindow.xaml             # Main window with WebView2
├── MainWindow.xaml.cs          # Window logic & event handlers
├── ViewModels/
│   └── GraphViewModel.cs       # Data models & sample data
└── wwwroot/                    # Web assets
    ├── index.html              # HTML container
    ├── styles.css              # Custom styling
    └── graph.js                # Cytoscape.js configuration
```

## Getting Started

### Prerequisites

- **.NET 10.0 SDK**: [Download here](https://dotnet.microsoft.com/download)
- **WebView2 Runtime**: Usually pre-installed on Windows 10/11
  - If needed: [Download here](https://developer.microsoft.com/microsoft-edge/webview2/)

### Building the Application

```powershell
cd GraphViewer
dotnet restore
dotnet build
```

### Running the Application

```powershell
dotnet run
```

Or use the VS Code task: `Ctrl+Shift+P` → "Tasks: Run Task" → Create a task for GraphViewer

## Usage Guide

### Testing with Sample Data

1. Launch the application
2. Click **"Load Sample Data"** button
3. The graph will display with sample DWG relationships
4. Experiment with:
   - Different layouts from the dropdown
   - Clicking nodes to highlight connections
   - Searching for specific files
   - Zooming and panning

### Loading Your Data

#### Option 1: JSON File

Create a JSON file with this structure:

```json
{
  "nodes": [
    {
      "id": "A-101",
      "label": "A-101 Floor Plan",
      "group": "Architectural",
      "referencedByCount": 5,
      "xrefCount": 2,
      "filePath": "C:\\Projects\\Drawings\\A-101.dwg"
    }
  ],
  "edges": [
    {
      "id": "e1",
      "source": "A-101",
      "target": "BASE-MAP",
      "label": "xref"
    }
  ]
}
```

Then use **"Load JSON File"** to import.

#### Option 2: Integration with XLinkViewer

When you're ready to merge this with the main XLinkViewer application:

1. Add a service class to convert `DwgFileInfo` to `GraphData`
2. Add a button in MainWindow to launch GraphViewer
3. Pass JSON data via command-line args or temporary file
4. GraphViewer launches and visualizes the scanned data

## Data Model

### GraphNode
- `Id`: Unique identifier (typically filename)
- `Label`: Display name
- `Group`: Category (Architectural, Structural, MEP, Reference)
- `ReferencedByCount`: How many files reference this one
- `XrefCount`: How many files this one references
- `FilePath`: Full file path (optional)

### GraphEdge
- `Id`: Unique identifier
- `Source`: Source node ID (the file that has the xref)
- `Target`: Target node ID (the file being referenced)
- `Label`: Relationship type (usually "xref")

## Customization

### Node Colors

Edit `getNodeColor()` in `graph.js`:

```javascript
function getNodeColor(node) {
    if (node.group === 'Architectural') return '#2196F3'; // Blue
    if (node.group === 'Structural') return '#FF9800';    // Orange
    if (node.group === 'MEP') return '#4CAF50';           // Green
    if (node.group === 'Reference') return '#9C27B0';     // Purple
    // ... customize colors
}
```

### Node Sizing

Modify `getNodeSize()` in `graph.js`:

```javascript
function getNodeSize(node) {
    const baseSize = 60;
    const sizeIncrement = Math.min(node.referencedByCount * 5, 40);
    return baseSize + sizeIncrement;
}
```

### Layout Parameters

Adjust layout options in `applyLayout()` function:

```javascript
if (layoutName === 'cose') {
    layoutOptions.nodeRepulsion = 8000;      // Spacing between nodes
    layoutOptions.idealEdgeLength = 100;      // Edge length
    layoutOptions.gravity = 1;                // Center pull strength
}
```

## Integration Plan

To merge GraphViewer into the main XLinkViewer application:

1. **Add GraphViewer project reference** to main solution
2. **Create conversion service**:
   ```csharp
   public class GraphDataConverter
   {
       public GraphData ConvertFromScanResults(Dictionary<string, DwgFileInfo> scannedFiles)
       {
           // Convert DwgFileInfo to GraphNode
           // Convert Xref relationships to GraphEdge
       }
   }
   ```

3. **Add menu item** in MainWindow.xaml:
   ```xml
   <Button Content="Show Graph View" Command="{Binding ShowGraphCommand}"/>
   ```

4. **Launch GraphViewer** from MainViewModel:
   ```csharp
   [RelayCommand]
   private void ShowGraph()
   {
       var graphData = _converter.ConvertFromScanResults(_scannedFiles);
       // Launch GraphViewer with data
   }
   ```

## Keyboard Shortcuts

- **Click Node**: Highlight connections
- **Click Background**: Clear highlighting
- **Click Edge**: Highlight source and target
- **Mouse Wheel**: Zoom in/out
- **Click + Drag**: Pan the graph
- **Type in Search**: Find and highlight nodes

## Troubleshooting

### WebView2 Not Initializing

**Problem**: Error message about WebView2 Runtime

**Solution**: Install WebView2 Runtime from Microsoft

### Graph Not Loading

**Problem**: White screen or no graph displayed

**Solution**: 
- Check browser console (F12 in WebView2)
- Ensure wwwroot files are copied to output directory
- Verify JSON data structure is correct

### Performance Issues

**Problem**: Slow with large graphs (1000+ nodes)

**Solution**:
- Use simpler layouts (grid, circle)
- Filter data to show subset of nodes
- Disable animations in layout options

## Future Enhancements

- [ ] Save/Load graph layouts
- [ ] Export to SVG format
- [ ] Advanced filtering (by discipline, date, etc.)
- [ ] Multi-select nodes
- [ ] Path finding between nodes
- [ ] Graph statistics panel
- [ ] Custom node shapes
- [ ] Animation effects
- [ ] Dark mode theme

## License

Part of the XLink Viewer project.

## Support

For issues or questions, refer to the main XLinkViewer documentation.
