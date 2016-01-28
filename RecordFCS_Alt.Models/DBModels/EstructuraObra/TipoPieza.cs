using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RecordFCS_Alt.Models
{
    [MetadataType(typeof(TipoPiezaMetadata))]
    public partial class TipoPieza
    {
        public TipoPieza()
        {
            EsValido = false;
        }

        [Key]
        public Guid TipoPiezaID { get; set; }

        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Prefijo { get; set; }
        public int Orden { get; set; }
        public bool EsPrincipal { get; set; }
        public bool Status { get; set; }

        public bool EsValido { get; set; }


        //Llaves Foraneas
        [ForeignKey("TipoObra")]
        public Guid TipoObraID { get; set; }

        [ForeignKey("TipoPiezaPadre")]
        public Guid? TipoPiezaPadreID { get; set; }



        //Anteriores
        public string Temp { get; set; }

        //Virtuales
        public virtual TipoObra TipoObra { get; set; }
        public virtual ICollection<Atributo> Atributos { get; set; }
        public virtual ICollection<Pieza> Piezas { get; set; }

        public virtual TipoPieza TipoPiezaPadre { get; set; }
        public virtual ICollection<TipoPieza> TipoPiezasHijas { get; set; }


    }



    public class TipoPiezaMetadata
    {
        public Guid TipoPiezaID { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Requerido.")]
        [StringLength(63)]
        [Display(Name = "Tipo de Pieza")]
        [Remote("EsUnico", "TipoPieza", HttpMethod = "POST", AdditionalFields = "TipoPiezaID,TipoObraID,TipoPiezaPadreID", ErrorMessage = "Ya existe, intenta otro nombre.")]
        public string Nombre { get; set; }

        [StringLength(255)]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Requerido.")]
        [StringLength(3)]
        [Display(Name = "Letra")]
        public string Prefijo { get; set; }

        [Required(ErrorMessage = "Requerido.")]
        public int Orden { get; set; }

        [Display(Name = "Estado")]
        public bool Status { get; set; }


        [Display(Name = "Valido")]
        public bool EsValido { get; set; }


        public Guid TipoObraID { get; set; }
        public Guid? TipoPiezaPadreID { get; set; }


        [StringLength(63)]
        public string Temp { get; set; }
    }
}