using RecordFCS_Alt.Models;
using RecordFCS_Alt.Models.ViewModels;
using RecordFCS_Alt.Models.ViewsModel;
using Rotativa;
using Rotativa.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace RecordFCS_Alt.Controllers
{
    public class ImprimirController : Controller
    {
        private RecordFCSContext db = new RecordFCSContext();

        // GET: Imprimir
        public ActionResult Menu(string id)
        {
            var imprimir = new itemImprimir()
            {
                TipoImprimir = "Busqueda"
            };


            ViewBag.TipoVista = id;

            return PartialView("_Menu", imprimir);
        }

        private void generarPiezaImprimir(Pieza pieza, itemImprimir imprimir)
        {
            string nombreDatos = "";
            string nombreArchivos = "";

            if (imprimir.DatosCompletos)
            {
                switch (imprimir.TipoImprimir)
                {
                    case "Conservacion":
                        nombreDatos = "Completos";
                        nombreArchivos = "Conservacion";
                        break;

                    case "Etiqueta":
                        nombreDatos = "Etiquetas";
                        nombreArchivos = "Basicos";
                        break;

                    default:
                        nombreDatos = "Completos";
                        nombreArchivos = "Completos";
                        break;
                }
            }
            else
            {
                switch (imprimir.TipoImprimir)
                {

                    case "Etiqueta":
                        nombreDatos = "Etiquetas";
                        nombreArchivos = "Basicos";
                        break;

                    case "Conservacion":
                        nombreDatos = "Conservacion";
                        nombreArchivos = "Conservacion";
                        break;

                    default:
                        nombreDatos = "Basicos";
                        nombreArchivos = "Basicos";
                        break;
                }
            }


            itemPiezaMini piezaMini = new itemPiezaMini()
            {
                ObraID = pieza.ObraID,
                PiezaID = pieza.PiezaID,
                FolioObra = pieza.Obra.LetraFolio.Nombre + pieza.Obra.NumeroFolio,
                FolioPieza = pieza.ImprimirFolio(),
                NombreObra = pieza.Obra.TipoObra.Nombre,
                NombrePieza = pieza.TipoPieza.Nombre,
                esPrincipal = pieza.TipoPieza.EsPrincipal,
                //esBusqueda = esBusqueda,
                //ListaPiezasHijas = new List<Guid>(),
                Atributos = new List<itemPiezaMiniAtt>(),
                UbicacionOrigenID = pieza.UbicacionID
            };


            //Encontrar Imagen de la pieza
            var tipoMostrarArchivos = db.TipoMostrarArchivos.FirstOrDefault(a => a.Nombre == nombreArchivos);
            TipoArchivo tipoArchivo = db.TipoArchivos.FirstOrDefault(a => a.Temp == "imagen_clave");

            var imagen = pieza.ArchivosPiezas.Where(a => a.Status && a.TipoArchivoID == tipoArchivo.TipoArchivoID && a.MostrarArchivos.Any(b => b.TipoMostrarArchivo.TipoMostrarArchivoID == tipoMostrarArchivos.TipoMostrarArchivoID && b.Status)).OrderBy(a => a.Orden).FirstOrDefault();
            if (imagen != null)
            {
                //Comprobar que el archivo exista
                FileInfo infoThumb = new FileInfo(Server.MapPath("~" + imagen.Ruta));

                if (infoThumb.Exists)
                {
                    piezaMini.ImagenID = imagen.ArchivoPiezaID;
                    piezaMini.RutaImagenMini = imagen.RutaThumb;
                }
            }

            //extraer una lista de los atributos que presenta la pieza
            var tipoMostrarDatos = db.TipoMostarlos.FirstOrDefault(a => a.Nombre == nombreDatos);
            List<Atributo> listaAtt = pieza.TipoPieza.Atributos.Where(a => a.Status && a.MostrarAtributos.Any(b => b.TipoMostrarID == tipoMostrarDatos.TipoMostrarID && b.Status) && a.TipoAtributo.Status).OrderBy(a => a.Orden).ToList();


            //llenar los atributos de la pieza
            foreach (var att in listaAtt)
            {
                var tipoAtt = att.TipoAtributo;
                var infoAtt = att.MostrarAtributos.FirstOrDefault(a => a.TipoMostrarID == tipoMostrarDatos.TipoMostrarID);

                infoAtt.InicioHTML = infoAtt.InicioHTML ?? "";
                infoAtt.FinHTML = infoAtt.FinHTML ?? "";


                var attFicha = new itemPiezaMiniAtt()
                {
                    Nombre = att.NombreAlterno,
                    Orden = att.Orden,
                    PiezaID = piezaMini.PiezaID,
                    AtributoID = att.AtributoID,
                    Valores = new List<itemPiezaMiniAttValor>()
                };

                if (tipoAtt.EsGenerico)
                {
                    var lista_AttPieza = pieza.AtributoPiezas.Where(a => a.Atributo == att).ToList();

                    if (lista_AttPieza.Count > 0)
                    {
                        foreach (var item in lista_AttPieza)
                        {
                            var attValor = new itemPiezaMiniAttValor()
                            {
                                AtributoPiezaID = item.AtributoPiezaID,
                                Orden = attFicha.Valores.Count + 1
                            };

                            if (tipoAtt.EsLista)
                            {
                                attValor.Valor = item.ListaValor.Valor;
                            }
                            else
                            {
                                attValor.Valor = item.Valor;
                            }

                            if (!string.IsNullOrWhiteSpace(attValor.Valor))
                            {
                                attFicha.Valores.Add(attValor);
                            }
                        }
                    }
                }
                else
                {
                    switch (tipoAtt.TablaSQL)
                    {
                        case "Autor":
                            var lista_AttAutor = pieza.AutorPiezas.Where(a => a.Status).OrderByDescending(a => a.esPrincipal).ThenBy(a => a.Prefijo).ThenBy(a => a.Autor.Nombre).ToList();
                            if (lista_AttAutor.Count > 0)
                            {
                                foreach (var item in lista_AttAutor)
                                {
                                    var attValor = new itemPiezaMiniAttValor()
                                    {
                                        AtributoPiezaID = item.AutorID,
                                        Orden = attFicha.Valores.Count + 1
                                    };

                                    var textoNombre = Regex.Replace(string.Format(item.Autor.Seudonimo + " " + item.Autor.Nombre + " " + item.Autor.ApellidoPaterno + " " + item.Autor.ApellidoMaterno).ToString().Trim(), @"\s+", " ");

                                    attValor.Valor = string.IsNullOrWhiteSpace(item.Prefijo) ? "" : item.Prefijo + ": " + textoNombre;

                                    if (!string.IsNullOrWhiteSpace(textoNombre))
                                    {
                                        attFicha.Valores.Add(attValor);
                                    }
                                }
                            }
                            break;

                        case "Ubicacion":
                            if (pieza.UbicacionID != null)
                            {
                                var attValor = new itemPiezaMiniAttValor()
                                {
                                    AtributoPiezaID = pieza.UbicacionID,
                                    Orden = 1,
                                    Valor = pieza.Ubicacion.Nombre
                                };

                                if (!string.IsNullOrWhiteSpace(attValor.Valor))
                                {
                                    attFicha.Valores.Add(attValor);
                                }

                            }
                            break;

                        case "TipoTecnica":
                            var lista_Tecnicas = pieza.TecnicaPiezas.Where(a => a.Status).OrderBy(a => a.TipoTecnica.Nombre).ToList();

                            if (lista_Tecnicas.Count > 0)
                            {
                                foreach (var item in lista_Tecnicas)
                                {
                                    var attValor = new itemPiezaMiniAttValor()
                                    {
                                        AtributoPiezaID = item.TecnicaID,
                                        Orden = attFicha.Valores.Count + 1
                                    };

                                    attValor.Valor = item.TipoTecnica.Nombre + ": " + item.Tecnica.Descripcion;

                                    if (!string.IsNullOrWhiteSpace(item.Tecnica.Descripcion))
                                    {
                                        attFicha.Valores.Add(attValor);
                                    }
                                }
                            }
                            break;

                        case "TipoMedida":
                            var lista_Medidas = pieza.MedidaPiezas.Where(a => a.Status).OrderBy(a => a.TipoMedida.Nombre).ToList();
                            if (lista_Medidas.Count > 0)
                            {
                                foreach (var item in lista_Medidas)
                                {
                                    var attValor = new itemPiezaMiniAttValor()
                                    {
                                        AtributoPiezaID = item.TipoMedidaID,
                                        Orden = attFicha.Valores.Count + 1
                                    };

                                    string medidaTexto = "";
                                    bool existe0 = false;
                                    bool existe1 = false;

                                    //1x
                                    existe0 = item.Altura.HasValue ? true : false;
                                    existe1 = item.Anchura.HasValue ? true : false;

                                    medidaTexto += existe0 ? item.Altura.ToString() : "";
                                    medidaTexto += existe0 && existe1 ? "x" : "";
                                    existe0 = existe1;
                                    existe1 = item.Profundidad.HasValue ? true : false;

                                    //2x
                                    medidaTexto += medidaTexto.EndsWith("x") ? "" : medidaTexto.Length > 0 && existe0 ? "x" : "";
                                    medidaTexto += existe0 ? item.Anchura.ToString() : "";
                                    medidaTexto += existe0 && existe1 ? "x" : "";
                                    existe0 = existe1;
                                    existe1 = item.Diametro.HasValue ? true : false;

                                    //3x
                                    medidaTexto += medidaTexto.EndsWith("x") ? "" : medidaTexto.Length > 0 && existe0 ? "x" : "";
                                    medidaTexto += existe0 ? item.Profundidad.ToString() : "";
                                    medidaTexto += existe0 && existe1 ? "x" : "";
                                    existe0 = existe1;
                                    existe1 = item.Diametro2.HasValue ? true : false;

                                    //4Øx
                                    medidaTexto += medidaTexto.EndsWith("x") ? "" : medidaTexto.Length > 0 && existe0 ? "x" : "";
                                    medidaTexto += existe0 ? item.Diametro.ToString() + "Ø" : "";
                                    medidaTexto += existe0 && existe1 ? "x" : "";
                                    existe0 = existe1;
                                    existe1 = item.UMLongitud.HasValue ? true : false;

                                    //cm
                                    medidaTexto += medidaTexto.EndsWith("x") ? "" : medidaTexto.Length > 0 && existe0 ? "x" : "";
                                    medidaTexto += existe0 ? item.Diametro2.ToString() + "Ø" : "";
                                    medidaTexto += existe1 && medidaTexto.Length > 0 ? item.UMLongitud.ToString() + " " : " ";
                                    existe0 = item.Peso.HasValue ? true : false;

                                    //6
                                    medidaTexto += medidaTexto.EndsWith(" ") && existe0 ? item.Peso.ToString() + item.UMMasa : "";
                                    existe0 = item.Otra == null || item.Otra == "" ? false : true;

                                    //otra
                                    medidaTexto += existe0 ? medidaTexto.Length > 0 ? ", " + item.Otra : item.Otra : "";

                                    attValor.Valor = medidaTexto;

                                    if (!string.IsNullOrWhiteSpace(attValor.Valor))
                                    {
                                        attFicha.Valores.Add(attValor);
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }

                if (attFicha.Valores.Count > 0)
                {
                    //Asignarles la etiqueta html

                    foreach (var etiquetaHTML in infoAtt.InicioHTML.Split(','))
                        if (!string.IsNullOrWhiteSpace(etiquetaHTML))
                            foreach (var itemValor in attFicha.Valores)
                                itemValor.Valor = "<" + etiquetaHTML + ">" + itemValor.Valor + "</" + etiquetaHTML + ">";


                    piezaMini.Atributos.Add(attFicha);
                }
            }

            imprimir.ListaPiezas.Add(piezaMini);

            if (imprimir.ConPiezasAdicionales)
            {
                foreach (var item in pieza.PiezasHijas)
                {
                    generarPiezaImprimir(item, imprimir);
                }
            }

        }


        [HttpPost]
        public ActionResult Imprimir(itemImprimir imprimir, string ListaPiezasID = "")
        {
            string[] listaID = ListaPiezasID.Split(',');
            string FormatoImpresion = "_FormatoBasicoPDF";
            imprimir.ListaPiezas = new List<itemPiezaMini>();

            foreach (var itemID in listaID)
            {
                var piezaTemp = db.Piezas.Find(new Guid(itemID));

                if (piezaTemp != null)
                {
                    //extraer toda la informacion de la pieza
                    //llenar la lista piezas
                    generarPiezaImprimir(piezaTemp, imprimir);
                }

            }

            string tipo = "";

            switch (imprimir.TipoImprimir)
            {
                case "Etiqueta":
                    tipo = "print-logoFondo";
                    break;

                default:
                    tipo = "";
                    break;
            }

            ViewBag.tipo = tipo;

            if (imprimir.TipoImprimir == "Etiqueta")
            {
                FormatoImpresion = "_FormatoEtiqueta5163PDF";



                return new ViewAsPdf(FormatoImpresion, imprimir)
                {
                    // FileName = flightPlan.ListingItemDetailsModel.FlightDetails + ".pdf",
                    PageSize = Size.Letter,
                    PageOrientation = Orientation.Portrait,
                    PageMargins = new Margins(0, 0, 0, 0),
                    //PageWidth = 210,
                    //PageHeight = 297
                    CustomSwitches = "--disable-smart-shrinking"
                };
            }

            return new ViewAsPdf(FormatoImpresion, imprimir);
        }


        #region Imprimir movimientos


        public ActionResult ImprimirBoletin(Guid? id)
        {
            string NombreArchivo = "Movimiento_";


            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            MovimientoTemp mov = db.MovimientosTemp.Find(id);
            if (mov == null) return HttpNotFound();


            //Movimiento con piezas aceptadas
            switch (mov.EstadoMovimiento)
            {
                //Imprimir todas las piezas
                case EstadoMovimientoTemp.Cancelado:
                case EstadoMovimientoTemp.Retornado:
                    mov.MovimientoTempPiezas = mov.MovimientoTempPiezas;
                    break;
                //Imprimir todas las piezas con estatus verdadero en SeMovio 
                case EstadoMovimientoTemp.Concluido:
                case EstadoMovimientoTemp.Concluido_SinValidar:
                    mov.MovimientoTempPiezas = mov.MovimientoTempPiezas.Where(a => a.SeMovio).ToList();
                    break;
                //Imprimir todas las piezas con estatus falso en EnError
                case EstadoMovimientoTemp.Procesando:
                    mov.MovimientoTempPiezas = mov.MovimientoTempPiezas.Where(a => !a.EnError).ToList();
                    break;
                //Imprimir ninguna de las piezas
                default:
                    mov.MovimientoTempPiezas = new List<MovimientoTempPieza>();
                    break;
            }


            var listaPiezas = new List<itemPiezaMini>();
            var i = 0;
            //Ordenas si no es 0
            //if (mov.MovimientoTempPiezas.Count > 1)
            //{
            foreach (var movPieza in mov.MovimientoTempPiezas.OrderBy(a => a.Pieza.Obra.LetraFolio.Nombre).ThenBy(a => a.Pieza.Obra.NumeroFolio).ThenBy(a => a.Pieza.SubFolio).ToList())
            {
                listaPiezas.Add(GenerarPiezaParaMovimientoImprimir(movPieza.Pieza, i));
                i++;
            }

            //Crear aqui la lista de piezas
            //}

            ViewData["ListaPiezasMov_" + id] = listaPiezas;


            NombreArchivo += mov.Folio;

            //return View("BoletinPDF",mov);
            return new ViewAsPdf("_FormatoMovimientoTemp", mov);
            //return new ActionAsPdf("~/Views/MovimientoTemp/BoletinPDF.cshtml", mov)
            //{
            //    FileName = NombreArchivo + ".pdf",
            //    PageSize = Rotativa.Options.Size.Letter
            //};

        }


        private itemPiezaMini GenerarPiezaParaMovimientoImprimir(Pieza pieza, int i = 0)
        {
            var piezaMini = new itemPiezaMini();


            string nombreDatos = "Basicos";
            string nombreArchivos = "Basicos";

            //Tipo de mostrar atributos: Basicos

            //Tipo de mostrar archivo: imagen_clave

            piezaMini.ObraID = pieza.ObraID;
            piezaMini.PiezaID = pieza.PiezaID;
            piezaMini.FolioObra = pieza.Obra.LetraFolio.Nombre + pieza.Obra.NumeroFolio;
            piezaMini.FolioPieza = pieza.ImprimirFolio();
            piezaMini.NombreObra = pieza.Obra.TipoObra.Nombre;
            piezaMini.NombrePieza = pieza.TipoPieza.Nombre;
            piezaMini.esPrincipal = pieza.TipoPieza.EsPrincipal;
            //piezaMini.esBusqueda = esBusqueda;
            //piezaMini.ListaPiezasHijas = new List<Guid>();
            piezaMini.Atributos = new List<itemPiezaMiniAtt>();
            piezaMini.UbicacionOrigenID = pieza.UbicacionID;

            //Encontrar Imagen de la pieza
            var tipoMostrarArchivos = db.TipoMostrarArchivos.FirstOrDefault(a => a.Nombre == nombreArchivos);
            TipoArchivo tipoArchivo = db.TipoArchivos.FirstOrDefault(a => a.Temp == "imagen_clave");


            var imagen = pieza.ArchivosPiezas.Where(a => a.Status && a.TipoArchivoID == tipoArchivo.TipoArchivoID && a.MostrarArchivos.Any(b => b.TipoMostrarArchivo.TipoMostrarArchivoID == tipoMostrarArchivos.TipoMostrarArchivoID && b.Status)).OrderBy(a => a.Orden).FirstOrDefault();
            if (imagen != null)
            {
                //Comprobar que el archivo exista
                FileInfo infoThumb = new FileInfo(Server.MapPath("~" + imagen.Ruta));

                if (infoThumb.Exists)
                {
                    piezaMini.ImagenID = imagen.ArchivoPiezaID;
                    piezaMini.RutaImagenMini = imagen.RutaThumb;
                }
            }

            //extraer una lista de los atributos que presenta la pieza
            var tipoMostrarDatos = db.TipoMostarlos.FirstOrDefault(a => a.Nombre == nombreDatos);
            List<Atributo> listaAtt = pieza.TipoPieza.Atributos.Where(a => a.Status && a.MostrarAtributos.Any(b => b.TipoMostrarID == tipoMostrarDatos.TipoMostrarID && b.Status) && a.TipoAtributo.Status).OrderBy(a => a.Orden).ToList();

            //llenar los atributos de la pieza
            foreach (var att in listaAtt)
            {
                var tipoAtt = att.TipoAtributo;
                var infoAtt = att.MostrarAtributos.FirstOrDefault(a => a.TipoMostrarID == tipoMostrarDatos.TipoMostrarID);

                infoAtt.InicioHTML = infoAtt.InicioHTML ?? "";
                infoAtt.FinHTML = infoAtt.FinHTML ?? "";


                var attFicha = new itemPiezaMiniAtt()
                {
                    Nombre = att.NombreAlterno,
                    Orden = att.Orden,
                    PiezaID = piezaMini.PiezaID,
                    AtributoID = att.AtributoID,
                    Valores = new List<itemPiezaMiniAttValor>()
                };

                if (tipoAtt.EsGenerico)
                {
                    var lista_AttPieza = pieza.AtributoPiezas.Where(a => a.Atributo == att).ToList();

                    if (lista_AttPieza.Count > 0)
                    {
                        foreach (var item in lista_AttPieza)
                        {
                            var attValor = new itemPiezaMiniAttValor()
                            {
                                AtributoPiezaID = item.AtributoPiezaID,
                                Orden = attFicha.Valores.Count + 1
                            };

                            if (tipoAtt.EsLista)
                            {
                                attValor.Valor = item.ListaValor.Valor;
                            }
                            else
                            {
                                attValor.Valor = item.Valor;
                            }

                            if (!string.IsNullOrWhiteSpace(attValor.Valor))
                            {
                                attFicha.Valores.Add(attValor);
                            }
                        }
                    }
                }
                else
                {
                    switch (tipoAtt.TablaSQL)
                    {
                        case "Autor":
                            var lista_AttAutor = pieza.AutorPiezas.Where(a => a.Status).OrderByDescending(a => a.esPrincipal).ThenBy(a => a.Prefijo).ThenBy(a => a.Autor.Nombre).ToList();
                            if (lista_AttAutor.Count > 0)
                            {
                                foreach (var item in lista_AttAutor)
                                {
                                    var attValor = new itemPiezaMiniAttValor()
                                    {
                                        AtributoPiezaID = item.AutorID,
                                        Orden = attFicha.Valores.Count + 1
                                    };

                                    var textoNombre = Regex.Replace(string.Format(item.Autor.Seudonimo + " " + item.Autor.Nombre + " " + item.Autor.ApellidoPaterno + " " + item.Autor.ApellidoMaterno).ToString().Trim(), @"\s+", " ");

                                    attValor.Valor = string.IsNullOrWhiteSpace(item.Prefijo) ? "" : item.Prefijo + ": " + textoNombre;

                                    if (!string.IsNullOrWhiteSpace(textoNombre))
                                    {
                                        attFicha.Valores.Add(attValor);
                                    }
                                }
                            }
                            break;

                        case "Ubicacion":
                            if (pieza.UbicacionID != null)
                            {
                                var attValor = new itemPiezaMiniAttValor()
                                {
                                    AtributoPiezaID = pieza.UbicacionID,
                                    Orden = 1,
                                    Valor = pieza.Ubicacion.Nombre
                                };

                                if (!string.IsNullOrWhiteSpace(attValor.Valor))
                                {
                                    attFicha.Valores.Add(attValor);
                                }

                            }
                            break;

                        case "TipoTecnica":
                            var lista_Tecnicas = pieza.TecnicaPiezas.Where(a => a.Status).OrderBy(a => a.TipoTecnica.Nombre).ToList();

                            if (lista_Tecnicas.Count > 0)
                            {
                                foreach (var item in lista_Tecnicas)
                                {
                                    var attValor = new itemPiezaMiniAttValor()
                                    {
                                        AtributoPiezaID = item.TecnicaID,
                                        Orden = attFicha.Valores.Count + 1
                                    };

                                    attValor.Valor = item.TipoTecnica.Nombre + ": " + item.Tecnica.Descripcion;

                                    if (!string.IsNullOrWhiteSpace(item.Tecnica.Descripcion))
                                    {
                                        attFicha.Valores.Add(attValor);
                                    }
                                }
                            }
                            break;

                        case "TipoMedida":
                            var lista_Medidas = pieza.MedidaPiezas.Where(a => a.Status).OrderBy(a => a.TipoMedida.Nombre).ToList();
                            if (lista_Medidas.Count > 0)
                            {
                                foreach (var item in lista_Medidas)
                                {
                                    var attValor = new itemPiezaMiniAttValor()
                                    {
                                        AtributoPiezaID = item.TipoMedidaID,
                                        Orden = attFicha.Valores.Count + 1
                                    };

                                    string medidaTexto = "";
                                    bool existe0 = false;
                                    bool existe1 = false;

                                    //1x
                                    existe0 = item.Altura.HasValue ? true : false;
                                    existe1 = item.Anchura.HasValue ? true : false;

                                    medidaTexto += existe0 ? item.Altura.ToString() : "";
                                    medidaTexto += existe0 && existe1 ? "x" : "";
                                    existe0 = existe1;
                                    existe1 = item.Profundidad.HasValue ? true : false;

                                    //2x
                                    medidaTexto += medidaTexto.EndsWith("x") ? "" : medidaTexto.Length > 0 && existe0 ? "x" : "";
                                    medidaTexto += existe0 ? item.Anchura.ToString() : "";
                                    medidaTexto += existe0 && existe1 ? "x" : "";
                                    existe0 = existe1;
                                    existe1 = item.Diametro.HasValue ? true : false;

                                    //3x
                                    medidaTexto += medidaTexto.EndsWith("x") ? "" : medidaTexto.Length > 0 && existe0 ? "x" : "";
                                    medidaTexto += existe0 ? item.Profundidad.ToString() : "";
                                    medidaTexto += existe0 && existe1 ? "x" : "";
                                    existe0 = existe1;
                                    existe1 = item.Diametro2.HasValue ? true : false;

                                    //4Øx
                                    medidaTexto += medidaTexto.EndsWith("x") ? "" : medidaTexto.Length > 0 && existe0 ? "x" : "";
                                    medidaTexto += existe0 ? item.Diametro.ToString() + "Ø" : "";
                                    medidaTexto += existe0 && existe1 ? "x" : "";
                                    existe0 = existe1;
                                    existe1 = item.UMLongitud.HasValue ? true : false;

                                    //cm
                                    medidaTexto += medidaTexto.EndsWith("x") ? "" : medidaTexto.Length > 0 && existe0 ? "x" : "";
                                    medidaTexto += existe0 ? item.Diametro2.ToString() + "Ø" : "";
                                    medidaTexto += existe1 && medidaTexto.Length > 0 ? item.UMLongitud.ToString() + " " : " ";
                                    existe0 = item.Peso.HasValue ? true : false;

                                    //6
                                    medidaTexto += medidaTexto.EndsWith(" ") && existe0 ? item.Peso.ToString() + item.UMMasa : "";
                                    existe0 = item.Otra == null || item.Otra == "" ? false : true;

                                    //otra
                                    medidaTexto += existe0 ? medidaTexto.Length > 0 ? ", " + item.Otra : item.Otra : "";

                                    attValor.Valor = medidaTexto;

                                    if (!string.IsNullOrWhiteSpace(attValor.Valor))
                                    {
                                        attFicha.Valores.Add(attValor);
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }

                if (attFicha.Valores.Count > 0)
                {
                    //Asignarles la etiqueta html

                    foreach (var etiquetaHTML in infoAtt.InicioHTML.Split(','))
                        if (!string.IsNullOrWhiteSpace(etiquetaHTML))
                            foreach (var itemValor in attFicha.Valores)
                                itemValor.Valor = "<" + etiquetaHTML + ">" + itemValor.Valor + "</" + etiquetaHTML + ">";


                    piezaMini.Atributos.Add(attFicha);
                }
            }

            piezaMini.Lado = (i % 2 == 0) ? "left" : "right";


            return piezaMini;
        }

        #endregion

    }


}