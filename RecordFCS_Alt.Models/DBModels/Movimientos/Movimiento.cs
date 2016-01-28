using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RecordFCS_Alt.Models
{
    public enum EstadoMovimiento
    {
        Error,
        Cancelado,
        EnRegistro,
        EnAutorizacion1,
        EnAutorizacion2,
        EnProceso,
        Terminado
    }

    public class Movimiento
    {
        [Key]
        public Guid MovientoID { get; set; }

        public int FolioMovimiento { get; set; }

        public bool HaciaExposicion { get; set; }

        [ForeignKey("TipoMovimiento")]
        public Guid TipoMovimientoID { get; set; }

        public EstadoMovimiento? EstadoMovimiento { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? FechaRegistro { get; set; }


        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? FechaRet { get; set; }



        //Origen / Destino
        [ForeignKey("UbicacionOrigen")]
        public Guid? UbicacionOrigenID { get; set; }
        [ForeignKey("UbicacionDestino")]
        public Guid UbicacionDestinoID { get; set; }

        [Required(ErrorMessage = "Ingresa la fecha y hora del movimiento")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? FechaHoraMovimiento { get; set; }

        public string ColeccionTexto { get; set; }


        [DataType(DataType.MultilineText)]
        public string Observaciones { get; set; }



        //Foraneos
        [ForeignKey("MovimientoSolicitante")]
        public int? MovimientoSolicitanteID { get; set; }
        [ForeignKey("MovimientoResponsable")]
        public int? MovimientoResponsableID { get; set; }
        [ForeignKey("MovimientoExposicion")]
        public int? MovimientoExposicionID { get; set; }
        [ForeignKey("MovimientoAutorizacion")]
        public int? MovimientoAutorizacionID { get; set; }
        [ForeignKey("MovimientoTransporte")]
        public int? MovimientoTransporteID { get; set; }
        [ForeignKey("MovimientoSeguro")]
        public int? MovimientoSeguroID { get; set; }




        //Virtual
        public virtual TipoMovimiento TipoMovimiento { get; set; }
        public virtual Ubicacion UbicacionOrigen { get; set; }
        public virtual Ubicacion UbicacionDestino { get; set; }

        public virtual MovimientoSolicitante MovimientoSolicitante { get; set; }
        public virtual MovimientoResponsable MovimientoResponsable { get; set; }
        public virtual MovimientoExposicion MovimientoExposicion { get; set; }
        public virtual MovimientoAutorizacion MovimientoAutorizacion { get; set; }
        public virtual MovimientoTransporte MovimientoTransporte { get; set; }
        public virtual MovimientoSeguro MovimientoSeguro { get; set; }



        public virtual ICollection<MovimientoPieza> MovimientoPiezas { get; set; }


    }
}