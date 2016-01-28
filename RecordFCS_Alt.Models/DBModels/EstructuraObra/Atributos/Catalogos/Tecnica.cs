using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RecordFCS_Alt.Models
{
    [MetadataType(typeof(TecnicaMetadata))]
    public partial class Tecnica
    {
        public Tecnica()
        {
            EsValido = false;
        }

        [Key]
        public Guid TecnicaID { get; set; }

        public string ClaveSigla { get; set; }
        public string ClaveTexto { get; set; }
        public string MatriculaSigla { get; set; }
        public string Descripcion { get; set; }

        public bool EsValido { get; set; }

        public bool Status { get; set; }

        //Llaves Foraneas
        [ForeignKey("TipoTecnica")]
        public Guid TipoTecnicaID { get; set; }

        [ForeignKey("TecnicaPadre")]
        public Guid? TecnicaPadreID { get; set; }

        //Anterior
        public string Temp1 { get; set; }
        public string Temp2 { get; set; }

        //virtuales
        public virtual TipoTecnica TipoTecnica { get; set; }
        public virtual Tecnica TecnicaPadre { get; set; }
        public virtual ICollection<TecnicaPieza> TecnicaPiezas { get; set; }

    }

    public class TecnicaMetadata
    {
        public Guid TecnicaID { get; set; }

        [StringLength(31)]
        [Display(Name = "Clave")]
        public string ClaveSigla { get; set; }

        [StringLength(2047)]
        [Display(Name = "Clave Descripción")]
        public string ClaveTexto { get; set; }

        [StringLength(127)]
        [Display(Name = "Matricula")]
        public string MatriculaSigla { get; set; }


        [Display(Name = "Descripción")]
        [Required(AllowEmptyStrings = false)]
        [Remote("EsUnico", "Tecnica", HttpMethod = "POST", AdditionalFields = "TipoTecnicaID, TecnicaPadreID, TecnicaID,", ErrorMessage = "Ya existe, intenta de nuevo.")]
        public string Descripcion { get; set; }

        [Display(Name = "Valido")]
        public bool EsValido { get; set; }

        [Display(Name = "Estado")]
        public bool Status { get; set; }


        public Guid TipoTecnicaID { get; set; }

        public Guid? TecnicaPadreID { get; set; }


        [StringLength(63)]
        public string Temp1 { get; set; }
        [StringLength(63)]
        public string Temp2 { get; set; }
    }
}