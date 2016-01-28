using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RecordFCS_Alt.Models
{

    [MetadataType(typeof(TipoObraMetadata))]
    public partial class TipoObra
    {
        public TipoObra()
        {
            EsValido = false;
        }

        [Key]
        public Guid TipoObraID { get; set; }

        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool Status { get; set; }

        public bool EsValido { get; set; }



        //Anteriores
        public string Temp { get; set; }


        //Virtuales
        public virtual ICollection<TipoPieza> TipoPiezas { get; set; }
        public virtual ICollection<Obra> Obras { get; set; }


    }


    public class TipoObraMetadata
    {
        public Guid TipoObraID { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Requerido.")]
        [StringLength(63)]
        [Display(Name = "Tipo de Obra")]
        [Remote("EsUnico", "TipoObra", HttpMethod = "POST", AdditionalFields = "TipoObraID", ErrorMessage = "Ya existe, intenta otro nombre.")]
        public string Nombre { get; set; }

        [StringLength(255)]
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