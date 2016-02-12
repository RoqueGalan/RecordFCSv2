using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordFCS_Alt.Models
{
    public partial class HistorialLogDetalle
    {

        private string _ValorOriginal;
        private string _ValorNuevo;




        [Key]
        public Guid HistorialLogDetalleID { get; set; }


        public string ColumnaNombre { get; set; }
        public string ValorOriginal
        {
            get
            {
                return string.IsNullOrWhiteSpace(_ValorOriginal) ? _ValorOriginal : _ExtraerValorDeTabla(_ValorOriginal);
            }
            set
            {
                _ValorOriginal = value;
            }
        }

        public string ValorNuevo
        {
            get
            {
                return string.IsNullOrWhiteSpace(_ValorNuevo) ? _ValorNuevo : _ExtraerValorDeTabla(_ValorNuevo);

            }
            set
            {
                _ValorNuevo = value;
            }
        }



        [ForeignKey("HistorialLog")]
        public Guid HistorialLogID { get; set; }


        //virtual
        public HistorialLog HistorialLog { get; set; }

        private string _ExtraerValorDeTabla(string campoValor)
        {


            string valor = "";

            Guid llave1 = Guid.Empty;
            Guid llave2 = Guid.Empty;
            int llaveInt = 0;

            campoValor = campoValor ?? "";

            try
            {
                var db = new RecordFCSContext();

                try
                {
                    llave1 = new Guid(campoValor);

                }
                catch (Exception)
                {
                    llave1 = Guid.Empty;
                }

                var objetoDB = new { llave1 = Guid.Empty, llave2 = Guid.Empty, llaveInt = 0, Texto = campoValor };
                bool tratarObjeto = true;

                switch (ColumnaNombre)
                {
                    case "AutorID":
                        {
                            objetoDB = db.Autores.Select(a => new
                            {
                                llave1 = a.AutorID,
                                llave2 = Guid.Empty,
                                llaveInt = 0,
                                Texto = a.Seudonimo + " " + a.Nombre + " " + a.ApellidoPaterno + " " + a.ApellidoMaterno
                            })
                            .FirstOrDefault(a => a.llave1 == llave1);
                        }
                        break;

                    case "LetraFolioID":
                        {
                            try
                            {
                                llaveInt = Convert.ToInt32(campoValor);
                            }
                            catch (Exception)
                            {
                                llaveInt = 0;
                            }


                            objetoDB = db.LetraFolios.Select(a => new
                            {
                                llave1 = Guid.Empty,
                                llave2 = Guid.Empty,
                                llaveInt = a.LetraFolioID,
                                Texto = a.Nombre
                            })
                            .FirstOrDefault(a => a.llaveInt == llaveInt);
                        }
                        break;

                    case "ListaValorID":
                        {
                            objetoDB = db.ListaValores.Select(a => new
                            {
                                llave1 = a.ListaValorID,
                                llave2 = Guid.Empty,
                                llaveInt = 0,
                                Texto = a.Valor
                            })
                            .FirstOrDefault(a => a.llave1 == llave1);
                        }
                        break;

                    case "UbicacionID":
                    case "UbicacionOrigenID":
                    case "UbicacionDestinoID":
                        {
                            objetoDB = db.Ubicaciones.Select(a => new
                            {
                                llave1 = a.UbicacionID,
                                llave2 = Guid.Empty,
                                llaveInt = 0,
                                Texto = a.Nombre
                            })
                            .FirstOrDefault(a => a.llave1 == llave1);
                        }
                        break;

                    case "MovimientoTempID":
                        {
                            objetoDB = db.MovimientosTemp.Select(a => new
                            {
                                llave1 = a.MovimientoTempID,
                                llave2 = Guid.Empty,
                                llaveInt = 0,
                                Texto = a.Folio.ToString()
                            })
                            .FirstOrDefault(a => a.llave1 == llave1);
                        }
                        break;

                    case "TecnicaID":
                    case "TecnicaPadreID":
                        {
                            objetoDB = db.Tecnicas.Select(a => new
                            {
                                llave1 = a.TecnicaID,
                                llave2 = Guid.Empty,
                                llaveInt = 0,
                                Texto = a.Descripcion
                            })
                            .FirstOrDefault(a => a.llave1 == llave1);
                        }
                        break;

                    case "TipoTecnicaID":
                        {
                            objetoDB = db.TipoTecnicas.Select(a => new
                            {
                                llave1 = a.TipoTecnicaID,
                                llave2 = Guid.Empty,
                                llaveInt = 0,
                                Texto = a.Nombre
                            })
                            .FirstOrDefault(a => a.llave1 == llave1);
                        }
                        break;

                    case "PiezaID":
                    case "PiezaPadreID":
                        {
                            objetoDB = db.Piezas.Select(a => new
                            {
                                llave1 = a.PiezaID,
                                llave2 = Guid.Empty,
                                llaveInt = 0,
                                Texto = a.Obra.LetraFolio.Nombre + a.Obra.NumeroFolio + a.SubFolio
                            })
                            .FirstOrDefault(a => a.llave1 == llave1);
                        }
                        break;

                    case "TipoArchivoID":
                        {
                            objetoDB = db.TipoArchivos.Select(a => new
                            {
                                llave1 = a.TipoArchivoID,
                                llave2 = Guid.Empty,
                                llaveInt = 0,
                                Texto = a.Nombre
                            })
                            .FirstOrDefault(a => a.llave1 == llave1);
                        }
                        break;

                    case "TipoAtributoID":
                        {
                            objetoDB = db.TipoAtributos.Select(a => new
                            {
                                llave1 = a.TipoAtributoID,
                                llave2 = Guid.Empty,
                                llaveInt = 0,
                                Texto = a.Nombre
                            })
                            .FirstOrDefault(a => a.llave1 == llave1);
                        }
                        break;

                    case "TipoMedidaID":
                        {
                            objetoDB = db.TipoMedidas.Select(a => new
                            {
                                llave1 = a.TipoMedidaID,
                                llave2 = Guid.Empty,
                                llaveInt = 0,
                                Texto = a.Nombre
                            })
                            .FirstOrDefault(a => a.llave1 == llave1);
                        }
                        break;

                    case "TipoMostrarArchivoID":
                        {
                            objetoDB = db.TipoMostrarArchivos.Select(a => new
                            {
                                llave1 = a.TipoMostrarArchivoID,
                                llave2 = Guid.Empty,
                                llaveInt = 0,
                                Texto = a.Nombre
                            })
                            .FirstOrDefault(a => a.llave1 == llave1);
                        }
                        break;

                    case "TipoMostrarID":
                        {
                            objetoDB = db.TipoMostarlos.Select(a => new
                            {
                                llave1 = a.TipoMostrarID,
                                llave2 = Guid.Empty,
                                llaveInt = 0,
                                Texto = a.Nombre
                            })
                            .FirstOrDefault(a => a.llave1 == llave1);
                        }
                        break;

                    case "TipoMovimientoID":
                        {
                            objetoDB = db.TipoMovimientos.Select(a => new
                            {
                                llave1 = a.TipoMovimientoID,
                                llave2 = Guid.Empty,
                                llaveInt = 0,
                                Texto = a.Nombre
                            })
                            .FirstOrDefault(a => a.llave1 == llave1);
                        }
                        break;

                    case "ObraID":
                        {
                            objetoDB = db.Obras.Select(a => new
                            {
                                llave1 = a.ObraID,
                                llave2 = Guid.Empty,
                                llaveInt = 0,
                                Texto = a.LetraFolio.Nombre + a.NumeroFolio
                            })
                            .FirstOrDefault(a => a.llave1 == llave1);
                        }
                        break;

                    case "TipoObraID":
                        {
                            objetoDB = db.TipoObras.Select(a => new
                            {
                                llave1 = a.TipoObraID,
                                llave2 = Guid.Empty,
                                llaveInt = 0,
                                Texto = a.Nombre
                            })
                            .FirstOrDefault(a => a.llave1 == llave1);
                        }
                        break;

                    case "TipoPiezaID":
                    case "TipoPiezaPadreID":
                        {
                            objetoDB = db.TipoPiezas.Select(a => new
                            {
                                llave1 = a.TipoPiezaID,
                                llave2 = Guid.Empty,
                                llaveInt = 0,
                                Texto = a.Nombre
                            })
                            .FirstOrDefault(a => a.llave1 == llave1);

                        }
                        break;

                    case "UsuarioID":
                        {
                            objetoDB = db.Usuarios.Select(a => new
                            {
                                llave1 = a.UsuarioID,
                                llave2 = Guid.Empty,
                                llaveInt = 0,
                                Texto = a.Nombre + " " + a.Apellido
                            })
                            .FirstOrDefault(a => a.llave1 == llave1);
                        }
                        break;

                    //Bool ACTIVO -- INACTIVO
                    case "Status":
                        {
                            valor = campoValor == "True" ? "Activo" : campoValor == "False" ? "Inactivo" : campoValor;
                            tratarObjeto = false;
                        }
                        break;

                    //Bool SI --- NO
                    case "EsPrincipal":
                    case "esPrincipal":
                    case "EsValido":
                    case "TieneExposicion":
                    case "Solicitante_DictamenCondicionEspacio":
                    case "Solicitante_DictamenSeguridad":
                    case "Solicitante_PeticionRecibida":
                    case "Solicitante_FacilityReport":
                    case "Solicitante_RevisionGuion":
                    case "Solicitante_CartaAceptacion":
                    case "Solicitante_ListaAvaluo":
                    case "Solicitante_ContratoComodato":
                    case "Solicitante_TramitesFianza":
                    case "Solicitante_PolizaSeguro":
                    case "Solicitante_CondicionConservacion":
                    case "Solicitante_AvisoSeguridad":
                    case "Solicitante_CartasEntregaRecepcion":
                        {
                            valor = campoValor == "True" ? "Si" : campoValor == "False" ? "No" : campoValor;
                            tratarObjeto = false;
                        }
                        break;

                    case "AtributoID":
                        {
                            objetoDB = db.Atributos.Select(a => new
                            {
                                llave1 = a.AtributoID,
                                llave2 = Guid.Empty,
                                llaveInt = 0,
                                Texto = a.NombreAlterno
                            })
                                                        .FirstOrDefault(a => a.llave1 == llave1);
                        }
                        break;

                    case "ArchivoPiezaID":
                    case "AtributoPiezaID":
                    case "ImagenPiezaID":
                    default:
                        {
                            valor = campoValor;
                            tratarObjeto = false;
                        }
                        break;

                }

                if (tratarObjeto)
                    valor = objetoDB.Texto;
            }
            catch (Exception)
            {
                valor = campoValor;
            }

            return valor;
        }


    }
}
