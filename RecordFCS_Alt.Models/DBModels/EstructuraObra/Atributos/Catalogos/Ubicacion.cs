using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RecordFCS_Alt.Models
{
    [MetadataType(typeof(UbicacionMetadata))]
    public partial class Ubicacion
    {
        public Ubicacion()
        {
            EsValido = false;
        }

        [Key]
        public Guid UbicacionID { get; set; }

        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool Status { get; set; }

        public bool EsValido { get; set; }

        //Anteriores
        public string Temp { get; set; }


     
        //Virtuales
        public virtual ICollection<Pieza> Piezas { get; set; }

    }

    public class UbicacionMetadata
    {
        public Guid UbicacionID { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Requerido.")]
        [StringLength(127)]
        [Display(Name = "Ubicación")]
        [Remote("EsUnico", "Ubicacion", HttpMethod = "POST", AdditionalFields = "UbicacionID", ErrorMessage = "Ya existe, intenta otro nombre.")]
        public string Nombre { get; set; }

        [StringLength(127)]
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