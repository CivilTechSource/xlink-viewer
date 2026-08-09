using ACadSharp;
using ACadSharp.IO;
using ACadSharp.Tables;
using XLinkViewer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace XLinkViewer.Services
{
    /// <summary>
    /// Service for scanning DWG files and extracting external reference information using ACadSharp
    /// </summary>
    public class XrefScanner
    {
        public event Action<string>? ProgressChanged;
        public event Action<string, string>? ErrorOccurred;

        /// <summary>
        /// Scans all DWG files in the specified directory and builds a relationship map
        /// </summary>
        /// <param name="rootDirectory">The root directory to scan</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Dictionary mapping file paths to DwgFileInfo objects</returns>
        public async Task<Dictionary<string, DwgFileInfo>> ScanDirectoryAsync(
            string rootDirectory, 
            CancellationToken cancellationToken = default)
        {
            var results = new Dictionary<string, DwgFileInfo>();
            
            try
            {
                ProgressChanged?.Invoke("Finding DWG files...");
                var dwgFiles = Directory.GetFiles(rootDirectory, "*.dwg", SearchOption.AllDirectories);
                
                ProgressChanged?.Invoke($"Found {dwgFiles.Length} DWG files. Starting scan...");

                for (int i = 0; i < dwgFiles.Length; i++)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    var filePath = dwgFiles[i];
                    var fileName = Path.GetFileName(filePath);
                    
                    ProgressChanged?.Invoke($"Scanning {fileName} ({i + 1}/{dwgFiles.Length})");
                    
                    var fileInfo = await ScanDwgFileAsync(filePath, cancellationToken);
                    results[filePath] = fileInfo;
                    
                    // Small delay to allow UI updates
                    await Task.Delay(10, cancellationToken);
                }

                ProgressChanged?.Invoke("Building reverse lookup relationships...");
                BuildReverseLookup(results);
                
                ProgressChanged?.Invoke("Scan complete!");
                return results;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke("General scan error", ex.Message);
                return results;
            }
        }

        /// <summary>
        /// Scans a single DWG file for external references
        /// </summary>
        /// <param name="filePath">Path to the DWG file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>DwgFileInfo object with Xref information</returns>
        public async Task<DwgFileInfo> ScanDwgFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            var fileInfo = new DwgFileInfo(filePath);
            
            await Task.Run(() =>
            {
                try
                {
                    using var stream = File.OpenRead(filePath);
                    
                    var cadDocument = DwgReader.Read(stream);
                    
                    // Access the Block Table to find external references
                    foreach (var blockRecord in cadDocument.BlockRecords)
                    {
                        // Check if this block record has external reference properties
                        // External references are blocks that have the XRef flag set or have an XRefPath
                        if (blockRecord.Flags.HasFlag(ACadSharp.Blocks.BlockTypeFlags.XRef) || 
                            !string.IsNullOrEmpty(blockRecord.BlockEntity.XRefPath))
                        {
                            var xrefPath = blockRecord.BlockEntity.XRefPath;
                            if (!string.IsNullOrWhiteSpace(xrefPath))
                            {
                                fileInfo.XrefPaths.Add(xrefPath);
                            }
                        }
                    }
                    
                    fileInfo.IsScanned = true;
                }
                catch (Exception ex)
                {
                    fileInfo.ErrorMessage = ex.Message;
                    ErrorOccurred?.Invoke(Path.GetFileName(filePath), ex.Message);
                }
            }, cancellationToken);

            return fileInfo;
        }

        /// <summary>
        /// Builds reverse lookup relationships - populates ReferencedBy lists
        /// </summary>
        /// <param name="fileInfos">Dictionary of all scanned file info objects</param>
        private void BuildReverseLookup(Dictionary<string, DwgFileInfo> fileInfos)
        {
            // First, clear all ReferencedBy lists
            foreach (var fileInfo in fileInfos.Values)
            {
                fileInfo.ReferencedBy.Clear();
            }

            // Build a lookup by filename for easier matching
            var filesByName = new Dictionary<string, List<DwgFileInfo>>();
            foreach (var fileInfo in fileInfos.Values)
            {
                var fileName = Path.GetFileName(fileInfo.FilePath).ToLowerInvariant();
                if (!filesByName.ContainsKey(fileName))
                {
                    filesByName[fileName] = new List<DwgFileInfo>();
                }
                filesByName[fileName].Add(fileInfo);
            }

            // Now build the reverse relationships
            foreach (var fileInfo in fileInfos.Values)
            {
                foreach (var xrefPath in fileInfo.XrefPaths)
                {
                    // Extract filename from the xref path
                    var xrefFileName = Path.GetFileName(xrefPath).ToLowerInvariant();
                    
                    // Find matching files
                    if (filesByName.ContainsKey(xrefFileName))
                    {
                        foreach (var referencedFile in filesByName[xrefFileName])
                        {
                            if (!referencedFile.ReferencedBy.Contains(fileInfo.FilePath))
                            {
                                referencedFile.ReferencedBy.Add(fileInfo.FilePath);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets a list of relationship objects for visualization purposes
        /// </summary>
        /// <param name="fileInfos">Dictionary of all scanned file info objects</param>
        /// <returns>List of XrefRelationship objects</returns>
        public List<XrefRelationship> GetRelationships(Dictionary<string, DwgFileInfo> fileInfos)
        {
            var relationships = new List<XrefRelationship>();
            
            foreach (var fileInfo in fileInfos.Values)
            {
                foreach (var xrefPath in fileInfo.XrefPaths)
                {
                    relationships.Add(new XrefRelationship(fileInfo.FilePath, xrefPath, xrefPath));
                }
            }
            
            return relationships;
        }
    }
}