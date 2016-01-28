using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RecordFCS_Alt.Models
{
    [MetadataType(typeof(PiezaMetadata))]
    public partial class Pieza
    {
        [Key]
        public Guid PiezaID { get; set; }

        public string SubFolio { get; set; }

        public DateTime FechaRegistro { get; set; }
        public bool Status { get; set; }


        //Llaves Foraneas
        [ForeignKey("Obra")]
        public Guid ObraID { get; set; }

        [ForeignKey("TipoPieza")]
        public Guid TipoPiezaID { get; set; }

        [ForeignKey("Ubicacion")]
        public Guid? UbicacionID { get; set; }


        [ForeignKey("PiezaPadre")]
        public Guid? PiezaPadreID { get; set; }

        [ForeignKey("UsuarioCreador")]
        public Guid? UsuarioID { get; set; }



        //Anteriores
        public string Temp { get; set; }

        //Virtuales
        public virtual Usuario UsuarioCreador { get; set; }
        public virtual Obra Obra { get; set; }
        public virtual TipoPieza TipoPieza { get; set; }
        public virtual Pieza PiezaPadre { get; set; }
        public virtual ICollection<Pieza> PiezasHijas { get; set; }

        // atributos 
        public virtual Ubicacion Ubicacion { get; set; }

        public virtual ICollection<AutorPieza> AutorPiezas { get; set; }
        public virtual ICollection<ImagenPieza> ImagenPiezas { get; set; }
        public virtual ICollection<TecnicaPieza> TecnicaPiezas { get; set; }
        public virtual ICollection<MedidaPieza> MedidaPiezas { get; set; }
        public virtual ICollection<AtributoPieza> AtributoPiezas { get; set; }
        public virtual ICollection<ArchivoPieza> ArchivosPiezas { get; set; }

        //atributoPieza

        //  videos
        //  audio


        public virtual ICollection<MovimientoPieza> MovimientoPiezas { get; set; }
        public virtual ICollection<MovimientoTempPieza> MovimientoTempPiezas { get; set; }


        public string ImprimirFolio()
        {
            string Fol = "";
            List<string> lista = new List<string>();
            // TIPOLETRA    /   OBRA.NOFOLIO    /   TIPOPIEZA.SUBFOLIO  /   TIPOPIEZA.SUBFOLIO  /   TIPOPIEZA.SUBFOLIO
            //      A               587                 A                      H2                       K3                  
            // A587AH2K3

            var temp = this;
            do
            {
                lista.Add(temp.SubFolio);
                temp = temp.PiezaPadre;
            }
            while (temp != null);

            lista.Add(Obra.LetraFolio.Nombre + Obra.NumeroFolio);
            lista.Reverse();

            foreach (var item in lista)
            {
                Fol += item;
            }

            return Fol;
        }

    }


    public class PiezaMetadata
    {
        public Guid PiezaID { get; set; }

        [StringLength(31)]
        [Display(Name = "No. Interno")]
        public string SubFolio { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Fecha de Registro")]
        [Required(ErrorMessage = "Requerido.")]
        public DateTime FechaRegistro { get; set; }

        [Display(Name = "Estado")]
        public bool Status { get; set; }

        [StringLength(63)]
        public string Temp { get; set; }

        [Display(Name = "Usuario creador")]
        public Guid? UsuarioID { get; set; }

    }
}