using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RecordFCS_Alt.Models
{
    [MetadataType(typeof(TipoMovimientoMetadata))]
    public partial class TipoMovimiento
    {
        [Key]
        public Guid TipoMovimientoID { get; set; }
        public string Nombre { get; set; }
        public bool Status { get; set; }

        //virtual
        //public virtual ICollection<Movimiento> Movimientos { get; set; }

        public virtual ICollection<MovimientoTemp> MovimientosTemporales { get; set; }
    }

    public class TipoMovimientoMetadata
    {
        public Guid TipoMovimientoID { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Requerido.")]
        [StringLength(127)]
        [Display(Name = "Tipo de Movimiento")]
        [Remote("EsUnico", "TipoMovimiento", HttpMethod = "POST", AdditionalFields = "TipoMovimientoID", ErrorMessage = "Ya existe, intenta otro nombre.")]
        public string Nombre { get; set; }

        [Display(Name = "Estado")]
        public bool Status { get; set; }

    }
}