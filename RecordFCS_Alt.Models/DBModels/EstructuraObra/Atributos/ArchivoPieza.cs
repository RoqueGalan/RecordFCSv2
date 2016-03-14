using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecordFCS_Alt.Models
{
    [MetadataType(typeof(ArchivoPiezaMetadata))]
    public partial class ArchivoPieza
    {
        [Key]
        public Guid ArchivoPiezaID { get; set; }

        public int Orden { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }

        [DataType(DataType.Url)]
        public string Url { get; set; }

        public string NombreArchivo { get; set; }
        public string Extension { get; set; }

        public bool Status { get; set; }


        //Llaves foraneas
        [ForeignKey("Pieza")]
        public Guid PiezaID { get; set; }

        [ForeignKey("TipoArchivo")]
        public Guid TipoArchivoID { get; set; }


        //Virtuales
        public virtual Pieza Pieza { get; set; }
        public virtual TipoArchivo TipoArchivo { get; set; }
        public virtual ICollection<MostrarArchivo> MostrarArchivos { get; set; }

        //No mapeados
        [NotMapped]
        public string Ruta
        {
            get
            {
                return TipoArchivo.Ruta + "" + NombreArchivo + Extension;
            }

            set { }
        }

        [NotMapped]
        public string RutaThumb
        {
            get 
            {
                return TipoArchivo.Ruta + "thumb/" + NombreArchivo + Extension;
            }

            set { }
        }

       
    }

    public class ArchivoPiezaMetadata
    {
        public Guid ArchivoPiezaID { get; set; }

        public int Orden { get; set; }

        [StringLength(127)]
        [Display(Name = "Título")]
        public string Titulo { get; set; }

        [StringLength(255)]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [StringLength(63)]
        public string NombreArchivo { get; set; }

        [Display(Name = "Estado")]
        public bool Status { get; set; }

    }
}