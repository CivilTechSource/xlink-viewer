## 🖱️ **Mouse/Interaction Enhancements** (EASY)

### Middle-Click Pan
- Middle-click drag to pan the graph


### Double-Click Actions
- **Double-click node** → Center and zoom to that node
- **Double-click background** → Reset view (fit all)
- **Double-click edge** → Highlight full path between nodes

### Right-Click Context Menu
- Right-click node → "Center on this", "Hide this node", "Show only connected"


## 🎨 **Visual Customization** (EASY)

### Theme Switcher
- **Light/Dark mode** toggle button
- Changes background, node colors, edge colors
- Store preference in localStorage

### Node Shape Options
- Different shapes for different based on user Filter input

### Edge Styling
- **Thickness** based on importance
- **Dashed lines** for optional references vs solid for required
- **Edge labels** show/hide toggle

## 🔍 **Filter & Display Options** (EASY)

### Quick Filters (Toolbar Buttons)
- "Show only Architectural files"
- "Show only Structural files"
- "Show only MEP files"
- "Hide isolated nodes" (files with no connections)
- "Show only important nodes" (referenced by 3+ files)

### Slider Controls
- **Node size multiplier** (make all nodes bigger/smaller)
- **Edge thickness** adjustment


### Discipline Toggle Buttons
- Checkbox to hide and show filters created by user

## 📊 **Information Display** (EASY)

### Node Details Panel (Side Panel)
When node is clicked, show:
- Full file path
- Number of references (in/out)
- List of files it references
- List of files that reference it


### Statistics Panel
- Total nodes/edges
- Most referenced file
- Least referenced file
- Average references per file


## 🔄 **Path & Relationship Features** (MEDIUM)


### Show All Paths
- Select a node → "Show all paths to this file"
- Useful for understanding deep dependencies

### Dependency Depth
- Color nodes by "depth" (how many hops from root files)
- Helps visualize hierarchy

## 💾 **Save/Load Features** (EASY)

### Save Current View
- Button to save current:
  - Filter settings


### Export Options (In Addition to PNG)
- **SVG export** (vector, better quality)


### Screenshot with Options
- Include/exclude background
- Include/exclude node labels
- High DPI multiplier (1x, 2x, 4x)

## 🎯 **Selection & Highlight** (EASY)



### Highlight Modes
- **Upstream** (show everything this file depends on)
- **Downstream** (show everything that depends on this file)
- **Both** (default, current behavior)

## 🎛️ **Layout Enhancements** (EASY)

### Layout with Constraints
- "Keep selected nodes fixed" while re-layouting others
- "Separate by discipline" (auto-group)
- "Arrange by reference count" (most referenced in center)

### Animation Speed Control
- Slider for layout animation duration
- Toggle animations on/off


## 🔔 **Smart Features** (MEDIUM-EASY)

### Orphan Detection
- Highlight files with no connections
- "These files might be unused"


## ⚡ **Performance Toggles** (EASY)

### Simplify for Large Graphs
- "Hide labels" for >100 nodes
- "Disable shadows" for >200 nodes
- "Reduce edge curves" for better performance

### Progressive Loading
- Show important nodes first
- Load others in background

---
