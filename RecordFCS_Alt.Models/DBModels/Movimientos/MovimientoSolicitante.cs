using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RecordFCS_Alt.Models
{
    public class MovimientoSolicitante
    {
        [Key]
        public int MovimientoSolicitanteID { get; set; }


        public string NombreSolicitante { get; set; }
        public string CargoSolicitante { get; set; }
        public string ParaInstitucion { get; set; }
        public string NombreRepresentante { get; set; }
        public string CargoRepresentante { get; set; }
        public string Sede { get; set; }
        public string PaisEstado { get; set; }
        public bool PeticionRecibida { get; set; }
        public bool DictCondEspacio { get; set; }
        public bool CartaAceptacion { get; set; }
        public bool ContratoComodato { get; set; }
        public bool RevisionGuion { get; set; }
        public bool CondicionConservacion { get; set; }
        public bool FacilityReport { get; set; }
        public bool DicSeguridad { get; set; }

        public bool ListaAvaluo { get; set; }
        public bool TramiteFianza { get; set; }
        public bool PolizaSeguro { get; set; }
        public bool CartasEntradaSalida { get; set; }

        //virtual
    }
}