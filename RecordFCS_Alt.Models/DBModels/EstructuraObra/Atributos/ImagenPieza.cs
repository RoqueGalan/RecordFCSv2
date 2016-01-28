using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RecordFCS_Alt.Models
{
    [MetadataType(typeof(ImagenPiezaMetadata))]
    public partial class ImagenPieza
    {
        [Key]
        public Guid ImagenPiezaID { get; set; }

        public int Orden { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public bool EsPrincipal { get; set; }
        public string RutaParcial { get; set; }
        public string NombreImagen { get; set; }
        public bool Status { get; set; }


        //Llaves Foraneas
        [ForeignKey("Pieza")]
        public Guid PiezaID { get; set; }


        //Virtuales
        public virtual Pieza Pieza { get; set; }


        //No Mapeados

        [NotMapped]
        public string Ruta
        {
            get
            {
                if (string.IsNullOrWhiteSpace(NombreImagen) || string.IsNullOrWhiteSpace(RutaParcial))
                {
                    return "holder.js/250x200/text:404";
                }
                else
                {
                    return RutaParcial + NombreImagen;
                }
            }

            set { }
        }

        [NotMapped]
        public string RutaMini
        {
            get
            {
                if (string.IsNullOrWhiteSpace(NombreImagen) || string.IsNullOrWhiteSpace(RutaParcial))
                {
                    return "holder.js/250x200/text:404";
                }
                else
                {
                    return RutaParcial + "thumb/" + NombreImagen;
                }
            }

            set { }
        }
    }

    public class ImagenPiezaMetadata
    {
        public Guid ImagenPiezaID { get; set; }

        public int Orden { get; set; }

        [StringLength(127)]
        [Display(Name = "Título")]
        public string Titulo { get; set; }
        [StringLength(255)]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Display(Name = "Es principal")]
        public bool EsPrincipal { get; set; }

        [StringLength(255)]
        public string RutaParcial { get; set; }
        
        [StringLength(63)]
        public string NombreImagen { get; set; }

        [Display(Name = "Estado")]
        public bool Status { get; set; }

    }
}