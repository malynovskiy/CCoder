using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCoder.Models
{
    public class Document : Observer
    {
        public string Text
        {
            get { return _text; }
            set { onPropertyChanged(ref _text, value); }
        }
        public string FilePath
        {
            get { return _filePath; }
            set { onPropertyChanged(ref _filePath, value); }
        }
        public string FileName
        {
            get { return _fileName; }
            set { onPropertyChanged(ref _fileName, value); }
        }

        public bool isEmpty
        {
            get
            {
                if (string.IsNullOrEmpty(FileName) || 
                    string.IsNullOrEmpty(FilePath))
                {
                    return true;
                }

                return false;
            }
        }

        private string _text;
        private string _filePath;
        private string _fileName;
    }
}
