using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RecordFCS_Alt.Models
{
    [MetadataType(typeof(MostrarArchivoMetadata))]
    public partial class MostrarArchivo
    {
        [Key]
        [Column(Order = 1)]
        [ForeignKey("TipoMostrarArchivo")]
        public Guid TipoMostrarArchivoID { get; set; }

        [Key]
        [Column(Order = 2)]
        [ForeignKey("ArchivoPieza")]
        public Guid ArchivoPiezaID { get; set; }

        public bool Status { get; set; }

        //vituales
        public virtual TipoMostrarArchivo TipoMostrarArchivo { get; set; }
        public virtual ArchivoPieza ArchivoPieza { get; set; }

    }

    public class MostrarArchivoMetadata
    {
        [Display(Name = "Estado")]
        public bool Status { get; set; }
    }
}