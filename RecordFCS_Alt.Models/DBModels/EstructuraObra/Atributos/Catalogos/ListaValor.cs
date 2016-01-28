using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace RecordFCS_Alt.Models
{
    [MetadataType(typeof(ListaValorMetadata))]
    public partial class ListaValor
    {
        public ListaValor()
        {
            EsValido = false;
        }

        [Key]
        public Guid ListaValorID { get; set; }

        public string Valor { get; set; }

        public bool Status { get; set; }

        public bool EsValido { get; set; }

        public string Temp { get; set; }

        //llave foranea

        [ForeignKey("TipoAtributo")]
        public Guid TipoAtributoID { get; set; }


        //virtual
        public virtual TipoAtributo TipoAtributo { get; set; }
        public virtual ICollection<AtributoPieza> AtributoPiezas { get; set; }

    }

    public class ListaValorMetadata
    {
        public Guid ListaValorID { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Requerido.")]
        [Display(Name = "Valor")]
        [Remote("EsUnico", "ListaValor", HttpMethod = "POST", AdditionalFields = "ListaValorID,TipoAtributoID", ErrorMessage = "Ya existe, intenta otro valor.")]
        public string Valor { get; set; }
        
        [Display(Name = "Estado")]
        public bool Status { get; set; }

        [Display(Name = "Valido")]
        public bool EsValido { get; set; }

        
        [StringLength(63)]
        public string Temp { get; set; }

        public Guid TipoAtributoID { get; set; }
    }
}
