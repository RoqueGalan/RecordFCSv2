using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RecordFCS_Alt.Models
{
    public class Permiso
    {
        [Key]
        [Column(Order = 1)]
        [ForeignKey("Usuario")]
        public Guid UsuarioID { get; set; }

        [Key]
        [Column(Order = 2)]
        [ForeignKey("TipoPermiso")]
        public Guid TipoPermisoID { get; set; }

        [Display(Name = "¿Activo?")]
        public bool Status { get; set; }

        /* Propiedades de navegacion*/
        public virtual Usuario Usuario { get; set; }
        public virtual TipoPermiso TipoPermiso { get; set; }
    }
}