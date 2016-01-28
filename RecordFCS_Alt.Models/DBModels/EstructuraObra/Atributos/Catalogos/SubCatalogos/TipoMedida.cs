using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RecordFCS_Alt.Models
{
    [MetadataType(typeof(TipoMedidaMetadata))]
    public partial class TipoMedida
    {
        public TipoMedida()
        {
            EsValido = false;
        }

        [Key]
        public Guid TipoMedidaID { get; set; }

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public bool Status { get; set; }

        public bool EsValido { get; set; }


        //Anteriores
        public string Temp { get; set; }

        //Virtuales
        public virtual ICollection<MedidaPieza> Medidas { get; set; }
    }

    public class TipoMedidaMetadata
    {
        public Guid TipoMedidaID { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Requerido.")]
        [StringLength(255)]
        [Display(Name = "Tipo de Médida")]
        [Remote("EsUnico", "TipoMedida", HttpMethod = "POST", AdditionalFields = "TipoMedidaID", ErrorMessage = "Ya existe, intenta otro nombre.")]
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