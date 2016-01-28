using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RecordFCS_Alt.Models
{
    [MetadataType(typeof(AtributoPiezaMetadata))]
    public partial class AtributoPieza
    {
        public AtributoPieza()
        {
            EsValido = false;
        }
        [Key]
        public Guid AtributoPiezaID { get; set; }

        public string Valor { get; set; }

        public string UrlTitulo { get; set; }
        public bool Status { get; set; }

        public bool EsValido { get; set; }


        //llaves foraneas
        [ForeignKey("Pieza")]
        public Guid PiezaID { get; set; }

        [ForeignKey("Atributo")]
        public Guid AtributoID { get; set; }

        [ForeignKey("ListaValor")]
        public Guid? ListaValorID { get; set; }



        //virtuales
        public virtual Pieza Pieza { get; set; }
        public virtual Atributo Atributo { get; set; }
        public virtual ListaValor ListaValor { get; set; }
    }

    public class AtributoPiezaMetadata
    {

        [Display(Name = "Valor")]
        public string Valor { get; set; }

        [Display(Name = "Estado")]
        public bool Status { get; set; }

        [Display(Name = "Título de la url")]
        [StringLength(63)]
        public string UrlTitulo { get; set; }

        [Display(Name = "Valido")]
        public bool EsValido { get; set; }


    }
}