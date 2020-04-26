using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CCoder
{
    public class Observer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void onPropertyChanged<T>(ref T property, T value, 
            [CallerMemberName] string propertyName = "")
        {
            property = value;

            var handler = PropertyChanged;
            if(handler != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
