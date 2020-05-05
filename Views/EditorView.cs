using CCoder.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
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

        // skeleton of text change handler
        public void TextChangeHandler(ref object sender, ref TextChangedEventArgs eventArgs)
        {
            List<String> changeList = new List<string>();
            foreach (TextChange textChange in eventArgs.Changes)
                changeList.Add(Document.Text.Substring(textChange.Offset, textChange.AddedLength));

            // the skeleton of the loop that should surf through the change list and apply
            // text highlighting (need to be reobserved when new TextBox will be implemented)
            for (int index = 0; ; index += "int".Length)
            {
                index = changeList[0].IndexOf("int", index);
                if (index == -1)
                    break;
            }
        }
    }
}
