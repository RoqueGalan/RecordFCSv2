using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordFCS_Alt.Models
{
    [MetadataType(typeof(HistorialLogMetadata))]
    public partial class HistorialLog
    {
        [Key]
        public Guid HistorialLogID { get; set; }

        public DateTime EventoFecha { get; set; }
        public string EventoTipo { get; set; }


        public string CategoriaTipo { get; set; }
        public string TablaNombre { get; set; }
        public string LlavePrimaria { get; set; }
        
        public string Motivo { get; set; }

        //Llaves Foraneas
        [ForeignKey("Usuario")]
        public Guid? UsuarioID { get; set; }


        //Virtuales
        public virtual Usuario Usuario { get; set; }
        public virtual ICollection<HistorialLogDetalle> HistorialLogDetalles { get; set; }


        //extras
        public virtual string Color()
        {
            string color = "";

            switch (this.EventoTipo)
            {
                case "Crear":
                    color = "success";
                    break;
                case "Editar":
                    color = "info";
                    break;
                case "Eliminar":
                    color = "danger";
                    break;
            }

            return color;
        }

        public virtual string Icono()
        {
            string icono = "";

            switch (this.EventoTipo)
            {
                case "Crear":
                    icono = "fa-plus";
                    break;
                case "Editar":
                    icono = "fa-pencil";
                    break;
                case "Eliminar":
                    icono = "fa-trash";
                    break;
            }

            return icono;
        }

    }

    public class HistorialLogMetadata
    {

        [Display(Name = "Fecha de modificación")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime EventoFecha { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Requerido.")]
        public string Motivo { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Requerido.")]
        public string LlavePrimaria { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Requerido.")]
        public string TablaNombre { get; set; }

    }

}
