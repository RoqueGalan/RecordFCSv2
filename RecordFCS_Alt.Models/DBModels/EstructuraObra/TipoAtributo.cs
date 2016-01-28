using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RecordFCS_Alt.Models
{
    [MetadataType(typeof(TipoAtributoMetadata))]
    public partial class TipoAtributo
    {
        [Key]
        public Guid TipoAtributoID { get; set; }

        public int Orden { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }

        public bool EsLista { get; set; }
        public bool EsMultipleValor { get; set; }
        public bool EsGenerico { get; set; }
        public string PerteneceA { get; set; }
        public string TablaSQL { get; set; }

        public bool EnBuscador { get; set; }

        public bool Status { get; set; }



        public string DatoCS { get; set; }
        public string HTMLParametros { get; set; }



        //Anteriores
        public string Temp { get; set; }


        //Virtuales
        public virtual ICollection<Atributo> Atributos { get; set; }
        public virtual ICollection<ListaValor> ListaValores { get; set; }
    }

    public class TipoAtributoMetadata
    {
        public Guid TipoAtributoID { get; set; }

        [Required(ErrorMessage = "Requerido.")]
        public int Orden { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Requerido.")]
        [StringLength(127)]
        [Display(Name = "Tipo de Atributo")]
        [Remote("EsUnico", "TipoAtributo", HttpMethod = "POST", AdditionalFields = "TipoAtributoID", ErrorMessage = "Ya existe, intenta otro nombre.")]
        public string Nombre { get; set; }

        [StringLength(127)]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }


        [Display(Name = "Generico")]
        public bool EsGenerico { get; set; }
        [Display(Name = "Catálogo")]
        public bool EsLista { get; set; }

        [Display(Name = "Multivalor")]
        public bool EsMultipleValor { get; set; }

        [StringLength(63)]
        [Display(Name = "Pertenece a")]
        public string PerteneceA { get; set; }
        [StringLength(63)]
        [Display(Name = "En la tabla")]
        public string TablaSQL { get; set; }

        [Display(Name = "En Buscador")]
        public bool EnBuscador { get; set; }

        [Display(Name = "Estado")]
        public bool Status { get; set; }


        [Display(Name = "Dato C#")]
        public string DatoCS { get; set; }
        [Display(Name = "Parametros HTML")]
        [StringLength(255)]
        public string HTMLParametros { get; set; }



        //Anteriores
        [Display(Name = "Nombre Único")]
        [StringLength(63)]
        public string Temp { get; set; }
    }
}