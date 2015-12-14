using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PruebasWPF
{
    class Obra : INotifyPropertyChanged
    {
        private string _NoInventario = "";
        public string NoInventario
        {
            get
            {
                return _NoInventario;
            }
            set
            {
                _NoInventario = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("NoInventario"));
            }

        }


        private string _Titulo = "";
        public string Titulo
        {
            get
            {
                return _Titulo;
            }
            set
            {
                _Titulo = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Titulo"));
            }

        }






        public event PropertyChangedEventHandler PropertyChanged;
    }
}
