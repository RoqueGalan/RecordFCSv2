using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RecordFCS_Alt.Models
{
    public class TipoPermiso
    {

        [Key]
        public Guid TipoPermisoID { get; set; }

        [Remote("validarRegistroUnicoClave", "TipoPermiso", HttpMethod = "POST", ErrorMessage = "Ya existe un registro con esta clave. Intenta con otro.")]
        public string Clave { get; set; }

        public string Nombre { get; set; }
        public string Descripcion { get; set; }

        [Display(Name = "¿Activo?")]
        public bool Status { get; set; }


        /* Propiedades de navegacion*/
        public virtual ICollection<Permiso> Permisos { get; set; }
    }
}