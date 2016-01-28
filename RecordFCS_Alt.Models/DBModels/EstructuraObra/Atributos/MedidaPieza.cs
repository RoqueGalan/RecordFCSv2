using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RecordFCS_Alt.Models
{
    public enum UMLongitud
    {
        mm,
        cm,
        pulgada,
        dc,
        m,
        dam,
        hm,
        km
    }

    public enum UMMasa
    {
        mg,
        cg,
        dg,
        g,
        dag,
        hg,
        kg,
        tonelada
    }

    [MetadataType(typeof(MedidaPiezaMetadata))]
    public partial class MedidaPieza
    {
        public MedidaPieza()
        {
            EsValido = false;
        }

        [Key]
        [Column(Order = 1)]
        [ForeignKey("Pieza")]
        public Guid PiezaID { get; set; }

        [Key]
        [Column(Order = 2)]
        [ForeignKey("TipoMedida")]
        public Guid TipoMedidaID { get; set; }


        public double? Altura { get; set; }
        public double? Anchura { get; set; }

        public double? Profundidad { get; set; }

        public double? Diametro { get; set; }
        public double? Diametro2 { get; set; }

        public UMLongitud? UMLongitud { get; set; }

        public double? Peso { get; set; }
        public UMMasa UMMasa { get; set; }

        public string Otra { get; set; }

        public bool Status { get; set; }


        public bool EsValido { get; set; }


        //Antetior
        public string Temp { get; set; }

        //virtual
        public virtual Pieza Pieza { get; set; }
        public virtual TipoMedida TipoMedida { get; set; }

    }

    public class MedidaPiezaMetadata
    {
        public Guid PiezaID { get; set; }

        [Display(Name = "Tipo de Medida")]
        public Guid TipoMedidaID { get; set; }

        [Display(Name = "Altura")]
        public double? Altura { get; set; }

        [Display(Name = "Anchura")]
        public double? Anchura { get; set; }
        
        [Display(Name = "Profundidad")]
        public double? Profundidad { get; set; }

        [Display(Name = "Diámetro 1")]
        public double? Diametro { get; set; }
        [Display(Name = "Diámetro 2")]
        public double? Diametro2 { get; set; }

        [Display(Name = "Unidad de Medida de Longitud")]
        public UMLongitud? UMLongitud { get; set; }


        [Display(Name = "Unidad de Medida de Peso")]
        public UMMasa UMMasa { get; set; }

        [Display(Name = "Otra Médida")]
        [StringLength(127)]
        public string Otra { get; set; }

        [Display(Name = "Estado")]
        public bool Status { get; set; }



        [Display(Name = "Valido")]
        public bool EsValido { get; set; }



        [StringLength(63)]
        public string Temp { get; set; }
    }
}