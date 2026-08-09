# GraphViewer Quick Start Guide

## What We Built

A **standalone WPF application** that creates beautiful, interactive graph visualizations for DWG Xref relationships using:

- **WPF + WebView2**: Native Windows app embedding modern web technologies
- **Cytoscape.js**: Professional-grade graph library (similar to D3.js, used in Obsidian)
- **Modern UI**: Clean interface with multiple layout algorithms

## Running the Application

### From VS Code Terminal

```powershell
cd GraphViewer
dotnet run
```

### From Windows Explorer

```powershell
cd GraphViewer\bin\Debug\net10.0-windows
.\GraphViewerApp.exe
```

## Testing the Visualization

1. **Launch the app** using one of the methods above
2. **Click "Load Sample Data"** - This loads a sample graph with 7 nodes
3. **Interact with the graph**:
   - **Drag nodes** to reposition them
   - **Scroll wheel** to zoom in/out
   - **Click a node** to highlight its connections
   - **Hover** over nodes to see detailed information
4. **Try different layouts** from the dropdown:
   - Force Directed (CoSE) - Organic, physics-based
   - Hierarchical - Top-down tree structure
   - Circular - Nodes in a circle
   - Concentric - Based on importance
5. **Search** for nodes using the search box
6. **Export** as PNG image

## Visual Features You'll See

### Node Styling
- **Size** varies by reference count (more referenced = larger)
- **Colors** indicate discipline:
  - 🔵 Blue = Architectural
  - 🟠 Orange = Structural  
  - 🟢 Green = MEP
  - 🟣 Purple = Reference files
- **Borders** highlight on hover/click
- **Shadows** give depth

### Interactions
- **Click node** → Highlights connected nodes, dims others
- **Click edge** → Highlights source and target
- **Click background** → Clears all highlighting
- **Hover** → Shows tooltip with details

### Animations
- Smooth layout transitions
- Animated zoom/pan
- Fade in/out effects

## Next Steps: Merging with XLinkViewer

Once you're happy with the graph viewer, we can integrate it:

### Option A: Launch Separately
- Add button in XLinkViewer to export JSON
- Save graph data to temp file
- Launch GraphViewer with file path as argument
- GraphViewer loads and displays

### Option B: Embed in XLinkViewer
- Add GraphViewer as project reference
- Create new window in XLinkViewer
- Embed WebView2 control directly
- Pass data object instead of JSON file

### Option C: Replace MSAGL
- Remove Microsoft.Msagl references
- Replace graph display with WebView2
- Keep same ViewModel pattern
- Users get better visualization in same window

## Sample Data Structure

The sample includes:
- **7 nodes** representing different DWG files
- **7 edges** showing Xref relationships
- **3 groups** (Architectural, Structural, MEP, Reference)
- Various reference counts to show sizing

## Customization Tips

### Change Colors
Edit `wwwroot/graph.js` → `getNodeColor()` function

### Adjust Node Sizes
Edit `wwwroot/graph.js` → `getNodeSize()` function

### New Layout
Add to `MainWindow.xaml` ComboBox and `graph.js` layout options

### Custom Styles
Edit `wwwroot/styles.css` for tooltips, backgrounds, etc.

## Troubleshooting

### "WebView2 Runtime not found"
**Solution**: Install from https://developer.microsoft.com/microsoft-edge/webview2/

### "index.html not found"
**Solution**: Rebuild project - wwwroot files should copy to bin folder automatically

### Graph doesn't appear
**Solution**: 
1. Press F12 to open browser dev tools
2. Check console for JavaScript errors
3. Verify wwwroot files exist in bin folder

## Comparison to MSAGL

| Feature | MSAGL | Cytoscape.js (GraphViewer) |
|---------|-------|---------------------------|
| Visual Quality | ⭐⭐ Basic | ⭐⭐⭐⭐⭐ Modern, polished |
| Interactivity | ⭐⭐ Limited | ⭐⭐⭐⭐⭐ Rich interactions |
| Layouts | ⭐⭐⭐ Good | ⭐⭐⭐⭐⭐ Many algorithms |
| Customization | ⭐⭐ Code-based | ⭐⭐⭐⭐⭐ CSS/JS flexible |
| Performance | ⭐⭐⭐⭐ Native | ⭐⭐⭐⭐ Web-based |
| Export | ⭐⭐ Limited | ⭐⭐⭐⭐ PNG, SVG, JSON |

## What You Get

✅ **Standalone working application**
✅ **Sample data for testing**  
✅ **6 layout algorithms**
✅ **Interactive highlighting**
✅ **Search functionality**
✅ **Export to PNG**
✅ **Professional appearance**
✅ **Hover tooltips**
✅ **Smooth animations**
✅ **Ready to integrate**

## Screenshots Description

When you run it, you'll see:

1. **Top toolbar** with layout dropdown, action buttons, and search
2. **Large graph area** with gradient background
3. **Nodes** sized and colored appropriately
4. **Curved arrows** connecting nodes
5. **Tooltips** appearing on hover
6. **Status bar** showing node/edge counts

The visual style is **modern and professional**, similar to tools like:
- Obsidian graph view
- Neo4j Browser
- Gephi
- Figma connections

Much more appealing than the basic MSAGL tree view!

## Questions?

Experiment with the sample data first, then let me know:
1. Do you like the visual style?
2. Which layout works best for your use case?
3. Any color/size adjustments needed?
4. Ready to integrate with XLinkViewer?
