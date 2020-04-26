using CCoder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CCoder.Views
{
    public class EditorView
    {
        public ICommand FormatCommand { get; }
        public Format Format { get; set; }
        public Document Document { get; set; }

        public EditorView(Document document)
        {
            Document = document;
            Format = new Format();

            FormatCommand = new RelayCommand(OpenStyleDialog);
        }
        
        private void OpenStyleDialog()
        {
            var fontDialog = new FontDialog();
            fontDialog.DataContext = Format;
            fontDialog.Show();
        }
    }
}
