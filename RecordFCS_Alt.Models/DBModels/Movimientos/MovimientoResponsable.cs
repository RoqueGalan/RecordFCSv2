using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RecordFCS_Alt.Models
{
    public class MovimientoResponsable
    {
        [Key]
        public int MovimientoResponsableID { get; set; }

        //[ForeignKey("Movimiento")]
        //public Guid MovimientoID { get; set; }

        public string Nombre { get; set; }
        public string Institucion { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? FechaSalida { get; set; }

        //public virtual Movimiento Movimiento { get; set; }

    }
}