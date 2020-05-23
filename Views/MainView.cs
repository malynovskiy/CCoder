using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCoder.Models;

namespace CCoder.Views
{
    public class MainView
    {
        private Document _document;

        public EditorView Editor { get; set; }
        public FileView File { get; set; }
        public HelpView Help { get; set; }

        public ref Document getCurrentDocument() { return ref _document; }

        public MainView()
        {
            _document = new Document();

            Editor = new EditorView(_document);
            Help = new HelpView();
            File = new FileView(_document);
        }
    }
}
