# DWG Xref Relationship Mapper Project

A C# WPF application that scans DWG files to build reverse lookup tables for external references using the ACadSharp library.

## Project Requirements
- **Target**: Desktop WPF application 
- **Language**: C# (.NET)
- **Key Library**: ACadSharp for DWG file reading
- **Visualization**: Microsoft.Msagl.WpfGraphControl for graph display
- **Pattern**: MVVM architecture

## Features
- Recursive DWG file scanning in project folders
- External reference (Xref) extraction from Block Table Records
- Reverse lookup mapping (show which files reference selected drawing(s))
- **Multi-drawing selection support** for impact analysis across multiple source drawings
- Interactive TreeView/Graph visualization
- Folder selection browse functionality
- **Enhanced checklist generation** for single or multiple drawing revisions

## Project Structure
- Models: DwgFileInfo, XrefRelationship data models
- Services: XrefScanner service for ACadSharp integration  
- ViewModels: MainViewModel with MVVM pattern
- Views: MainWindow with TreeView and Graph controls

## Checklist Progress

- [x] Verify copilot-instructions.md file created
- [x] Clarify Project Requirements (provided by user)
- [x] Scaffold the Project (WPF project created with dotnet CLI)
- [x] Customize the Project (Added NuGet packages, created MVVM structure, implemented XrefScanner)
- [x] Install Required Extensions (No specific extensions required)
- [x] Compile the Project (Build successful)
- [x] Create and Run Task (Task created: "Run DWG Xref Mapper")
- [x] Launch the Project (Run task available)
- [x] Ensure Documentation Complete (README.md created with comprehensive documentation)

## Project Status
✅ **COMPLETE** - DWG Xref Relationship Mapper is ready for use!

### To Run the Application:
1. Use VS Code Task: Ctrl+Shift+P → "Tasks: Run Task" → "Run DWG Xref Mapper"
2. Or use terminal: `dotnet run --project DwgXrefMapper.csproj`

The application provides a complete WPF interface for scanning DWG files and analyzing external reference relationships.