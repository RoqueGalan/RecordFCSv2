using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RecordFCS_Alt.Models
{
    [MetadataType(typeof(TecnicaPiezaMetadata))]
    public partial class TecnicaPieza
    {
        [Key]
        [Column(Order = 1)]
        [ForeignKey("Pieza")]
        public Guid PiezaID { get; set; }

        [Key]
        [Column(Order = 2)]
        [ForeignKey("TipoTecnica")]
        public Guid TipoTecnicaID { get; set; }


        public bool Status { get; set; }


        public bool EsValido { get; set; }



        // llave foranea
        [ForeignKey("Tecnica")]
        public Guid TecnicaID { get; set; }


        //virtual
        public virtual Pieza Pieza { get; set; }
        public virtual TipoTecnica TipoTecnica { get; set; }
        public virtual Tecnica Tecnica { get; set; }
    }

    public class TecnicaPiezaMetadata
    {
        [Display(Name = "Estado")]
        public bool Status { get; set; }

        [Display(Name = "Valido")]
        public bool EsValido { get; set; }

    }
}