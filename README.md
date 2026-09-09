# XLink Viewer

**Stop guessing which drawings break when you edit an Xref.** XLink Viewer scans a folder of
AutoCAD DWG files, maps every external reference (Xref) relationship between them, and shows you
— instantly and visually — which drawings depend on which. Select a file before you revise it and
know exactly who else needs a checklist.

No AutoCAD installation required: drawings are read directly with a pure C# DWG parser, so it
runs on any Windows machine.

![Platform](https://img.shields.io/badge/platform-Windows-0078D6)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)
![License](https://img.shields.io/badge/license-MIT-green)

## Why XLink Viewer?

Large CAD projects accumulate deep chains of Xrefs — architectural sheets reference structural
grids, structural sheets reference site plans, and so on. When one drawing changes, figuring out
everything downstream that needs re-checking is tedious and error-prone if done by hand. XLink
Viewer builds that reverse-dependency map for you in seconds and turns it into a shareable
revision checklist.

## Features

- **Recursive DWG Scanning** — point it at a project folder and it finds every `.dwg` file
- **Xref Detection** — reads Block Table Records with ACadSharp, no AutoCAD needed
- **Reverse Lookup** — select a drawing and instantly see every drawing that references it
- **Interactive Graph View** — a Cytoscape.js graph (rendered in an embedded WebView2 browser)
  with 6 layout algorithms, click-to-highlight, search, discipline color-coding, and high-res
  PNG export
- **Multi-Drawing Selection** — Ctrl+click to analyze the combined impact of several drawings
- **Revision Checklists** — generate a Text or Markdown checklist of every affected drawing
- **Explorer Integration** — right-click a folder or DWG file to launch a scan directly
- **Self-Contained** — ships as a single-file, self-contained deployment; no separate .NET
  install required on the target machine

## Project Structure

```
XLinkViewer/
├── Models/              # DwgFileInfo, XrefRelationship — core data models
├── Services/            # XrefScanner (DWG scanning), GraphDataConverter (graph data mapping)
├── ViewModels/          # MainViewModel — MVVM UI logic and commands
├── Views/               # MainWindow, ChecklistWindow, GraphViewerWindow
├── GraphViewer/          # Standalone prototype graph viewer (separate project, own README)
│   └── wwwroot/          # HTML/CSS/JS (Cytoscape.js) shared with the integrated graph view
├── App.xaml / App.xaml.cs
├── XLinkViewer.csproj
└── XLinkViewer-Setup.iss # Inno Setup installer script
```

## Getting Started

### Prerequisites

- Windows 10/11
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) (to build/run from source)
- WebView2 Runtime (preinstalled on most Windows 10/11 machines)

### Build and run from source

```bash
git clone https://github.com/CivilTechSource/xlink-viewer.git
cd xlink-viewer
dotnet restore
dotnet build
dotnet run --project XLinkViewer.csproj
```

### Running with VS Code

1. Open the project folder in VS Code
2. Press `Ctrl+Shift+P` → "Tasks: Run Task" → **Run XLink Viewer**

## Usage Guide

1. **Select Project Folder** — click "Browse..." and choose the folder containing your DWG files
2. **Start Scan** — recursively analyzes every DWG file for Xref relationships
3. **Explore References** — select a file to see every drawing that references it
4. **Visualize** — click **Show Graph View** to open the interactive Cytoscape.js graph; click
   a node to highlight its connections, search for a specific file, or export a PNG
5. **Create Checklists** — select one or more drawings and click "Create Checklist" to generate a
   Text or Markdown revision checklist listing every dependent drawing

## How It Works

1. Recursively discovers every `*.dwg` file under the selected folder
2. Opens each file with [ACadSharp](https://github.com/DomCR/ACadSharp) and reads its Block Table
   Records
3. Extracts file paths from blocks flagged as external references (`XRef`)
4. Builds a reverse lookup table: for each drawing, which other drawings reference it
5. Feeds that data into the Cytoscape.js graph and the checklist generator

## Dependencies

- **[ACadSharp](https://github.com/DomCR/ACadSharp)** (3.4.2) — pure C# DWG file reader
- **[CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)** (8.4.0) — MVVM helpers
- **Microsoft.Web.WebView2** (1.0.2792.45) — embeds the Cytoscape.js graph view
- **Microsoft.Msagl** (1.1.6) — legacy graph layout dependency, no longer used by the UI

## Contributing

Contributions are very welcome — this is a small tool with plenty of room to grow (see
[Updates.md](Updates.md) for a backlog of ideas: theming, filters, path-finding, save/load views,
and more).

- **Found a bug or have an idea?** [Open an issue](https://github.com/CivilTechSource/xlink-viewer/issues)
- **Want to contribute code?** Fork the repo, create a branch, and open a pull request
- **Just find it useful?** Star the repo ⭐ — it helps others discover it

## License

Released under the [MIT License](LICENSE). Third-party dependencies (ACadSharp,
CommunityToolkit.Mvvm, Microsoft MSAGL) carry their own MIT licenses.

## Support

- **DWG reading issues** — check the [ACadSharp](https://github.com/DomCR/ACadSharp) documentation
- **App bugs** — check the status bar for error messages, then [open an issue](https://github.com/CivilTechSource/xlink-viewer/issues)
