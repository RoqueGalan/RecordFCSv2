using RecordFCS_Alt.Models.ViewsModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RecordFCS_Alt.Models.ViewModels
{
    public class itemImprimir
    {
        public string TipoImprimir { get; set; }

        public bool DatosCompletos { get; set; }
        public bool ConPiezasAdicionales { get; set; }

        public virtual List<itemPiezaMini> ListaPiezas{ get; set; }
    }
}