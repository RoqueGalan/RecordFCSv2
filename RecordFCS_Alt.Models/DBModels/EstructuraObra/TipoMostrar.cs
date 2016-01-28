using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RecordFCS_Alt.Models
{
    [MetadataType(typeof(TipoMostrarMetadata))]
    public partial class TipoMostrar
    {
        [Key]
        public Guid TipoMostrarID { get; set; }
        public string Nombre { get; set; }
        public bool Status { get; set; }

        //virtual
        public virtual ICollection<MostrarAtributo> MostrarAtributos { get; set; }
    }

    public class TipoMostrarMetadata
    {
        public Guid TipoMostrarID { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Requerido.")]
        [StringLength(127)]
        [Display(Name = "Tipo de Mostrar")]
        [Remote("EsUnico", "TipoMostrar", HttpMethod = "POST", AdditionalFields = "TipoMostrarID", ErrorMessage = "Ya existe, intenta otro nombre.")]
        public string Nombre { get; set; }

        [Display(Name = "Estado")]
        public bool Status { get; set; }
    }
}