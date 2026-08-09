using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace GraphViewerApp.ViewModels;

/// <summary>
/// ViewModel for the Graph Viewer
/// </summary>
public partial class GraphViewModel : ObservableObject
{
    [ObservableProperty]
    private GraphData? _currentGraphData;

    [ObservableProperty]
    private bool _hasData;

    partial void OnCurrentGraphDataChanged(GraphData? value)
    {
        HasData = value != null && value.Nodes.Count > 0;
    }

    /// <summary>
    /// Generates sample data for testing the graph viewer
    /// </summary>
    public GraphData GenerateSampleData()
    {
        var nodes = new List<GraphNode>
        {
            new GraphNode 
            { 
                Id = "A-101", 
                Label = "A-101 (Floor Plan)", 
                Group = "Architectural",
                ReferencedByCount = 5,
                XrefCount = 2
            },
            new GraphNode 
            { 
                Id = "A-102", 
                Label = "A-102 (Details)", 
                Group = "Architectural",
                ReferencedByCount = 1,
                XrefCount = 0
            },
            new GraphNode 
            { 
                Id = "S-201", 
                Label = "S-201 (Structural)", 
                Group = "Structural",
                ReferencedByCount = 3,
                XrefCount = 1
            },
            new GraphNode 
            { 
                Id = "M-301", 
                Label = "M-301 (Mechanical)", 
                Group = "MEP",
                ReferencedByCount = 2,
                XrefCount = 2
            },
            new GraphNode 
            { 
                Id = "E-401", 
                Label = "E-401 (Electrical)", 
                Group = "MEP",
                ReferencedByCount = 1,
                XrefCount = 1
            },
            new GraphNode 
            { 
                Id = "P-501", 
                Label = "P-501 (Plumbing)", 
                Group = "MEP",
                ReferencedByCount = 0,
                XrefCount = 1
            },
            new GraphNode 
            { 
                Id = "BASE-MAP", 
                Label = "BASE-MAP", 
                Group = "Reference",
                ReferencedByCount = 8,
                XrefCount = 0
            }
        };

        var edges = new List<GraphEdge>
        {
            new GraphEdge { Id = "e1", Source = "A-101", Target = "BASE-MAP", Label = "xref" },
            new GraphEdge { Id = "e2", Source = "A-101", Target = "A-102", Label = "xref" },
            new GraphEdge { Id = "e3", Source = "S-201", Target = "BASE-MAP", Label = "xref" },
            new GraphEdge { Id = "e4", Source = "M-301", Target = "BASE-MAP", Label = "xref" },
            new GraphEdge { Id = "e5", Source = "M-301", Target = "A-101", Label = "xref" },
            new GraphEdge { Id = "e6", Source = "E-401", Target = "BASE-MAP", Label = "xref" },
            new GraphEdge { Id = "e7", Source = "P-501", Target = "M-301", Label = "xref" }
        };

        return new GraphData { Nodes = nodes, Edges = edges };
    }
}

/// <summary>
/// Represents the entire graph structure
/// </summary>
public class GraphData
{
    public List<GraphNode> Nodes { get; set; } = new();
    public List<GraphEdge> Edges { get; set; } = new();
}

/// <summary>
/// Represents a node in the graph (a DWG file)
/// </summary>
public class GraphNode
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Group { get; set; }
    public int ReferencedByCount { get; set; }
    public int XrefCount { get; set; }
    public string? FilePath { get; set; }
}

/// <summary>
/// Represents an edge in the graph (an Xref relationship)
/// </summary>
public class GraphEdge
{
    public string Id { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public string Label { get; set; } = "xref";
}
