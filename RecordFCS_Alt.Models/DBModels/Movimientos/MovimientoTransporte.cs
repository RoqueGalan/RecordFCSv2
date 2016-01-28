using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RecordFCS_Alt.Models
{
    public class MovimientoTransporte
    {
        [Key]
        public int MovimientoTransporteID { get; set; }


        public string EmpresaNombre { get; set; }
        public string Recorrido { get; set; }
        public string Horario { get; set; }
        public string Nota { get; set; }

        //virtual
    }
}