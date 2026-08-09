using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace XLinkViewer.Models
{
    /// <summary>
    /// Represents information about a DWG file and its external references
    /// </summary>
    public class DwgFileInfo : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string FilePath { get; set; } = string.Empty;
        public string FileName => Path.GetFileNameWithoutExtension(FilePath);
        public string Directory => Path.GetDirectoryName(FilePath) ?? string.Empty;
        public List<string> XrefPaths { get; set; } = new List<string>();
        public List<string> ReferencedBy { get; set; } = new List<string>();
        public bool IsScanned { get; set; }
        public string? ErrorMessage { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public DwgFileInfo(string filePath)
        {
            FilePath = filePath;
        }

        public override string ToString()
        {
            return FileName;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}