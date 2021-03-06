﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace RecordFCS_Alt.Models
{
    public class MovimientoTempPieza
    {
        public MovimientoTempPieza()
        {
            SeMovio = false;
            EsPendiente = true;
            EnError = true;
            Orden = 0;
        }

        [Key]
        [Column(Order = 1)]
        [ForeignKey("MovimientoTemp")]
        public Guid MovimientoTempID { get; set; }

        [Key]
        [Column(Order = 2)]
        [ForeignKey("Pieza")]
        public Guid PiezaID { get; set; }

        public int Orden { get; set; }// = 0;

        public string Comentario { get; set; }

        public bool SeMovio { get; set; }// = false;

        public bool EsPendiente { get; set; }// = true;
        public bool EnError { get; set; }// = true;

        public string FolioPieza { get; set; } //Solo es para agilizar las vistas en movimietnos

        //Virtuales
        public virtual Pieza Pieza { get; set; }
        public virtual MovimientoTemp MovimientoTemp { get; set; }
    }
}
