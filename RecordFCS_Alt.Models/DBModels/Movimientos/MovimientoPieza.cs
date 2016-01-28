using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RecordFCS_Alt.Models
{
    public class MovimientoPieza
    {
        [Key]
        [Column(Order = 1)]
        [ForeignKey("Movimiento")]
        public Guid MovimientoID { get; set; }

        [Key]
        [Column(Order = 2)]
        [ForeignKey("Pieza")]
        public Guid PiezaID { get; set; }

        public string Comentario { get; set; }
        public bool estaDisponible { get; set; }

        //Virtuales
        public virtual Pieza Pieza { get; set; }
        public virtual Movimiento Movimiento { get; set; }
    }
}