using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RecordFCS_Alt.Models
{
    [MetadataType(typeof(AutorMetadata))]
    public partial class Autor
    {
        public Autor()
        {
            EsValido = false;
        }

        [Key]
        public Guid AutorID { get; set; }
        public string Seudonimo { get; set; }
        public string Nombre { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public string NacimientoLugar { get; set; }
        public string NacimientoFecha { get; set; }
        public string MuerteLugar { get; set; }
        public string MuerteFecha { get; set; }
        public string ActividadLugar { get; set; }
        public string ActividadFechaInicio { get; set; }
        public string ActividadFechaFin { get; set; }
        public string Observaciones { get; set; }
        
        [DataType(DataType.Url)]
        public string Url { get; set; }
        public bool Status { get; set; }

        public bool EsValido { get; set; }

        //Anteriores
        public string Temp { get; set; }



        //Virtuales
        public virtual ICollection<AutorPieza> AutorPiezas { get; set; }

    }

    public class AutorMetadata
    {
        [Display(Name = "Pseudonimo")]
        [StringLength(127)]
        public string Seudonimo { get; set; }
        
        [Required(AllowEmptyStrings = false, ErrorMessage = "Requerido.")]
        //[StringLength(127)]
        [Display(Name = "Nombre(s)")]
        public string Nombre { get; set; }

        [Display(Name = "Apellido paterno")]
        [StringLength(127)]
        public string ApellidoPaterno { get; set; }

        [Display(Name = "Apellido materno")]
        [StringLength(127)]
        public string ApellidoMaterno { get; set; }

        [Display(Name = "Lugar de nacimiento")]
        [StringLength(127)]
        public string NacimientoLugar { get; set; }

        [Display(Name = "Año de nacimiento")]
        [StringLength(127)]
        public string NacimientoFecha { get; set; }

        [Display(Name = "Lugar de muerte")]
        [StringLength(127)]
        public string MuerteLugar { get; set; }

        [Display(Name = "Año de muerte")]
        [StringLength(127)]
        public string MuerteFecha { get; set; }

        [Display(Name = "Lugar de actividad")]
        [StringLength(127)]
        public string ActividadLugar { get; set; }

        [Display(Name = "Inicio de actividad")]
        [StringLength(127)]
        public string ActividadFechaInicio { get; set; }
        
        [Display(Name = "Fin de actividad")]
        [StringLength(127)]
        public string ActividadFechaFin { get; set; }

        [Display(Name = "Estado")]
        public bool Status { get; set; }

        [Display(Name = "Valido")]
        public bool EsValido { get; set; }

        [StringLength(63)]
        public string Temp { get; set; }
    }

}