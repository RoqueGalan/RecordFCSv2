using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RecordFCS_Alt.Models
{
    [MetadataType(typeof(AtributoMetadata))]
    public partial class Atributo
    {
        public Atributo()
        {
            EsValido = false;
        }

        [Key]
        public Guid AtributoID { get; set; }
        public int Orden { get; set; }
        public string NombreAlterno { get; set; }
        public bool Status { get; set; }

        public bool EsValido { get; set; }


        //Llaves Foraneas
        [ForeignKey("TipoPieza")]
        public Guid TipoPiezaID { get; set; }

        [ForeignKey("TipoAtributo")]
        public Guid TipoAtributoID { get; set; }


        //virtuales
        public virtual TipoPieza TipoPieza { get; set; }
        public virtual TipoAtributo TipoAtributo { get; set; }
        public virtual ICollection<MostrarAtributo> MostrarAtributos { get; set; }

        //public virtual ICollection<AtributoPieza> AtributoPiezas { get; set; }

    }

    public class AtributoMetadata
    {
        public Guid AtributoID { get; set; }

        [StringLength(127)]
        [Display(Name = "Nombre Alterno")]
        public string NombreAlterno { get; set; }

        [Display(Name = "Estado")]
        public bool Status { get; set; }


        [Display(Name = "Valido")]
        public bool EsValido { get; set; }


        [Display(Name = "Tipo de Pieza")]
        public Guid TipoPiezaID { get; set; }

        [Display(Name = "Tipo de Atributo")]
        [Remote("EsUnico", "Atributo", HttpMethod = "POST", AdditionalFields = "TipoPiezaID,AtributoID", ErrorMessage = "Atributo ya existe.")]
        public Guid TipoAtributoID { get; set; }
    }


}