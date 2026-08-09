namespace XLinkViewer.Models
{
    /// <summary>
    /// Represents a parent-child relationship between DWG files
    /// </summary>
    public class XrefRelationship
    {
        public string ParentFile { get; set; } = string.Empty;
        public string ChildFile { get; set; } = string.Empty;
        public string XrefPath { get; set; } = string.Empty;
        public string RelationType { get; set; } = "Xref"; // Could be Extended for other relationship types
        
        public XrefRelationship(string parentFile, string childFile, string xrefPath)
        {
            ParentFile = parentFile;
            ChildFile = childFile;
            XrefPath = xrefPath;
        }

        public override string ToString()
        {
            return $"{ParentFile} → {ChildFile}";
        }
    }
}