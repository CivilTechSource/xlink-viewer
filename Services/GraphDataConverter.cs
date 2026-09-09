using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XLinkViewer.Models;

namespace XLinkViewer.Services;

/// <summary>
/// Represents the entire graph structure for visualization
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

/// <summary>
/// Converts DWG file scan results into graph visualization data
/// </summary>
public class GraphDataConverter
{
    /// <summary>
    /// Converts a dictionary of DwgFileInfo objects to GraphData format
    /// </summary>
    public GraphData ConvertFromDwgFileInfo(Dictionary<string, DwgFileInfo> scannedFiles)
    {
        var graphData = new GraphData();
        var edgeId = 1;

        // Create nodes from each DWG file
        foreach (var kvp in scannedFiles)
        {
            var filePath = kvp.Key;
            var fileInfo = kvp.Value;

            var node = new GraphNode
            {
                Id = fileInfo.FileName,
                Label = fileInfo.FileName,
                FilePath = filePath,
                ReferencedByCount = fileInfo.ReferencedBy.Count,
                XrefCount = fileInfo.XrefPaths.Count,
                Group = DetermineGroup(fileInfo.FileName)
            };

            graphData.Nodes.Add(node);
        }

        // Create edges from Xref relationships
        foreach (var kvp in scannedFiles)
        {
            var fileInfo = kvp.Value;

            foreach (var xrefPath in fileInfo.XrefPaths)
            {
                var targetFileName = Path.GetFileNameWithoutExtension(xrefPath);

                // Only create edge if target node exists
                if (graphData.Nodes.Any(n => n.Id.Equals(targetFileName, StringComparison.OrdinalIgnoreCase)))
                {
                    var edge = new GraphEdge
                    {
                        Id = $"e{edgeId++}",
                        Source = fileInfo.FileName,
                        Target = targetFileName,
                        Label = "xref"
                    };

                    graphData.Edges.Add(edge);
                }
            }
        }

        return graphData;
    }

    /// <summary>
    /// Determines the group/discipline based on file naming conventions
    /// Common prefixes: A- (Architectural), S- (Structural), M- (Mechanical), 
    ///                  E- (Electrical), P- (Plumbing)
    /// </summary>
    private string DetermineGroup(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return "Unknown";

        var upperFileName = fileName.ToUpperInvariant();

        // Check for discipline prefixes
        if (upperFileName.StartsWith("A-") || upperFileName.StartsWith("AR-"))
            return "Architectural";

        if (upperFileName.StartsWith("S-") || upperFileName.StartsWith("ST-"))
            return "Structural";

        if (upperFileName.StartsWith("M-") || upperFileName.StartsWith("ME-") || upperFileName.StartsWith("HVAC"))
            return "MEP";

        if (upperFileName.StartsWith("E-") || upperFileName.StartsWith("EL-") || upperFileName.StartsWith("ELEC"))
            return "MEP";

        if (upperFileName.StartsWith("P-") || upperFileName.StartsWith("PL-") || upperFileName.StartsWith("PLUMB"))
            return "MEP";

        if (upperFileName.StartsWith("FP-") || upperFileName.StartsWith("FIRE"))
            return "MEP";

        // Common reference file names
        if (upperFileName.Contains("BASE") || upperFileName.Contains("SITE") || 
            upperFileName.Contains("SURVEY") || upperFileName.Contains("TOPO"))
            return "Reference";

        if (upperFileName.Contains("TITLE") || upperFileName.Contains("COVER"))
            return "Reference";

        return "Other";
    }

    /// <summary>
    /// Filters graph data to only show files in a specific directory
    /// </summary>
    public GraphData FilterByDirectory(GraphData graphData, string directoryPath)
    {
        var filteredNodes = graphData.Nodes
            .Where(n => n.FilePath != null && n.FilePath.StartsWith(directoryPath, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var filteredNodeIds = new HashSet<string>(filteredNodes.Select(n => n.Id));

        var filteredEdges = graphData.Edges
            .Where(e => filteredNodeIds.Contains(e.Source) && filteredNodeIds.Contains(e.Target))
            .ToList();

        return new GraphData
        {
            Nodes = filteredNodes,
            Edges = filteredEdges
        };
    }

    /// <summary>
    /// Filters graph to show only highly connected nodes
    /// </summary>
    public GraphData FilterByImportance(GraphData graphData, int minReferenceCount = 3)
    {
        var importantNodes = graphData.Nodes
            .Where(n => n.ReferencedByCount >= minReferenceCount || n.XrefCount >= minReferenceCount)
            .ToList();

        var importantNodeIds = new HashSet<string>(importantNodes.Select(n => n.Id));

        var filteredEdges = graphData.Edges
            .Where(e => importantNodeIds.Contains(e.Source) || importantNodeIds.Contains(e.Target))
            .ToList();

        return new GraphData
        {
            Nodes = importantNodes,
            Edges = filteredEdges
        };
    }
}
