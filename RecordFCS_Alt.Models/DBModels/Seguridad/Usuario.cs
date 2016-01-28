using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RecordFCS_Alt.Models
{
    public class Usuario
    {
        [Key]
        public Guid UsuarioID { get; set; }

        [Required]
        //[Remote("EsUsuarioDisponible", "Validacion")]
        [Display(Name = "Nombre de Usuario")]
        [Remote("validarRegistroUnico", "Usuario", HttpMethod = "POST", AdditionalFields = "UsuarioID", ErrorMessage = "Ya existe un registro con este nombre. Intenta con otro.")]
        public string UserName { get; set; }

        [Required]
        //[RegularExpression(@"^(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*_])[0-9a-zA-Z!@#$%^&*_0-9]{8,128}$", ErrorMessage = "Contraseña debe contener, Mayuscula, Número, Caracter Especial !@#$%^&*_ y 8 Caracteres Mínimo.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [NotMapped]
        [Remote("validarCompararPassword", "Usuario", HttpMethod = "POST", AdditionalFields = "Password", ErrorMessage = "La contraseña no coincide.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar")]
        public string ConfirmPassword { get; set; }

        public string Nombre { get; set; }
        public string Apellido { get; set; }

        [Display(Name = "Correo Eléctronico")]
        [EmailAddress]
        [StringLength(128)]
        [DataType(DataType.EmailAddress)]
        public string Correo { get; set; }

        [Display(Name = "¿Activo?")]
        public bool Status { get; set; }


        //Anteriores
        [StringLength(63)]
        public string Temp { get; set; }


        /* Propiedades de navegacion*/
        public virtual ICollection<Permiso> Permisos { get; set; }
        public virtual ICollection<MovimientoTemp> MovimientosCreados { get; set; }

        public virtual ICollection<Pieza> PiezasCreadas { get; set; }


        public virtual ICollection<HistorialLog> HistorialAcciones { get; set; }

        //public virtual ICollection<MovimientoAutorizacion> Autorizaciones { get; set; }

    }
}