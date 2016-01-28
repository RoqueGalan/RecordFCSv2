using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RecordFCS_Alt.Models
{
    [MetadataType(typeof(TipoArchivoMetadata))]
    public partial class TipoArchivo
    {
        public TipoArchivo()
        {
            EsValido = false;
        }
        [Key]
        public Guid TipoArchivoID { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string ExtensionesAceptadas { get; set; }
        public string CarpetaRuta { get; set; }

        public bool Status { get; set; }

        public bool EsValido { get; set; }

        public int Orden { get; set; }
        public string Default { get; set; }
        //Anteriores
        public string Temp { get; set; }

        public string Icono { get; set; }

        //Virtuales
        public virtual ICollection<ArchivoPieza> Archivos { get; set; }

        [NotMapped]
        public string RutaServidor
        {
            get
            {
                return "~";
            }
        }

        [NotMapped]
        public string RutaRaiz
        {
            get
            {
                return "/Content/docxPieza";
                //return "/Content/DirVirtual";
            }
        }

        [NotMapped]
        public string Ruta
        {
            get
            {
                return RutaRaiz + "/" + CarpetaRuta + "/";
            }
            set{
            }
        }

    }

    public class TipoArchivoMetadata
    {
        public Guid TipoArchivoID { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Requerido.")]
        [StringLength(255)]
        [Display(Name = "Tipo de archivo")]
        public string Nombre { get; set; }

        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Display(Name = "Extensiones aceptadas")]
        public string ExtensionesAceptadas { get; set; }


        [Display(Name = "Nombre de la carpeta")]
        public string CarpetaRuta { get; set; }

        [Display(Name = "Estado")]
        public bool Status { get; set; }

        [Display(Name = "Valido")]
        public bool EsValido { get; set; }


        [StringLength(63)]
        public string Temp { get; set; }
    }


}