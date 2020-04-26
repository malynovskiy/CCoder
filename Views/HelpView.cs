using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CCoder.Views
{
    public class HelpView : Observer
    {
        public ICommand HelpCommand { get; }

        public HelpView()
        {
            HelpCommand = new RelayCommand(DisplayAbout);
        }

        private void DisplayAbout()
        {

        }
    }
}
