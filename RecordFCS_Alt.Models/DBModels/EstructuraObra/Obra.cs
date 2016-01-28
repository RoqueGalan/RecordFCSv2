using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RecordFCS_Alt.Models
{
    [MetadataType(typeof(ObraMetadata))]
    public partial class Obra
    {
        [Key]
        public Guid ObraID { get; set; }

        public int NumeroFolio { get; set; }

        public DateTime FechaRegistro { get; set; }


        //Llaves Foraneas
        [ForeignKey("TipoObra")]
        public Guid TipoObraID { get; set; }

        [ForeignKey("LetraFolio")]
        public int LetraFolioID { get; set; }


        public bool Status { get; set; }


        //Anteriores
        public string Temp { get; set; }


        //Virtuales
        public virtual TipoObra TipoObra { get; set; }
        public virtual LetraFolio LetraFolio { get; set; }
        public virtual ICollection<Pieza> Piezas { get; set; }
    }


    public class ObraMetadata
    {

        public Guid ObraID { get; set; }

        [Display(Name = "Número de Folio")]
        [Required(ErrorMessage = "Requerido.")]
        [Remote("EsUnico", "Obra", HttpMethod = "POST", AdditionalFields = "LetraFolioID,ObraID", ErrorMessage = "Ya existe, intenta otro número o letra.")]
        public int NumeroFolio { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Fecha de Registro")]
        [Required(ErrorMessage = "Requerido.")]
        public DateTime FechaRegistro { get; set; }


        //Llaves Foraneas
        public Guid TipoObraID { get; set; }
        public int LetraFolioID { get; set; }



        [Display(Name = "Estado")]
        public bool Status { get; set; }


        [StringLength(63)]
        public string Temp { get; set; }
    }

}