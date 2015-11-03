using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RecordFCS_Alt.Models
{
    public enum EstadoMovimientoTemp
    {
        Cancelado,
        Concluido,
        Procesando
    }

    public class MovimientoTemp
    {
        //public MovimientoTemp()
        //{
        //    //Solicitante_DictamenCondicionEspacio = false;
        //    //Solicitante_DictamenSeguridad = false;
        //    //Solicitante_PeticionRecibida = false;
        //    //Solicitante_FacilityReport = false;
        //    //Solicitante_RevisionGuion = false;
        //    //Solicitante_CartaAceptacion = false;
        //    //Solicitante_ListaAvaluo = false;
        //    //Solicitante_ContratoComodato = false;
        //    //Solicitante_TramitesFianza = false;
        //    //Solicitante_PolizaSeguro = false;
        //    //Solicitante_CondicionConservacion = false;
        //    //Solicitante_AvisoSeguridad = false;
        //    //Solicitante_CartasEntregaRecepcion = false;
        //}

        //----------------------------------------------------------
        //-------------- M O V I M I E N T O -----------------------
        //----------------------------------------------------------
        [Key]
        public Guid MovimientoTempID { get; set; }

        public int Folio { get; set; }

        [Display(Name = "Tipo de movimiento")]
        [ForeignKey("TipoMovimiento")]
        public Guid TipoMovimientoID { get; set; }

        [Display(Name = "Hacia exposición")]
        public bool TieneExposicion { get; set; }

        [Display(Name = "Status")]
        public EstadoMovimientoTemp? EstadoMovimiento { get; set; }

        [Display(Name = "Fecha de registro")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? FechaRegistro { get; set; }



        [Display(Name = "Colección")]
        public string ColeccionTexto { get; set; }


        [Display(Name = "Observaciones")]
        public string Observaciones { get; set; }

        /*
         * Cboubici	-->  Origen
         * mov_condesp	-->  Condiciones especiales
         * mov_incidencias	-->  Incidencias
         */

        //--------------------------------------------------------------
        //-------------- O R I G E N  /  D E S T I N O -----------------
        //--------------------------------------------------------------

        [Display(Name = "Fecha de salida")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? FechaSalida { get; set; }

        [Display(Name = "Fecha de retorno")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? FechaRetorno { get; set; }

        [Display(Name = "Origen")]
        [ForeignKey("UbicacionOrigen")]
        public Guid? UbicacionOrigenID { get; set; }

        [Display(Name = "Destino")]
        [ForeignKey("UbicacionDestino")]
        public Guid UbicacionDestinoID { get; set; }

        // Origen / Destino	VAR_CVEDEST	cve_dest	Destino



        //--------------------------------------------
        //-------------- S E G U R O -----------------
        //--------------------------------------------
        [Display(Name = "Asegurador")]
        public string Seguro_Asegurador { get; set; }

        [Display(Name = "N° Poliza")]
        public string Seguro_NoPoliza { get; set; }

        [Display(Name = "Fecha inicial")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? Seguro_FechaInicial { get; set; }

        [Display(Name = "Fecha final")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? Seguro_FechaFinal { get; set; }

        [Display(Name = "Notas")]
        public string Seguro_Notas { get; set; }

        //------------------------------------------------------
        //-------------- S O L I C I T A N T E -----------------
        //------------------------------------------------------
        [Display(Name = "Nombre")]
        public string Solicitante_Nombre { get; set; }

        [Display(Name = "Cargo")]
        public string Solicitante_Cargo { get; set; }

        [Display(Name = "Para la institución")]
        public string Solicitante_Institucion { get; set; }

        [Display(Name = "Representante")]
        public string Solicitante_Representante { get; set; }

        [Display(Name = "Cargo")]
        public string Solicitante_RepresentanteCargo { get; set; }

        [Display(Name = "Pais / Estado")]
        public string Solicitante_PaisEstado { get; set; }
        [Display(Name = "Sede")]
        public string Solicitante_Sede { get; set; }

        [Display(Name = "Dict. cond. espacio")]
        public bool Solicitante_DictamenCondicionEspacio { get; set; } = false;

        [Display(Name = "Dict. de seguridad")]
        public bool Solicitante_DictamenSeguridad { get; set; } = false;

        [Display(Name = "Petición recibida")]
        public bool Solicitante_PeticionRecibida { get; set; } = false;

        [Display(Name = "Facility Report")]
        public bool Solicitante_FacilityReport { get; set; } = false;

        [Display(Name = "Revisión Guión")]
        public bool Solicitante_RevisionGuion { get; set; } = false;

        [Display(Name = "Carta aceptación")]
        public bool Solicitante_CartaAceptacion { get; set; } = false;

        [Display(Name = "Lista de avalúo")]
        public bool Solicitante_ListaAvaluo { get; set; } = false;

        [Display(Name = "Contrato comodato")]
        public bool Solicitante_ContratoComodato { get; set; } = false;

        [Display(Name = "Trámites de fianza")]
        public bool Solicitante_TramitesFianza { get; set; } = false;

        [Display(Name = "Póliza de seguro")]
        public bool Solicitante_PolizaSeguro { get; set; } = false;

        [Display(Name = "Condición conservación")]
        public bool Solicitante_CondicionConservacion { get; set; } = false;

        [Display(Name = "Aviso Seguridad")]
        public bool Solicitante_AvisoSeguridad { get; set; } = false;

        [Display(Name = "Cartas entr. y recep.")]
        public bool Solicitante_CartasEntregaRecepcion { get; set; } = false;



        //---------------------------------------------------------
        //------------------ T R A N S P O R T E ------------------
        //---------------------------------------------------------
        [Display(Name = "Empresa")]
        public string Transporte_Empresa { get; set; }

        [Display(Name = "Medio de transporte")]
        public string Transporte_Medio { get; set; }

        [Display(Name = "Recorrido")]
        public string Transporte_Recorrido { get; set; }

        [Display(Name = "Horarios")]
        public string Transporte_Horarios { get; set; }

        [Display(Name = "Notas")]
        public string Transporte_Notas { get; set; }


        //-------------------------------------------------------------
        //------------------ A U T O R I Z A C I O N ------------------
        //-------------------------------------------------------------
        [Display(Name = "Nombre")]
        public string Autorizacion_Nombre1 { get; set; }

        [Display(Name = "Nombre")]
        public string Autorizacion_Nombre2 { get; set; }

        [Display(Name = "Fecha")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? Autorizacion_Fecha { get; set; }

        //Autorización	VAR_DocAut	mov_aut_docto	Documento


        //-------------------------------------------------------------
        //------------------  R E S P O N S A B L E -------------------
        //-------------------------------------------------------------
        [Display(Name = "Nombre")]
        public string Responsable_Nombre { get; set; }

        [Display(Name = "Institución")]
        public string Responsable_Institucion { get; set; }

        [Display(Name = "Fecha de salida")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? Responsable_FechaSalida { get; set; }

        [Display(Name = "Fecha de retorno")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? Responsable_FechaRetorno { get; set; }



        //----------------------------------------------------------
        //------------------ E X P O S I C I O N -------------------
        //----------------------------------------------------------
        [Display(Name = "Título")]
        public string Exposicion_Titulo { get; set; }

        [Display(Name = "Curador")]
        public string Exposicion_Curador { get; set; }

        [Display(Name = "Sede")]
        public string Exposicion_Sede { get; set; }

        [Display(Name = "País")]
        public string Exposicion_Pais { get; set; }

        [Display(Name = "Fecha inicial")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? Exposicion_FechaInicial { get; set; }

        [Display(Name = "Fecha final")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? Exposicion_FechaFinal { get; set; }

        // Exposición	VAR_CART	mov_expcart	Cartela

        //-------------------------------------------------------------
        //------------------  A V A L U O -----------------------------
        //-------------------------------------------------------------
        /*
         * Avalúo	VAR_AVAMONTO	mov_ava_monto	Monto
         * Avalúo	VAR_PerVal	mov_ava_perito	Perito Valuador
         * Avalúo	VAR_FVal	mov_ava_fecha	Fecha
         * Avalúo	VAR_NotVal	mov_ava_notas	Notas
         */


        public string Temp { get; set; }

        //VIRUALES
        public virtual TipoMovimiento TipoMovimiento { get; set; }
        public virtual Ubicacion UbicacionOrigen { get; set; }
        public virtual Ubicacion UbicacionDestino { get; set; }

        public virtual ICollection<MovimientoTempPieza> MovimientoTempPiezas { get; set; }
    }
}