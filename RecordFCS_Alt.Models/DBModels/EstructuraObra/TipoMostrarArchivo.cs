using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RecordFCS_Alt.Models
{
    [MetadataType(typeof(TipoMostrarArchivoMetadata))]
    public partial class TipoMostrarArchivo
    {
        [Key]
        public Guid TipoMostrarArchivoID { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool Status { get; set; }

        //virtual
        public virtual ICollection<MostrarArchivo> MostrarArchivos { get; set; }
    }

    public class TipoMostrarArchivoMetadata
    {
        public Guid TipoMostrarArchivoID { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Requerido.")]
        [StringLength(127)]
        [Display(Name = "Tipo de mostrar archivo")]
        //[Remote("EsUnico", "TipoMostrar", HttpMethod = "POST", AdditionalFields = "TipoMostrarID", ErrorMessage = "Ya existe, intenta otro nombre.")]
        public string Nombre { get; set; }

        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Display(Name = "Estado")]
        public bool Status { get; set; }
    }
}