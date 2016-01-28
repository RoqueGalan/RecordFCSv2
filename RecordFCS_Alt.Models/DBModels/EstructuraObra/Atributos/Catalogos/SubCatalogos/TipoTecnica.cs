using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RecordFCS_Alt.Models
{
    [MetadataType(typeof(TipoTecnicaMetadata))]
    public partial class TipoTecnica
    {
        public TipoTecnica()
        {
            EsValido = false;
        }

        [Key]
        public Guid TipoTecnicaID { get; set; }

        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool Status { get; set; }

        public bool EsValido { get; set; }

        public string Temp { get; set; }


        //virtual
        public virtual ICollection<Tecnica> Tecnicas { get; set; }

    }

    public class TipoTecnicaMetadata
    {
        public Guid TipoTecnicaID { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Requerido.")]
        [StringLength(255)]
        [Display(Name = "Tipo de Técnica")]
        [Remote("EsUnico", "TipoTecnica", HttpMethod = "POST", AdditionalFields = "TipoTecnicaID", ErrorMessage = "Ya existe, intenta otro nombre.")]
        public string Nombre { get; set; }

        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Display(Name = "Estado")]
        public bool Status { get; set; }

        [Display(Name = "Valido")]
        public bool EsValido { get; set; }


        [StringLength(63)]
        public string Temp { get; set; }
    }
}