using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace CCoder.Models
{
    public class Format : Observer
    {
        public Format() : base()
        {
            // setting default format values
            _family = new FontFamily("Consolas");
            _size = 14.5f;
        }

        public FontStyle Style
        {
            get { return _style; }
            set { onPropertyChanged(ref _style, value); }
        }
        public FontWeight Weight
        {
            get { return _weight; }
            set { onPropertyChanged(ref _weight, value); }
        }
        public FontFamily Family
        {
            get { return _family; }
            set { onPropertyChanged(ref _family, value); }
        }
        public TextWrapping Wrap
        {
            get { return _wrap; }
            set 
            {
                onPropertyChanged(ref _wrap, value);
                isWrapped = value == TextWrapping.Wrap ? true : false;
            }
        }
        public bool isWrapped
        {
            get { return _isWrapped; }
            set { onPropertyChanged(ref _isWrapped, value); }
        }
        public double Size
        {
            get { return _size; }
            set { onPropertyChanged(ref _size, value); }
        }

        private FontStyle _style;
        private FontWeight _weight;
        private FontFamily _family;
        private TextWrapping _wrap;
        private double _size;

        private bool _isWrapped;
    }
}
