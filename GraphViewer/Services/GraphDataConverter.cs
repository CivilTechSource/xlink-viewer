using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GraphViewerApp.ViewModels;

namespace GraphViewerApp.Services;

/// <summary>
/// Service to convert DWG file scan results into graph visualization data
/// This will be used when integrating with the main XLinkViewer application
/// </summary>
public class GraphDataConverter
{
    /// <summary>
    /// Converts a dictionary of DwgFileInfo objects to GraphData format for visualization
    /// NOTE: This assumes DwgFileInfo has these properties:
    ///   - FilePath (string)
    ///   - FileName (string)  
    ///   - XrefPaths (List&lt;string&gt;)
    ///   - ReferencedBy (List&lt;string&gt;)
    /// </summary>
    /// <param name="scannedFiles">Dictionary of scanned DWG files</param>
    /// <returns>GraphData object ready for visualization</returns>
    public GraphData ConvertFromDwgFileInfo<T>(Dictionary<string, T> scannedFiles) where T : class
    {
        var graphData = new GraphData();
        var edgeId = 1;

        // Create nodes from each DWG file
        foreach (var kvp in scannedFiles)
        {
            var filePath = kvp.Key;
            var fileInfo = kvp.Value;

            // Use reflection to get properties (in case DwgFileInfo is in different assembly)
            var fileNameProp = fileInfo.GetType().GetProperty("FileName");
            var xrefPathsProp = fileInfo.GetType().GetProperty("XrefPaths");
            var referencedByProp = fileInfo.GetType().GetProperty("ReferencedBy");

            var fileName = fileNameProp?.GetValue(fileInfo) as string ?? Path.GetFileNameWithoutExtension(filePath);
            var xrefPaths = xrefPathsProp?.GetValue(fileInfo) as IList<string> ?? new List<string>();
            var referencedBy = referencedByProp?.GetValue(fileInfo) as IList<string> ?? new List<string>();

            var node = new GraphNode
            {
                Id = fileName,
                Label = fileName,
                FilePath = filePath,
                ReferencedByCount = referencedBy.Count,
                XrefCount = xrefPaths.Count,
                Group = DetermineGroup(fileName)
            };

            graphData.Nodes.Add(node);
        }

        // Create edges from Xref relationships
        foreach (var kvp in scannedFiles)
        {
            var fileInfo = kvp.Value;

            var fileNameProp = fileInfo.GetType().GetProperty("FileName");
            var xrefPathsProp = fileInfo.GetType().GetProperty("XrefPaths");

            var fileName = fileNameProp?.GetValue(fileInfo) as string ?? string.Empty;
            var xrefPaths = xrefPathsProp?.GetValue(fileInfo) as IList<string> ?? new List<string>();

            foreach (var xrefPath in xrefPaths)
            {
                var targetFileName = Path.GetFileNameWithoutExtension(xrefPath);

                // Only create edge if target node exists
                if (graphData.Nodes.Any(n => n.Id.Equals(targetFileName, StringComparison.OrdinalIgnoreCase)))
                {
                    var edge = new GraphEdge
                    {
                        Id = $"e{edgeId++}",
                        Source = fileName,
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
    /// Filters graph data to only show files in a specific directory or matching criteria
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
    /// Filters graph to show only nodes that are highly connected (referenced by many files)
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
