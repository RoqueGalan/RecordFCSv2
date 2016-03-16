using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RecordFCS_Alt.Models;
using RecordFCS_Alt.Helpers.Seguridad;
using System.Globalization;
using PagedList;
using RecordFCS_Alt.Models.ViewModels;
using Rotativa;
using RecordFCS_Alt.Models.ViewsModel;
using System.Text.RegularExpressions;
using System.IO;
using RecordFCS_Alt.Helpers.Historial;
using System.Threading.Tasks;

namespace RecordFCS_Alt.Controllers
{
    public class MovimientoTempController : BaseController
    {
        private RecordFCSContext db = new RecordFCSContext();

        #region Index
        // GET: MovimientoTemp
        [CustomAuthorize(permiso = "movList")]
        public ActionResult Index(MovimientoTemp MovTemp = null)
        {
            db = new RecordFCSContext();

            var listaLetras = db.LetraFolios.Select(a => new { a.LetraFolioID, Nombre = a.Nombre, a.Status }).Where(a => a.Status).OrderBy(a => a.Nombre);
            ViewBag.LetraFolioID = new SelectList(listaLetras, "LetraFolioID", "Nombre", listaLetras.FirstOrDefault().LetraFolioID);


            var listaUbicaciones = db.Ubicaciones.Where(a => a.Status).Select(a => new { a.Nombre, a.UbicacionID }).OrderBy(a => a.Nombre);

            ViewBag.UbicacionDestinoID = new SelectList(listaUbicaciones, "UbicacionID", "Nombre");
            ViewBag.UbicacionOrigenID = new SelectList(listaUbicaciones, "UbicacionID", "Nombre");

            ViewBag.TipoMovimientoID = new SelectList(db.TipoMovimientos.Where(a => a.Status).OrderBy(a => a.Nombre), "TipoMovimientoID", "Nombre", MovTemp.TipoMovimientoID);

            if (MovTemp == null)
            {
                MovTemp = new MovimientoTemp()
                {
                    TieneExposicion = false,
                    EstadoMovimiento = null
                };
            }

            return View(MovTemp);
        }

        #endregion

        #region Buscar Movimientos

        [CustomAuthorize(permiso = "")]
        public ActionResult BuscarMovimientos(int? FolioMovimiento, string FechaInicial, string FechaFinal, Guid? UbicacionOrigenID, Guid? UbicacionDestinoID, string PalabraFrase, EstadoMovimientoTemp? EstadoMovimiento, int? pagina = null)
        {
            db = new RecordFCSContext();

            int pagTamano = 50;
            int pagIndex = 1;
            pagIndex = pagina.HasValue ? Convert.ToInt32(pagina) : 1;

            IQueryable<MovimientoTemp> listaMovimientos = db.MovimientosTemp;
            IPagedList<MovimientoTemp> listaMovimientosEnPagina;

            if (FolioMovimiento != null && FolioMovimiento > 0)
            {
                //realizar la busqueda exclusiva del folio de movimiento
                listaMovimientos = listaMovimientos.Where(a => a.Folio == FolioMovimiento);
            }
            else
            {
                //if (!string.IsNullOrWhiteSpace(FolioObraPieza))
                //{
                //    //realizar la busqueda exclusica del folio de la obra o pieza
                //    listaMovimientos = listaMovimientos.Where(a => a.MovimientoTempPiezas.Any(b => (b.Pieza.Obra.LetraFolio.Nombre + b.Pieza.Obra.NumeroFolio) == FolioObraPieza || (b.Pieza.Obra.LetraFolio.Nombre + b.Pieza.Obra.NumeroFolio + b.Pieza.SubFolio) == FolioObraPieza));
                //}

                //Buscar por fecha Iniciar
                if (!string.IsNullOrWhiteSpace(FechaInicial))
                {
                    //Fecha menor
                    DateTime Fecha = DateTime.ParseExact(FechaInicial, "dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture);
                    listaMovimientos = listaMovimientos.Where(a => DateTime.Compare(a.FechaSalida.Value, Fecha) > 0);
                }

                //Buscar por fecha Final
                if (!string.IsNullOrWhiteSpace(FechaFinal))
                {
                    //Fecha mayor
                    DateTime Fecha = DateTime.ParseExact(FechaFinal, "dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture);
                    listaMovimientos = listaMovimientos.Where(a => DateTime.Compare(a.FechaSalida.Value, Fecha) < 0);
                }

                //Buscar por Ubicacion Origen
                if (UbicacionOrigenID != null)
                    listaMovimientos = listaMovimientos.Where(a => a.UbicacionOrigenID == UbicacionOrigenID);

                //Buscar por Ubicacion Destino
                if (UbicacionDestinoID != null)
                    listaMovimientos = listaMovimientos.Where(a => a.UbicacionDestinoID == UbicacionDestinoID);

                //Buscar por Palabra o Frase
                if (!string.IsNullOrEmpty(PalabraFrase))
                    listaMovimientos = listaMovimientos.Where(a => a.Observaciones.Contains(PalabraFrase) ||
                        a.Exposicion_Titulo.Contains(PalabraFrase) ||
                        a.Seguro_Notas.Contains(PalabraFrase) ||
                        a.Transporte_Notas.Contains(PalabraFrase) ||
                        a.Transporte_Recorrido.Contains(PalabraFrase)
                        );

                //Buscar por Estado del movimiento
                if (EstadoMovimiento != null)
                    listaMovimientos = listaMovimientos.Where(a => a.EstadoMovimiento == EstadoMovimiento);
            }

            listaMovimientosEnPagina = listaMovimientos.OrderByDescending(a => a.FechaSalida).Select(x => x).ToList().ToPagedList(pagIndex, pagTamano);

            return PartialView("_Lista", listaMovimientosEnPagina);
        }

        #endregion


        #region Detalles

        // GET: MovimientoTemp/Detalles/5
        [CustomAuthorize(permiso = "movDeta")]
        public ActionResult Detalles(Guid? id)
        {
            db = new RecordFCSContext();

            #region Validar que exista el movimiento

            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            MovimientoTemp mov = db.MovimientosTemp.Find(id);
            if (mov == null) return HttpNotFound();

            #endregion

            #region Navegacion de movimientos

            // 1 2 3 4 [5] 6 7 8 9 10
            // [1] 2 3 4 5 6 7 8 9 10
            // 1 2 3 4 5 6 7 8 9 9 [10]

            MovimientoTemp movTemp = null;
            movTemp = db.MovimientosTemp.Where(a => a.Folio < mov.Folio).OrderByDescending(a => a.Folio).FirstOrDefault();
            ViewBag.MovAnterior = movTemp == null ? Guid.Empty : movTemp.MovimientoTempID;

            movTemp = db.MovimientosTemp.Where(a => a.Folio > mov.Folio).OrderBy(a => a.Folio).FirstOrDefault();
            ViewBag.MovSiguiente = movTemp == null ? Guid.Empty : movTemp.MovimientoTempID;

            #endregion

            bool BtnCancelarEnabled = false;
            bool BtnRevertirEnabled = false;
            bool BtnEditarEnabled = true;
            bool BtnImprimirEnabled = false;



            //TipoArchivo tipoArchivo = db.TipoArchivos.FirstOrDefault(a => a.Temp == "imagen_clave");

            //Tipo de atributo a mostrar
            var TipoAttTitulo = db.TipoAtributos.FirstOrDefault(a => a.Temp == "titulo");

            //lista de piezas guardadas en el moviento


            var lista = db.MovimientoTempPiezas
                       .Where(a => a.MovimientoTempID == mov.MovimientoTempID)
                       .OrderBy(a => a.Pieza.Obra.LetraFolio.Nombre)
                       .ThenBy(a => a.Pieza.Obra.NumeroFolio)
                       .ThenBy(a => a.Pieza.SubFolio)
                       .Select(a => new Item_MovPieza()
                       {
                           ObraID = a.Pieza.ObraID,
                           FolioObra = a.Pieza.Obra.LetraFolio.Nombre + a.Pieza.Obra.NumeroFolio,
                           FolioPieza = a.FolioPieza,//a.Pieza.Obra.LetraFolio.Nombre + a.Pieza.Obra.NumeroFolio + a.
                           PiezaID = a.PiezaID,
                           UbicacionID = a.Pieza.UbicacionID,
                           EnError = a.EnError,
                           EsPendiente = a.EsPendiente,
                           SeMovio = a.SeMovio,
                           ExisteEnMov = true,
                           Comentario = a.Comentario,
                           EsUltimo = a.Pieza.MovimientoTempPiezas.Where(b => b.Orden > a.Orden && !a.EnError).Count() > 0 ? false : true
                       }).ToList();


            //foreach (var item in lista)
            //{
            //    var p = db.Piezas.AsNoTracking().FirstOrDefault(a => a.PiezaID == item.PiezaID);

            //    item.FolioPieza = p.ImprimirFolio();
            //}

            //lista de Items de tipo movimiento pieza
            //var lista = new List<Item_MovPieza>();

            //Crear la lista
            //foreach (var item in lista)
            //{
            //    db = new RecordFCSContext();
            //    db.Configuration.ValidateOnSaveEnabled = false;
            //    db.Configuration.AutoDetectChangesEnabled = false;

            //    item.FolioPieza = db.Piezas.FirstOrDefault(a => a.PiezaID == item.PiezaID).ImprimirFolio();

            //    //var obj = new Item_MovPieza()
            //    //{
            //    //    ObraID = item.Pieza.ObraID,
            //    //    PiezaID = item.PiezaID,
            //    //    UbicacionID = item.Pieza.UbicacionID,
            //    //    EnError = item.EnError,
            //    //    EsPendiente = item.EsPendiente,
            //    //    SeMovio = item.SeMovio,
            //    //    ExisteEnMov = true,
            //    //    Comentario = item.Comentario,
            //    //};

            //    //obj.FolioObra = item.Pieza.Obra.LetraFolio.Nombre + item.Pieza.Obra.NumeroFolio;
            //    //obj.FolioPieza = item.Pieza.ImprimirFolio();

            //    ////var imagen = item.ArchivosPiezas.Where(b => b.Status && b.TipoArchivoID == tipoArchivo.TipoArchivoID && b.MostrarArchivos.Any(c => c.TipoMostrarArchivoID == tipoMostrarArchivo.TipoMostrarArchivoID && c.Status)).OrderBy(b => b.Orden).FirstOrDefault();
            //    ////obj.Imagen = imagen == null ? "" : imagen.RutaThumb;

            //    //////Extraer el autor
            //    ////try
            //    ////{
            //    ////    var autor = item.Pieza.AutorPiezas.OrderBy(b => b.Orden).FirstOrDefault(b => b.esPrincipal && b.Status);
            //    ////    obj.Autor = autor.Autor.Seudonimo + " " + autor.Autor.Nombre + " " + autor.Autor.ApellidoPaterno + " " + autor.Autor.ApellidoMaterno;
            //    ////}
            //    ////catch (Exception)
            //    ////{
            //    ////    obj.Autor = "Sin autor";
            //    ////}

            //    //////Extraer el titulo
            //    ////try
            //    ////{
            //    ////    var titulo = item.Pieza.AtributoPiezas.FirstOrDefault(b => b.Atributo.TipoAtributoID == TipoAttTitulo.TipoAtributoID);
            //    ////    obj.Titulo = titulo.Valor;
            //    ////}
            //    ////catch (Exception)
            //    ////{
            //    ////    obj.Titulo = "Sin titulo";
            //    ////}

            //    ////obj.TotalPiezas = item.PiezasHijas.Count();

            //    ////Saber si es el ultimo registro (Este no sirve, solo como referencia)
            //    //obj.EsUltimo = item.Pieza.MovimientoTempPiezas.Where(a => a.Orden > item.Orden && !a.EnError).Count() > 0 ? false : true;

            //    //lista.Add(obj);
            //}

            ViewData["ListaPiezasMov_" + id] = lista;

            List<string> InfoEstadoMov = new List<string>();



            #region Crear la informacion del movimiento

            #region Comparar la fecha

            var limiteDias = 2; //limite de dias para validar

            var fechaActual = DateTime.Now; // fecha y hora actuales
            var fechaMov = mov.FechaSalida.Value; // fecha de execucion del movimiento

            var fechaTemp = fechaActual; // fecha temporal con la fehca actual
            var totalDias = (fechaTemp - fechaMov).Days; // extraer los dias faltantes
            fechaTemp = fechaTemp.AddDays(totalDias); //agregar los dias faltantes para ir acercandonos mas a la fecha
            var totalHoras = (fechaTemp - fechaMov).Hours; // extraer las horas faltantes
            fechaTemp = fechaTemp.AddHours(totalHoras); //agregar las horas faltaqntes para acercarn¡nos mas a la fecha
            var totalMinutos = (fechaTemp - fechaMov).Minutes; // extraer los minutos faltantes

            #endregion

            switch (mov.EstadoMovimiento)
            {
                case EstadoMovimientoTemp.Cancelado:
                    BtnRevertirEnabled = true;

                    break;

                case EstadoMovimientoTemp.Concluido:
                    BtnCancelarEnabled = true;

                    break;

                case EstadoMovimientoTemp.Concluido_SinValidar:
                    BtnCancelarEnabled = true;

                    InfoEstadoMov.Add("Movimiento Concluido <b>SIN VALIDACION</b>");

                    var fechaLimite = fechaMov.AddDays(limiteDias); // agregar los dias limites para crear la fecha limite

                    if (fechaActual < fechaLimite)
                    {
                        fechaTemp = fechaActual; // fecha temporal con la fehca actual



                        totalDias = (fechaLimite - fechaActual).Days; // extraer los dias faltantes
                        fechaTemp = fechaTemp.AddDays(totalDias); //agregar los dias faltantes para ir acercandonos mas a la fecha
                        totalHoras = (fechaLimite - fechaActual).Hours; // extraer las horas faltantes
                        fechaTemp = fechaTemp.AddHours(totalHoras); //agregar las horas faltaqntes para acercarn¡nos mas a la fecha
                        totalMinutos = (fechaLimite - fechaActual).Minutes; // extraer los minutos faltantes


                        InfoEstadoMov.Add("En máximo " + limiteDias + " dias se revertira el estado del movimiento a <b>RETORNADO</b>. Validar lo antes posible.");
                        InfoEstadoMov.Add("Tiempo estimado: <b>" + totalDias + " dias, " + totalHoras + " hrs y " + totalMinutos + " mins.</b>");
                    }
                    else
                        InfoEstadoMov.Add("En unos instantes se <b>REVERTIRA</b> el movimiento.");


                    break;

                case EstadoMovimientoTemp.Procesando:
                    BtnCancelarEnabled = true;

                    if (mov.MovimientoTempPiezas.Where(a => (a.SeMovio || a.EsPendiente) && !a.EnError).Count() == 0)
                        InfoEstadoMov.Add("No contiene obras/piezas. No se ejecutara hasta agregar por lo menos una valida.");
                    else
                    {
                        if (mov.EsValido)
                            InfoEstadoMov.Add("Movimiento en Proceso.");
                        else
                            InfoEstadoMov.Add("Movimiento en Proceso <b>SIN VALIDACION</b>. Validar lo antes posible.");

                        InfoEstadoMov.Add("Tiempo estimado: <b>" + totalDias + " dias, " + totalHoras + " hrs y " + totalMinutos + " mins.</b>");
                    }

                    BtnCancelarEnabled = true;
                    break;

                case EstadoMovimientoTemp.Retornado:

                    BtnRevertirEnabled = true;

                    InfoEstadoMov.Add("Movimiento retornado");
                    InfoEstadoMov.Add("Tiempo que retorno: <b>" + totalDias + " dias, " + totalHoras + " hrs y " + totalMinutos + " mins.</b>");

                    break;

                default:
                    InfoEstadoMov.Add("Error");
                    break;
            }

            #endregion


            BtnImprimirEnabled = (mov.MovimientoTempPiezas.Where(a => (a.SeMovio || a.EsPendiente) && !a.EnError).Count() == 0) ? false : true;




            ViewBag.InfoEstadoMov = InfoEstadoMov;

            #region Botones

            //logica de los botones
            ViewBag.BtnCancelarEnabled = BtnCancelarEnabled;
            ViewBag.BtnRevertirEnabled = BtnRevertirEnabled;
            ViewBag.BtnEditarEnabled = BtnEditarEnabled;
            ViewBag.BtnImprimirEnabled = BtnImprimirEnabled;

            #endregion


            return View(mov);
        }

        #endregion



        #region Imprimir Boletin

        // GET: MovimientoTemp/Detalles/5
        //[CustomAuthorize(permiso = "")]
        public ActionResult ImprimirBoletin(Guid? id)
        {
            db = new RecordFCSContext();

            string NombreArchivo = "Boletin_";

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

            //Ordenas si no es 0
            if (mov.MovimientoTempPiezas.Count > 1)
                mov.MovimientoTempPiezas = mov.MovimientoTempPiezas.OrderBy(a => a.Pieza.Obra.LetraFolio.Nombre).ThenBy(a => a.Pieza.Obra.NumeroFolio).ThenBy(a => a.Pieza.SubFolio).ToList();


            TipoArchivo tipoArchivo = db.TipoArchivos.FirstOrDefault(a => a.Temp == "imagen_clave");

            NombreArchivo += mov.Folio;

            //return View("BoletinPDF",mov);
            return new ViewAsPdf("BoletinPDF", mov);
            //return new ActionAsPdf("~/Views/MovimientoTemp/BoletinPDF.cshtml", mov)
            //{
            //    FileName = NombreArchivo + ".pdf",
            //    PageSize = Rotativa.Options.Size.Letter
            //};

        }

        #endregion

        #region Imprimir Ficha

        public ActionResult FichaPrint(Guid? id, int i)
        {
            db = new RecordFCSContext();

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Pieza pieza = db.Piezas.Find(id);

            if (pieza == null)
                return HttpNotFound();

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

            //extraer la lista de att de la pieza en guion
            var listaAttMovFicha = pieza.TipoPieza.Atributos.Where(a => a.Status && a.MostrarAtributos.Any(b => b.TipoMostrar.Nombre == "Basicos" && b.Status) && a.TipoAtributo.Status).OrderBy(a => a.Orden).ToList();

            //llenar los attFicha
            foreach (var att in listaAttMovFicha)
            {
                var tipoAtt = att.TipoAtributo;

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

                                    if (!string.IsNullOrWhiteSpace(attValor.Valor))
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
                    piezaMini.Atributos.Add(attFicha);
                }
            }

            //var imagen = pieza.ImagenPiezas.OrderBy(a => a.Orden).FirstOrDefault(a => a.Status && a.EsPrincipal);
            TipoArchivo tipoArchivo = db.TipoArchivos.FirstOrDefault(a => a.Temp == "imagen_clave");

            //Solo imagen que sea basica y sea la primera
            var imagen = pieza.ArchivosPiezas.Where(a => a.Status && a.TipoArchivoID == tipoArchivo.TipoArchivoID && a.MostrarArchivos.Any(b => b.TipoMostrarArchivo.Nombre == "Basicos" && b.Status)).OrderBy(a => a.Orden).FirstOrDefault();


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

            //pieza.TipoPieza.TipoPiezasHijas = pieza.TipoPieza.TipoPiezasHijas.Where(a => a.Status).OrderBy(a => a.Orden).ToList();

            //ViewBag.listaAttributosFichaCompleta = listaAttributosFicha;
            string lado = "";

            if (i % 2 == 0)
                lado = "left";
            else
                lado = "right";

            ViewBag.lado = lado;

            return PartialView("_FichaPrint", piezaMini);
        }

        #endregion



        #region Crear GET

        // GET: MovimientoTemp/Create
        [CustomAuthorize(permiso = "movNew")]
        public ActionResult Crear(Guid? TipoMovimientoID, bool TieneExposicion)
        {
            db = new RecordFCSContext();

            var tipoMov = db.TipoMovimientos.Find(TipoMovimientoID);

            if (tipoMov == null) return HttpNotFound();

            var mov = new MovimientoTemp()
            {
                TieneExposicion = TieneExposicion,
                TipoMovimientoID = tipoMov.TipoMovimientoID,
                TipoMovimiento = tipoMov,
                EstadoMovimiento = EstadoMovimientoTemp.Procesando,
                EsValido = false
                //Folio = db.MovimientosTemp.Select(a => a.Folio).OrderBy(a => a).SingleOrDefault() + 1

            };


            mov.Folio = db.MovimientosTemp.Select(a => a.Folio).OrderByDescending(a => a).FirstOrDefault() + 1;


            var listaUbicaciones = db.Ubicaciones.Where(a => a.Status).Select(a => new { a.Nombre, a.UbicacionID }).OrderBy(a => a.Nombre);

            ViewBag.UbicacionDestinoID = new SelectList(listaUbicaciones, "UbicacionID", "Nombre");
            ViewBag.UbicacionOrigenID = new SelectList(listaUbicaciones, "UbicacionID", "Nombre");

            return View(mov);
        }

        #endregion

        #region Crear POST

        // POST: MovimientoTemp/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permiso = "movNew")]
        public ActionResult Crear(MovimientoTemp movimientoTemp)
        {
            db = new RecordFCSContext();

            //Validacion 
            if (movimientoTemp.FechaSalida == null)
                ModelState.AddModelError("FechaSalida", "Ingrese una fecha.");

            if (movimientoTemp.UbicacionOrigenID == null)
                ModelState.AddModelError("UbicacionOrigenID", "Seleccione la ubicación.");

            if (movimientoTemp.UbicacionDestinoID == null)
                ModelState.AddModelError("UbicacionDestinoID", "Seleccione la ubicación.");



            var fechaCreacion = DateTime.Now;

            movimientoTemp.FechaUltimaEjecucion = fechaCreacion;
            //Validar ultimo Folioº
            try
            {
                if (ModelState.IsValid)
                {
                    //Crear la entidad
                    movimientoTemp.Folio = db.MovimientosTemp.Select(a => a.Folio).OrderByDescending(a => a).FirstOrDefault() + 1;

                    movimientoTemp.EstadoMovimiento = EstadoMovimientoTemp.Procesando;
                    movimientoTemp.UsuarioID = User.UsuarioID;
                    movimientoTemp.MovimientoTempID = Guid.NewGuid();
                    db.MovimientosTemp.Add(movimientoTemp);


                    //------ Logica HISTORIAL

                    #region Generar el historial

                    // Generar el historial
                    var historialLog =
                        HistorialLogica.CrearEntidad(
                        movimientoTemp,
                        movimientoTemp.GetType().Name,
                        movimientoTemp.MovimientoTempID.ToString(),
                        User.UsuarioID,
                        db,
                        fechaCreacion);

                    #endregion

                    #region Guardar el historial

                    //Guardar cambios si todo salio correcto
                    if (historialLog != null)
                    {
                        //Guardar la entidad
                        db.SaveChanges();

                        //Guardar el historial
                        db.HistorialLogs.Add(historialLog);
                        db.SaveChanges();
                    }
                    else
                    {
                        throw new Exception();
                    }

                    #endregion

                    //------

                    //Logica para terminar la instruccion
                    AlertaSuccess("Se registro el movimiento: <b>" + movimientoTemp.Folio + "</b>", true);

                    return Json(new { success = true, url = Url.Action("Detalles", new { id = movimientoTemp.MovimientoTempID }) });
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Error desconocido.");
            }


            //Regresar la vista
            var tipoMov = db.TipoMovimientos.Find(movimientoTemp.TipoMovimientoID);

            if (tipoMov == null) return HttpNotFound();

            movimientoTemp.TipoMovimiento = tipoMov;
            movimientoTemp.TipoMovimientoID = tipoMov.TipoMovimientoID;
            movimientoTemp.EsValido = false;
            movimientoTemp.EstadoMovimiento = EstadoMovimientoTemp.Procesando;

            movimientoTemp.Folio = db.MovimientosTemp.Select(a => a.Folio).OrderByDescending(a => a).FirstOrDefault() + 1;

            var listaUbicaciones = db.Ubicaciones.Where(a => a.Status).Select(a => new { a.Nombre, a.UbicacionID }).OrderBy(a => a.Nombre);

            ViewBag.UbicacionDestinoID = new SelectList(listaUbicaciones, "UbicacionID", "Nombre", movimientoTemp.UbicacionDestinoID);
            ViewBag.UbicacionOrigenID = new SelectList(listaUbicaciones, "UbicacionID", "Nombre", movimientoTemp.UbicacionOrigenID);

            return View(movimientoTemp);
        }

        #endregion



        #region Editar GET

        // GET: MovimientoTemp/Edit/5
        [CustomAuthorize(permiso = "movEdit")]
        public ActionResult Editar(Guid? id, bool EnExpo = false)
        {
            db = new RecordFCSContext();

            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            MovimientoTemp mov = db.MovimientosTemp.Find(id);

            if (mov == null) return HttpNotFound();

            if (!mov.TieneExposicion && EnExpo)
                mov.TieneExposicion = EnExpo;

            var listaUbicaciones = db.Ubicaciones.Where(a => a.Status).Select(a => new { a.Nombre, a.UbicacionID }).OrderBy(a => a.Nombre);


            //Ubicacion destino no puede editarse si se encuentra por lo menos una Pieza Procesada
            ViewBag.UbiDestinoEnabled = mov.MovimientoTempPiezas.Where(a => a.SeMovio).Count() == 0 ? true : false;

            ViewBag.FechaSalidaEnabled = (mov.EstadoMovimiento == EstadoMovimientoTemp.Procesando) ? true : false;

            //Solo habilitar el forzar cuando: MovimientoEstado = Procesando, Concluido,Concluido_SinValidar , 
            ViewBag.ForzarMovimientoEnabled =
                (
                mov.EstadoMovimiento == EstadoMovimientoTemp.Concluido ||
                mov.EstadoMovimiento == EstadoMovimientoTemp.Concluido_SinValidar ||
                mov.EstadoMovimiento == EstadoMovimientoTemp.Procesando
                )
                ? true : false;


            ViewBag.UbicacionDestinoID = new SelectList(listaUbicaciones, "UbicacionID", "Nombre", mov.UbicacionDestinoID);


            #region Validar navegacion de movimientos

            // 1 2 3 4 [5] 6 7 8 9 10
            // [1] 2 3 4 5 6 7 8 9 10
            // 1 2 3 4 5 6 7 8 9 9 [10]

            MovimientoTemp movTemp = null;
            movTemp = db.MovimientosTemp.Where(a => a.Folio < mov.Folio).OrderByDescending(a => a.Folio).FirstOrDefault();
            ViewBag.MovAnterior = movTemp == null ? Guid.Empty : movTemp.MovimientoTempID;

            movTemp = db.MovimientosTemp.Where(a => a.Folio > mov.Folio).OrderBy(a => a.Folio).FirstOrDefault();
            ViewBag.MovSiguiente = movTemp == null ? Guid.Empty : movTemp.MovimientoTempID;

            #endregion


            return View(mov);
        }

        #endregion

        #region Editar POST

        // POST: MovimientoTemp/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permiso = "movEdit")]
        public ActionResult Editar(MovimientoTemp movimientoTemp, List<Item_MovPieza> ListaPiezas, string Motivo, bool ForzarMovimiento = false)
        {
            db = new RecordFCSContext();

            #region Validaciones previas


            if (movimientoTemp.EstadoMovimiento == EstadoMovimientoTemp.Concluido_SinValidar && movimientoTemp.EsValido)
                movimientoTemp.EstadoMovimiento = EstadoMovimientoTemp.Concluido;

            ////cambiar el estado del movimiento dependiendo la fecha y hora
            //if (movimientoTemp.EstadoMovimiento != EstadoMovimientoTemp.Cancelado)
            //{
            //    if (movimientoTemp.FechaSalida < DateTime.Now)
            //    {
            //        movimientoTemp.EstadoMovimiento = EstadoMovimientoTemp.Concluido;
            //    }
            //    else
            //    {
            //        movimientoTemp.EstadoMovimiento = EstadoMovimientoTemp.Procesando;
            //    }
            //}

            //Separar en listas las piezas

            //lista de piezas existentes en el movimiento
            var listaGuidActual = db.MovimientoTempPiezas.Where(a => a.MovimientoTempID == movimientoTemp.MovimientoTempID).Select(a => a.PiezaID).ToList();
            //lista de piezas que se agregaran al movimiento
            var listaAdd = new List<MovimientoTempPieza>();
            //lista de piezas que se editaran 
            var listaEdit = new List<MovimientoTempPieza>();

            ListaPiezas = ListaPiezas == null ? new List<Item_MovPieza>() : ListaPiezas;

            foreach (var item in ListaPiezas)
            {
                var existeEnMov = listaGuidActual.Where(a => a == item.PiezaID).Count() > 0 ? true : false;

                var temp = new MovimientoTempPieza()
                {
                    Comentario = item.Comentario,
                    EnError = item.EnError,
                    EsPendiente = item.EsPendiente,
                    MovimientoTempID = movimientoTemp.MovimientoTempID,
                    PiezaID = item.PiezaID,
                    SeMovio = item.SeMovio,
                    FolioPieza = item.FolioPieza
                };

                temp.Comentario = item.EnError ? item.Comentario : "";

                if (!existeEnMov)
                {
                    // C R E A R
                    temp.EsPendiente = true;
                    temp.SeMovio = false;
                    listaAdd.Add(temp);
                }
                else
                {
                    // E D I T A R
                    listaEdit.Add(temp);

                    //quitarlo de la lista existente
                    listaGuidActual.Remove(item.PiezaID);
                }
            }

            #endregion


            //las piezas que no estuvieron eliminarlas
            //y buscar el movimiento anterior donde fueron validas
            //para regresar el estatus.
            if (ModelState.IsValid)
            {
                var fechaEdicion = DateTime.Now;

                movimientoTemp.FechaUltimaEjecucion = fechaEdicion;

                #region Crear el historial del movimiento

                //------ Logica HISTORIAL

                #region Generar el historial

                //objeto del formulario
                var objeto = movimientoTemp;
                //Objeto de la base de datos
                var objetoDB = db.MovimientosTemp.Find(movimientoTemp.MovimientoTempID);
                //tabla o clase a la que pertenece
                var tablaNombre = objeto.GetType().Name;
                //llave primaria del objeto
                var llavePrimaria = objetoDB.MovimientoTempID.ToString();

                //generar el historial
                var historialLog = HistorialLogica.EditarEntidad(
                    objeto,
                    objetoDB,
                    tablaNombre,
                    llavePrimaria,
                    User.UsuarioID,
                    db,
                    Motivo,
                    fechaEdicion
                    );

                #endregion

                #region Guardar el historial

                if (historialLog != null)
                {
                    //Cambiar el estado a la entidad a modificada
                    db.Entry(objetoDB).State = EntityState.Modified;
                    //Guardamos la entidad modificada
                    db.SaveChanges();

                    //Guardar el historial
                    db.HistorialLogs.Add(historialLog);
                    db.SaveChanges();
                }

                #endregion

                //------

                #endregion

                AlertaSuccess("Se edito el movimiento: <b>" + movimientoTemp.Folio + "</b>", true);

                //Crear el historial de las piezas del movimiento
                //Agregar, Editar y Eliminar las piezas
                //No se cambiara el estatus a las piezas ya que esto se hara automaticamente o mediante permisos especiales

                //Agregar
                #region Lista Agregar

                db = new RecordFCSContext();
                db.Configuration.LazyLoadingEnabled = false;

                foreach (var item in listaAdd)
                {

                    var ordenActual = db.MovimientoTempPiezas.Where(a => a.PiezaID == item.PiezaID).Count() > 0 ? db.MovimientoTempPiezas.Where(a => a.PiezaID == item.PiezaID).OrderByDescending(a => a.Orden).FirstOrDefault().Orden : 0;

                    item.Orden = item.EnError ? 0 : ordenActual + 1;

                    var temp = ListaPiezas.FirstOrDefault(a => a.PiezaID == item.PiezaID);

                    db.MovimientoTempPiezas.Add(item);

                    //------ Logica HISTORIAL

                    #region Generar el historial

                    //generar el historial
                    var historialLogAdd = HistorialLogica.CrearEntidad(
                        item,
                        "MovimientoTempPieza",
                        item.MovimientoTempID + "," + item.PiezaID,
                        User.UsuarioID,
                        db,
                        fechaEdicion
                        );

                    #endregion

                    #region Agrega el historial

                    if (historialLogAdd != null)
                    {
                        //Guardar el historial
                        db.HistorialLogs.Add(historialLogAdd);
                        AlertaSuccess("Se agrego la pieza [<b>" + temp.FolioPieza + "</b>]", true);
                    }
                    else
                    {
                        AlertaSuccess("No se pudo agregar la pieza [<b>" + temp.FolioPieza + "</b>]", true);
                    }
                    #endregion

                    //------


                }

                #region Guardar el historial y la entidad

                //Guardar todo 
                db.SaveChanges();

                #endregion

                #endregion

                //Editar
                #region Lista Editar

                db = new RecordFCSContext();
                db.Configuration.LazyLoadingEnabled = false;

                foreach (var item in listaEdit.ToList())
                {

                    //Piezas SeMovio = no se pueden edita

                    //buscar la pieza
                    //var pieza = db.Piezas.Find(item.PiezaID);
                    var piezaMovOriginal = db.MovimientoTempPiezas.FirstOrDefault(a => a.PiezaID == item.PiezaID && a.MovimientoTempID == movimientoTemp.MovimientoTempID);


                    if (piezaMovOriginal.SeMovio)
                    {
                        listaEdit.Remove(item);
                    }
                    else
                    {
                        //saber si la pieza cambio en el campo error
                        //valor original - valor nuevo
                        bool ConCambios = piezaMovOriginal.EnError != item.EnError;

                        if (ConCambios)
                        {
                            //Sufrio cambios
                            if (item.EnError)
                            {
                                //ahora con errores
                                //item.Orden = 0;
                                item.EnError = true;
                                item.EsPendiente = true;
                                item.SeMovio = false;
                            }
                            else
                            {
                                //ahora sin errores
                                if (piezaMovOriginal.Orden == 0)
                                {
                                    //generar un orden
                                    var ordenActual = db.MovimientoTempPiezas.Where(a => a.PiezaID == item.PiezaID).Count() > 0 ? db.MovimientoTempPiezas.Where(a => a.PiezaID == item.PiezaID).OrderByDescending(a => a.Orden).FirstOrDefault().Orden : 0;
                                    item.Orden = item.EnError ? 0 : ordenActual + 1;
                                }

                                item.Comentario = "";
                                item.EnError = false;
                                item.EsPendiente = true;
                                item.SeMovio = false;
                            }
                        }
                        else
                        {
                            item.Comentario = piezaMovOriginal.Comentario;
                            item.EnError = piezaMovOriginal.EnError;
                            item.EsPendiente = piezaMovOriginal.EsPendiente;
                            item.Orden = piezaMovOriginal.Orden;
                            item.SeMovio = piezaMovOriginal.SeMovio;
                        }


                        piezaMovOriginal = null;

                        //buscar el objeto
                        var itemTemp = db.MovimientoTempPiezas.Find(item.MovimientoTempID, item.PiezaID);
                        itemTemp.Comentario = item.Comentario;
                        itemTemp.EnError = item.EnError;
                        itemTemp.EsPendiente = item.EsPendiente;
                        itemTemp.Orden = item.Orden;
                        itemTemp.SeMovio = item.SeMovio;
                        itemTemp.FolioPieza = item.FolioPieza;

                        //------ Logica HISTORIAL

                        #region Generar el historial

                        //objeto del formulario
                        var objetoEdit = itemTemp;
                        //Objeto de la base de datos
                        var objetoDBEdit = db.MovimientoTempPiezas.Find(itemTemp.MovimientoTempID, itemTemp.PiezaID);
                        //tabla o clase a la que pertenece
                        var tablaNombreEdit = "MovimientoTempPieza";
                        //llave primaria del objeto
                        var llavePrimariaEdit = objetoDBEdit.MovimientoTempID + "," + objetoDBEdit.PiezaID;


                        //generar el historial
                        var historialLogEdit = HistorialLogica.EditarEntidad(
                            objetoEdit,
                            objetoDBEdit,
                            tablaNombreEdit,
                            llavePrimariaEdit,
                            User.UsuarioID,
                            db,
                            Motivo,
                            fechaEdicion
                            );

                        #endregion

                        #region Agrega el historial

                        if (historialLogEdit != null)
                        {
                            //Cambiar el estado a la entidad a modificada
                            db.Entry(objetoDBEdit).State = EntityState.Modified;

                            //Guardar el historial
                            db.HistorialLogs.Add(historialLogEdit);
                        }

                        #endregion

                        //------

                    }
                }

                #region Guardar el historial y la entidad

                //Guardar todo 
                db.SaveChanges();

                #endregion

                #endregion

                //Eliminar
                #region ListaEliminar

                db = new RecordFCSContext();
                //db.Configuration.LazyLoadingEnabled = false;

                foreach (var PiezaIDDel in listaGuidActual)
                {


                    var item = db.MovimientoTempPiezas.FirstOrDefault(a => a.PiezaID == PiezaIDDel && a.MovimientoTempID == movimientoTemp.MovimientoTempID);

                    bool esEliminable = false;
                    string Folio = "";
                    bool RegresarHistorial = false;

                    //saber si es eliminable y si se puede regresar el estado
                    if (item != null)
                    {
                        bool esUltimo = db.MovimientoTempPiezas.Where(a => a.PiezaID == item.PiezaID && a.Orden > item.Orden && !a.EnError).Count() > 0 ? false : true;
                        bool conPendientes = db.MovimientoTempPiezas.Where(a => a.PiezaID == item.PiezaID && a.EsPendiente && a.MovimientoTempID != movimientoTemp.MovimientoTempID && !a.EnError).Count() > 0 ? true : false;

                        Folio = item.Pieza.ImprimirFolio();

                        if (item.SeMovio)
                        {
                            if (esUltimo)
                            {
                                esEliminable = true;
                                RegresarHistorial = true;
                            }
                        }
                        else
                        {
                            if (item.EsPendiente)
                            {
                                esEliminable = true;
                            }
                        }
                    }

                    //es eliminable
                    if (esEliminable)
                    {
                        db.MovimientoTempPiezas.Remove(item);

                        //------ Logica HISTORIAL

                        #region Generar el historial

                        // Generar el historial
                        var historialLogDel =
                            HistorialLogica.EliminarEntidad(
                            item,
                            "MovimientoTempPieza",
                            item.MovimientoTempID + "," + item.PiezaID,
                            User.UsuarioID,
                            db,
                            fechaEdicion);

                        #endregion

                        #region Guardar el historial

                        //Guardar cambios si todo salio correcto
                        if (historialLogDel != null)
                        {
                            //Guardar el historial
                            db.HistorialLogs.Add(historialLogDel);

                            AlertaDanger("Se elimino la pieza [<b>" + Folio + "</b>]", true);

                        }


                        #endregion

                        //------

                        //Regresar el estado de la pieza
                        if (RegresarHistorial)
                        {
                            item = null;
                            var pieza = db.Piezas.Find(PiezaIDDel);
                            if (pieza != null)
                            {
                                if (pieza.UbicacionID == movimientoTemp.UbicacionDestinoID)
                                {
                                    pieza.UbicacionID = movimientoTemp.UbicacionOrigenID;

                                    //------ Logica HISTORIAL

                                    #region Generar el historial

                                    //objeto del formulario
                                    var objetoDelUbi = pieza;
                                    //Objeto de la base de datos
                                    var objetoDBDelUbi = db.Piezas.Find(pieza.PiezaID);
                                    //tabla o clase a la que pertenece
                                    var tablaNombreDelUbi = "Pieza";
                                    //llave primaria del objeto
                                    var llavePrimariaDelUbi = objetoDBDelUbi.PiezaID.ToString();

                                    //generar el historial
                                    var historialLogDelUbi = HistorialLogica.EditarEntidad(
                                        objetoDelUbi,
                                        objetoDBDelUbi,
                                        tablaNombreDelUbi,
                                        llavePrimariaDelUbi,
                                        User.UsuarioID,
                                        db,
                                        "Retornar Movimiento: " + movimientoTemp.Folio,
                                        fechaEdicion
                                        );

                                    #endregion

                                    #region Guardar el historial

                                    if (historialLogDelUbi != null)
                                    {
                                        //Cambiar el estado a la entidad a modificada
                                        db.Entry(objetoDBDelUbi).State = EntityState.Modified;


                                        //Guardar el historial
                                        db.HistorialLogs.Add(historialLogDelUbi);

                                    }

                                    #endregion

                                    //------


                                }
                            }
                        }
                    }
                }

                #region Guardar el historial

                //Guardar todo 
                db.SaveChanges();

                #endregion

                #endregion

                if (ForzarMovimiento)
                {
                    //Logica para forzar el movimiento 
                    db = new RecordFCSContext();

                    var mov = db.MovimientosTemp.Find(movimientoTemp.MovimientoTempID);

                    var EstadoActual = mov.EstadoMovimiento.Value;

                    //Solo acepta movimientos que no esten cancelados o que tengan retorno
                    if (mov.MovimientoTempPiezas.Where(a => !a.SeMovio && !a.EnError && a.EsPendiente).Count() > 0)
                    {
                        if (EstadoActual == EstadoMovimientoTemp.Concluido ||
                            EstadoActual == EstadoMovimientoTemp.Concluido_SinValidar ||
                            EstadoActual == EstadoMovimientoTemp.Procesando)
                        {
                            //Sirve para cambiar la ubicacion a las piezas

                            //Elegir el estado al cual pasara
                            //Ignorando la fecha
                            switch (EstadoActual)
                            {
                                case EstadoMovimientoTemp.Concluido_SinValidar:
                                    //Comprobar si el movimiento ya es valido
                                    if (mov.EsValido)
                                        EstadoActual = EstadoMovimientoTemp.Concluido;
                                    break;
                                case EstadoMovimientoTemp.Procesando:
                                    //Comprobar si el movimiento ya es valido
                                    if (mov.EsValido)
                                        EstadoActual = EstadoMovimientoTemp.Concluido;
                                    else
                                        EstadoActual = EstadoMovimientoTemp.Concluido_SinValidar;
                                    break;
                            }

                            mov.EstadoMovimiento = EstadoActual;

                            EjecutarMovimiento(mov, EstadoActual, fechaEdicion);

                        }

                    }

                }

                return Json(new { success = true, url = Url.Action("Detalles", new { id = movimientoTemp.MovimientoTempID }) });
            }


            //Si Error
            #region Logica si ocurre un error

            var listaUbicaciones = db.Ubicaciones.Where(a => a.Status).Select(a => new { a.Nombre, a.UbicacionID }).OrderBy(a => a.Nombre);


            //Ubicacion destino no puede editarse si se encuentra por lo menos una Pieza Procesada
            ViewBag.UbiDestinoEnabled = movimientoTemp.MovimientoTempPiezas.Where(a => a.SeMovio).Count() == 0 ? true : false;

            ViewBag.UbicacionDestinoID = new SelectList(listaUbicaciones, "UbicacionID", "Nombre", movimientoTemp.UbicacionDestinoID);


            #region Validar navegacion de movimientos

            // 1 2 3 4 [5] 6 7 8 9 10
            // [1] 2 3 4 5 6 7 8 9 10
            // 1 2 3 4 5 6 7 8 9 9 [10]

            MovimientoTemp movTemp = null;
            movTemp = db.MovimientosTemp.Where(a => a.Folio < movimientoTemp.Folio).OrderByDescending(a => a.Folio).FirstOrDefault();
            ViewBag.MovAnterior = movTemp == null ? Guid.Empty : movTemp.MovimientoTempID;

            movTemp = db.MovimientosTemp.Where(a => a.Folio > movimientoTemp.Folio).OrderBy(a => a.Folio).FirstOrDefault();
            ViewBag.MovSiguiente = movTemp == null ? Guid.Empty : movTemp.MovimientoTempID;

            #endregion

            #endregion

            return View(movimientoTemp);
        }

        #endregion


        #region Eliminar (Ya no se utiliza)

        //// GET: MovimientoTemp/Delete/5
        //[CustomAuthorize(permiso = "movDel")]
        //public ActionResult Delete(Guid? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    MovimientoTemp movimientoTemp = db.MovimientosTemp.Find(id);
        //    if (movimientoTemp == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(movimientoTemp);
        //}

        //// POST: MovimientoTemp/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //[CustomAuthorize(permiso = "movDel")]
        //public ActionResult DeleteConfirmed(Guid id)
        //{
        //    MovimientoTemp movimientoTemp = db.MovimientosTemp.Find(id);
        //    db.MovimientosTemp.Remove(movimientoTemp);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        #endregion


        #region Ejecutar Movimiento

        public bool EjecutarMovimiento(Guid? id, EstadoMovimientoTemp Estado, DateTime? Fecha = null)
        {
            db = new RecordFCSContext();

            bool bandera = false;

            Fecha = Fecha ?? DateTime.Now;
            try
            {
                if (id != null)
                {
                    MovimientoTemp mov = db.MovimientosTemp.Find(id);

                    if (mov != null)
                        if (mov.MovimientoTempPiezas.Where(a => !a.SeMovio && a.EsPendiente && !a.EnError).Count() > 0)
                            EjecutarMovimiento(mov, Estado, Fecha.Value);
                }
            }
            catch (Exception)
            {
                bandera = false;
            }

            return bandera;
        }

        private bool EjecutarMovimiento(MovimientoTemp mov, EstadoMovimientoTemp Estado, DateTime Fecha, string Motivo = null)
        {

            bool bandera = false;

            try
            {
                db = new RecordFCSContext();

                Motivo = Motivo ?? "Ejecución del movimiento.";
                //Revalidar las piezas del movimiento que fueron validas y que no se hayan movido y sean pendientes
                int contadorPMovidas = 0;

                foreach (var PiezaID in mov.MovimientoTempPiezas.Where(a => !a.EnError && !a.SeMovio && a.EsPendiente).Select(a => a.PiezaID).ToList())
                {


                    #region Buscar la pieza con su movimiento

                    //Buscar la pieza y la pieza en el movimiento
                    var pieza = db.Piezas.Find(PiezaID);
                    var piezaEnMovReal = pieza.MovimientoTempPiezas.FirstOrDefault(a => a.MovimientoTempID == mov.MovimientoTempID);

                    #endregion

                    #region Declaracion y asignaciones

                    piezaEnMovReal.EnError = false;
                    piezaEnMovReal.EsPendiente = true;
                    piezaEnMovReal.SeMovio = false;

                    string comentario = "";

                    #endregion

                    #region Validacion 1: Asignada en movimientos

                    #region Buscar donde sea Pendiente y sin errores

                    //lista de movimientos ID
                    //Buscar la pieza en todos los movimientos excepto este, donde sea pendiente y no tenga errrores.

                    List<int> listaMov_ID =
                        db.MovimientoTempPiezas
                        .Where(a =>
                            a.PiezaID == PiezaID &&
                            a.MovimientoTempID != mov.MovimientoTempID &&
                            a.EsPendiente && !a.EnError
                            )
                            .Select(a => a.MovimientoTemp.Folio).ToList();

                    #endregion

                    #region Buscar en Concluido sin validar

                    listaMov_ID.AddRange(
                        db.MovimientoTempPiezas.Where(a => a.PiezaID == PiezaID && a.MovimientoTempID != mov.MovimientoTempID &&
                        a.MovimientoTemp.EstadoMovimiento == EstadoMovimientoTemp.Concluido_SinValidar &&
                        a.SeMovio).Select(a => a.MovimientoTemp.Folio).ToList()
                        );

                    #endregion

                    #region Buscar en Cancelado

                    listaMov_ID.AddRange(
                        db.MovimientoTempPiezas.Where(a => a.PiezaID == PiezaID && a.MovimientoTempID != mov.MovimientoTempID &&
                        a.MovimientoTemp.EstadoMovimiento == EstadoMovimientoTemp.Cancelado &&
                        a.SeMovio).Select(a => a.MovimientoTemp.Folio).ToList()
                        );

                    #endregion

                    #region Buscar en Retornado

                    listaMov_ID.AddRange(
                        db.MovimientoTempPiezas.Where(a => a.PiezaID == PiezaID && a.MovimientoTempID != mov.MovimientoTempID &&
                        a.MovimientoTemp.EstadoMovimiento == EstadoMovimientoTemp.Retornado &&
                        a.SeMovio).Select(a => a.MovimientoTemp.Folio).ToList()
                        );

                    #endregion

                    //Si existe con estas condiciones entonces no es disponible
                    foreach (var Folio in listaMov_ID)
                        comentario += " [" + Folio + "]";

                    //Comprobar que no haya estado en algun movimiento
                    if (!string.IsNullOrWhiteSpace(comentario))
                    {
                        piezaEnMovReal.Comentario = "Asignada en:" + comentario + ". ";
                        piezaEnMovReal.EnError = true;
                    }

                    #endregion


                    #region Validacion 2: Ubicacion

                    //validar que la ubicacion de la pieza sea la misma que este movimiento

                    //puede haber piezas que UbicacionID sea null y en este caso se concidera valida.
                    if (pieza.UbicacionID != null)
                    {
                        //Validar que la ubicacion Origen sea igual a la de la pieza
                        if (mov.UbicacionOrigenID != pieza.UbicacionID)
                        {
                            piezaEnMovReal.Comentario += "No comparte la misma ubicación origen. ";
                            piezaEnMovReal.EnError = true;
                        }
                    }



                    #endregion

                    #region Validacion 3: En ultimo movimiento

                    if (pieza.MovimientoTempPiezas.Where(a => a.Orden > piezaEnMovReal.Orden && !a.EnError).Count() > 0)
                    {
                        piezaEnMovReal.Comentario += "No esta en su último movimiento. ";
                        piezaEnMovReal.EnError = true;
                    }

                    #endregion

                    //Sin errores entoces proceder a realizar el cambio de ubicacion

                    #region Cambio de Ubicacion

                    if (!piezaEnMovReal.EnError)
                    {
                        //Sin errores

                        #region Asignar un orden al movimiento de la pieza

                        if (piezaEnMovReal.Orden == 0)
                        {
                            var ordenActual = pieza.MovimientoTempPiezas.Count > 0 ? pieza.MovimientoTempPiezas.OrderByDescending(a => a.Orden).FirstOrDefault().Orden : 0;
                            piezaEnMovReal.Orden = piezaEnMovReal.EnError ? 0 : ordenActual + 1;
                        }

                        #endregion

                        //Ejecutar el cambio de ubicacion
                        pieza.UbicacionID = mov.UbicacionDestinoID;

                        //------ Logica HISTORIAL

                        #region Generar el historial

                        //objeto del formulario
                        var objeto = pieza;
                        //Objeto de la base de datos
                        var objetoDB = db.Piezas.Find(pieza.PiezaID);
                        //tabla o clase a la que pertenece
                        var tablaNombre = "Pieza";
                        //llave primaria del objeto
                        var llavePrimaria = objetoDB.PiezaID.ToString();

                        //generar el historial
                        var historialLog = HistorialLogica.EditarEntidad(
                            objeto,
                            objetoDB,
                            tablaNombre,
                            llavePrimaria,
                            User.UsuarioID,
                            db,
                            Motivo + "[Forzado]",
                            Fecha
                            );

                        #endregion

                        #region Guardar el historial

                        if (historialLog != null)
                        {
                            //Cambiar el estado a la entidad a modificada
                            db.Entry(objetoDB).State = EntityState.Modified;
                            //Guardamos la entidad modificada
                            db.SaveChanges();

                            //Guardar el historial
                            db.HistorialLogs.Add(historialLog);
                            db.SaveChanges();

                            contadorPMovidas++;

                            piezaEnMovReal.SeMovio = true;
                            piezaEnMovReal.EsPendiente = false;
                            piezaEnMovReal.EnError = false;
                            piezaEnMovReal.Comentario = "";
                        }
                        else
                        {
                            throw new Exception();
                        }
                        #endregion

                        //------

                    }
                    else
                    {
                        //Con errores
                        piezaEnMovReal.Orden = 0;
                    }

                    pieza = null;

                    #endregion


                    #region Asignacion de nuevos valores

                    //buscar el objeto
                    var itemTemp = db.MovimientoTempPiezas.Find(mov.MovimientoTempID, PiezaID);
                    itemTemp.Comentario = piezaEnMovReal.Comentario;
                    itemTemp.EnError = piezaEnMovReal.EnError;
                    itemTemp.EsPendiente = piezaEnMovReal.EsPendiente;
                    itemTemp.Orden = piezaEnMovReal.Orden;
                    itemTemp.SeMovio = piezaEnMovReal.SeMovio;

                    piezaEnMovReal = null;


                    #endregion


                    //------ Logica HISTORIAL

                    #region Generar el historial

                    //objeto del formulario
                    var objeto2 = itemTemp;
                    //Objeto de la base de datos
                    var objetoDB2 = db.MovimientoTempPiezas.Find(mov.MovimientoTempID, PiezaID);
                    //tabla o clase a la que pertenece
                    var tablaNombre2 = "MovimientoTempPieza";
                    //llave primaria del objeto
                    var llavePrimaria2 = objetoDB2.MovimientoTempID + "," + objetoDB2.PiezaID;

                    //generar el historial
                    var historialLog2 = HistorialLogica.EditarEntidad(
                        objeto2,
                        objetoDB2,
                        tablaNombre2,
                        llavePrimaria2,
                        User.UsuarioID,
                        db,
                        Motivo + "[Forzado]",
                        Fecha
                        );

                    #endregion

                    #region Guardar el historial

                    if (historialLog2 != null)
                    {
                        //Cambiar el estado a la entidad a modificada
                        db.Entry(objetoDB2).State = EntityState.Modified;
                        //Guardamos la entidad modificada
                        db.SaveChanges();

                        //Guardar el historial
                        db.HistorialLogs.Add(historialLog2);
                        db.SaveChanges();

                    }
                    else
                    {
                        throw new Exception();
                    }
                    #endregion

                    //------

                }


                //Cambio del estatus
                //Solo si se actualizo la ubicacion de por lo menos 1 pieza

                if (contadorPMovidas > 0)
                {
                    //saber cual sera el estatus Concluido ó Concluido_SinValidar
                    if (mov.EsValido)
                        mov.EstadoMovimiento = EstadoMovimientoTemp.Concluido;
                    else
                        mov.EstadoMovimiento = EstadoMovimientoTemp.Concluido_SinValidar;
                }



                //------ Logica HISTORIAL

                #region Generar el historial

                //objeto del formulario
                var objeto3 = mov;
                //Objeto de la base de datos
                var objetoDB3 = db.MovimientosTemp.Find(mov.MovimientoTempID);
                //tabla o clase a la que pertenece
                var tablaNombre3 = "MovimientoTemp";
                //llave primaria del objeto
                var llavePrimaria3 = objetoDB3.MovimientoTempID.ToString();

                //generar el historial
                var historialLog3 = HistorialLogica.EditarEntidad(
                    objeto3,
                    objetoDB3,
                    tablaNombre3,
                    llavePrimaria3,
                    User.UsuarioID,
                    db,
                    Motivo + "[Forzado]",
                    Fecha
                    );

                #endregion

                #region Guardar el historial

                if (historialLog3 != null)
                {
                    //Cambiar el estado a la entidad a modificada
                    db.Entry(objetoDB3).State = EntityState.Modified;
                    //Guardamos la entidad modificada
                    db.SaveChanges();

                    //Guardar el historial
                    db.HistorialLogs.Add(historialLog3);
                    db.SaveChanges();
                }
                else
                {
                    throw new Exception();
                }
                #endregion

                //------



            }
            catch (Exception)
            {
                bandera = false;
            }


            return bandera;
        }

        #endregion


        #region Cancelar Movimiento
        [CustomAuthorize(permiso = "movCancel")]
        public ActionResult CancelarMovimiento(Guid? id)
        {
            db = new RecordFCSContext();

            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            MovimientoTemp mov = db.MovimientosTemp.Find(id);

            if (mov == null) return HttpNotFound();

            //Validar que el movimiento se pueda cancelar
            //todas las piezas que estan en estado EsMovido se puedan mover

            bool Executar = false;

            int totalMovidas = mov.MovimientoTempPiezas.Where(a => a.SeMovio).Count();
            int totalMovidasSonUltimas = 0;

            List<string> listaPiezasEnError = new List<string>();

            #region Validacion 3: En ultimo movimiento

            foreach (var piezaEnMovReal in mov.MovimientoTempPiezas.Where(a => a.SeMovio && a.Pieza.UbicacionID == mov.UbicacionDestinoID).ToList())
            {
                if ((piezaEnMovReal.Pieza.MovimientoTempPiezas.Where(a => a.Orden > piezaEnMovReal.Orden && !a.EnError).Count() > 0))
                {
                    totalMovidasSonUltimas = 0;
                    listaPiezasEnError.Add(piezaEnMovReal.Pieza.ImprimirFolio());
                }
                else
                {
                    totalMovidasSonUltimas++;
                }

            }


            #endregion


            Executar = totalMovidas == totalMovidasSonUltimas ? true : false;


            ViewBag.Executar = Executar;
            ViewBag.listaPiezasEnError = listaPiezasEnError;

            return PartialView("_Cancelar", mov);
        }


        [HttpPost, ActionName("CancelarMovimiento")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permiso = "movCancel")]
        public ActionResult CancelarMovimientoConfirmado(Guid MovimientoTempID, bool Executar)
        {
            db = new RecordFCSContext();

            bool bandera = false;

            if (Executar)
            {
                //Buscar el movimiento

                MovimientoTemp mov = db.MovimientosTemp.Find(MovimientoTempID);

                if (mov != null)
                {
                    bandera = Cancelar(mov, Executar);
                }

            }

            var url = Url.Action("Detalles", "MovimientoTemp", new { id = MovimientoTempID });

            return Json(new { success = bandera, url = url });
        }

        #region Cancelar Metodo

        private bool Cancelar(MovimientoTemp mov, bool Executar)
        {
            db = new RecordFCSContext();

            bool bandera = false;

            DateTime Fecha = DateTime.Now;
            mov.FechaUltimaEjecucion = Fecha;

            string Motivo = "Cancelar movimiento.";

            try
            {
                bandera = true;

                //Validar que el movimiento se pueda cancelar, comprobando las piezas en estado SeMovio sean las ultimas en su mov
                #region Validacion 3: En ultimo movimiento

                int totalMovidas = mov.MovimientoTempPiezas.Where(a => a.SeMovio).Count();
                int totalMovidasSonUltimas = 0;

                foreach (var piezaEnMovReal in mov.MovimientoTempPiezas.Where(a => a.SeMovio && a.Pieza.UbicacionID == mov.UbicacionDestinoID).ToList())
                    if ((piezaEnMovReal.Pieza.MovimientoTempPiezas.Where(a => a.Orden > piezaEnMovReal.Orden && !a.EnError).Count() > 0))
                        totalMovidasSonUltimas = 0;
                    else
                        totalMovidasSonUltimas++;

                if (totalMovidas != totalMovidasSonUltimas)
                    throw new Exception("No se pueden cancelar las piezas");

                #endregion


                //Revertir todas las piezas
                foreach (var PiezaIDDel in mov.MovimientoTempPiezas.Select(a => a.PiezaID).ToList())
                {
                    bandera = false;
                    //la pieza en el movimiento
                    var item = db.MovimientoTempPiezas.FirstOrDefault(a => a.PiezaID == PiezaIDDel && a.MovimientoTempID == mov.MovimientoTempID);

                    bool RegresarHistorial = false;

                    //saber si es eliminable y si se puede regresar el estado
                    if (item != null)
                    {
                        if (item.SeMovio)
                        {
                            bool esUltimo = item.Pieza.MovimientoTempPiezas.Where(a => a.Orden > item.Orden && !a.EnError).Count() > 0 ? false : true;
                            bool conPendientes = item.Pieza.MovimientoTempPiezas.Where(a => a.EsPendiente && a.MovimientoTempID != mov.MovimientoTempID && !a.EnError).Count() > 0 ? true : false;

                            if (esUltimo)
                                RegresarHistorial = true;
                        }
                        else
                        {
                            RegresarHistorial = true;
                        }
                    }


                    if (RegresarHistorial)
                    {
                        //Regresar la ubicacion original
                        if (item.SeMovio)
                        {
                            var pieza = db.Piezas.Find(PiezaIDDel);

                            RegresarHistorial = false;

                            if (pieza != null)
                            {
                                if (pieza.UbicacionID == mov.UbicacionDestinoID)
                                {
                                    pieza.UbicacionID = mov.UbicacionOrigenID;

                                    //------ Logica HISTORIAL

                                    #region Generar el historial

                                    //objeto del formulario
                                    var objetoDelUbi = pieza;
                                    //Objeto de la base de datos
                                    var objetoDBDelUbi = db.Piezas.Find(pieza.PiezaID);
                                    //tabla o clase a la que pertenece
                                    var tablaNombreDelUbi = "Pieza";
                                    //llave primaria del objeto
                                    var llavePrimariaDelUbi = objetoDBDelUbi.PiezaID.ToString();

                                    //generar el historial
                                    var historialLogDelUbi = HistorialLogica.EditarEntidad(
                                        objetoDelUbi,
                                        objetoDBDelUbi,
                                        tablaNombreDelUbi,
                                        llavePrimariaDelUbi,
                                        User.UsuarioID,
                                        db,
                                        "Cancelar Movimiento: " + mov.Folio,
                                        mov.FechaUltimaEjecucion
                                        );

                                    #endregion

                                    #region Guardar el historial

                                    if (historialLogDelUbi != null)
                                    {
                                        //Cambiar el estado a la entidad a modificada
                                        db.Entry(objetoDBDelUbi).State = EntityState.Modified;
                                        //Guardamos la entidad modificada
                                        db.SaveChanges();

                                        //Guardar el historial
                                        db.HistorialLogs.Add(historialLogDelUbi);
                                        db.SaveChanges();

                                        RegresarHistorial = true;
                                    }


                                    #endregion

                                    //------


                                }

                            }

                        }


                        if (RegresarHistorial)
                        {

                            item.SeMovio = false;
                            item.EsPendiente = true;
                            item.EnError = true;
                            item.Comentario = "Validar de nuevo [Movimiento Cancelado].";


                            //------ Logica HISTORIAL

                            #region Generar el historial

                            //objeto del formulario
                            var objetoEdit = item;
                            //Objeto de la base de datos
                            var objetoDBEdit = db.MovimientoTempPiezas.Find(item.MovimientoTempID, item.PiezaID);
                            //tabla o clase a la que pertenece
                            var tablaNombreEdit = "MovimientoTempPieza";
                            //llave primaria del objeto
                            var llavePrimariaEdit = objetoDBEdit.MovimientoTempID + "," + objetoDBEdit.PiezaID;


                            //generar el historial
                            var historialLogEdit = HistorialLogica.EditarEntidad(
                                objetoEdit,
                                objetoDBEdit,
                                tablaNombreEdit,
                                llavePrimariaEdit,
                                User.UsuarioID,
                                db,
                                Motivo,
                                Fecha
                                );

                            #endregion

                            #region Guardar el historial

                            if (historialLogEdit != null)
                            {
                                //Cambiar el estado a la entidad a modificada
                                db.Entry(objetoDBEdit).State = EntityState.Modified;
                                //Guardamos la entidad modificada
                                db.SaveChanges();

                                //Guardar el historial
                                db.HistorialLogs.Add(historialLogEdit);
                                db.SaveChanges();

                                bandera = true;
                            }
                            else
                            {
                                bandera = true;
                            }

                            #endregion

                            //------


                        }


                    }


                }


                if (bandera)
                {
                    //Como todas las piezas que tenia se revertieron 
                    //Regresar el Estatus

                    mov.EstadoMovimiento = EstadoMovimientoTemp.Cancelado;


                    //------ Logica HISTORIAL

                    #region Generar el historial

                    //objeto del formulario
                    var objeto3 = mov;
                    //Objeto de la base de datos
                    var objetoDB3 = db.MovimientosTemp.Find(mov.MovimientoTempID);
                    //tabla o clase a la que pertenece
                    var tablaNombre3 = "MovimientoTemp";
                    //llave primaria del objeto
                    var llavePrimaria3 = objetoDB3.MovimientoTempID.ToString();

                    //generar el historial
                    var historialLog3 = HistorialLogica.EditarEntidad(
                        objeto3,
                        objetoDB3,
                        tablaNombre3,
                        llavePrimaria3,
                        User.UsuarioID,
                        db,
                        Motivo,
                        Fecha
                        );

                    #endregion

                    #region Guardar el historial

                    if (historialLog3 != null)
                    {
                        //Cambiar el estado a la entidad a modificada
                        db.Entry(objetoDB3).State = EntityState.Modified;
                        //Guardamos la entidad modificada
                        db.SaveChanges();

                        //Guardar el historial
                        db.HistorialLogs.Add(historialLog3);
                        db.SaveChanges();
                    }
                    else
                    {
                        throw new Exception();
                    }
                    #endregion

                    //------

                }
            }
            catch (Exception)
            {
                bandera = false;
            }


            if (bandera)
                AlertaDefault("Se Cancelo el moviento");
            else
                AlertaDefault("No se pudo cancelar el movimiento");

            return bandera;
        }

        #endregion

        #endregion


        #region Revertir Movimiento

        [CustomAuthorize(permiso = "movRevertir")]
        public ActionResult RevertirMovimiento(Guid? id)
        {
            db = new RecordFCSContext();

            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            MovimientoTemp mov = db.MovimientosTemp.Find(id);

            if (mov == null) return HttpNotFound();

            //Validar que el movimiento se pueda revertir
            //no debe de contener ninguna pieza en estado SeMovio

            bool Executar = mov.MovimientoTempPiezas.Where(a => a.SeMovio).Count() > 0 ? false : true;

            ViewBag.Executar = Executar;

            return PartialView("_Revertir", mov);
        }


        [HttpPost, ActionName("RevertirMovimiento")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permiso = "movRevertir")]
        public ActionResult RevertirMovimientoConfirmado(Guid MovimientoTempID, bool Executar)
        {
            db = new RecordFCSContext();

            bool bandera = false;

            if (Executar)
            {
                try
                {
                    //Buscar el movimiento

                    MovimientoTemp mov = db.MovimientosTemp.Find(MovimientoTempID);

                    if (mov != null && Executar)
                    {
                        var Fecha = DateTime.Now;
                        var Motivo = "Revertir movimiento.";
                        //Cambiar el estado del movimiento
                        mov.EstadoMovimiento = EstadoMovimientoTemp.Procesando;
                        mov.FechaUltimaEjecucion = Fecha;


                        //------ Logica HISTORIAL

                        #region Generar el historial

                        //objeto del formulario
                        var objeto3 = mov;
                        //Objeto de la base de datos
                        var objetoDB3 = db.MovimientosTemp.Find(mov.MovimientoTempID);
                        //tabla o clase a la que pertenece
                        var tablaNombre3 = "MovimientoTemp";
                        //llave primaria del objeto
                        var llavePrimaria3 = objetoDB3.MovimientoTempID.ToString();

                        //generar el historial
                        var historialLog3 = HistorialLogica.EditarEntidad(
                            objeto3,
                            objetoDB3,
                            tablaNombre3,
                            llavePrimaria3,
                            User.UsuarioID,
                            db,
                            Motivo,
                            Fecha
                            );

                        #endregion

                        #region Guardar el historial

                        if (historialLog3 != null)
                        {
                            //Cambiar el estado a la entidad a modificada
                            db.Entry(objetoDB3).State = EntityState.Modified;
                            //Guardamos la entidad modificada
                            db.SaveChanges();

                            //Guardar el historial
                            db.HistorialLogs.Add(historialLog3);
                            db.SaveChanges();
                            bandera = true;

                        }
                        else
                        {
                            throw new Exception();
                        }
                        #endregion

                        //------

                    }
                    else
                    {
                        bandera = false;
                    }
                }
                catch (Exception)
                {

                    bandera = false;
                }
            }


            if (bandera)
                AlertaDefault("Se Revertio el moviento a " + EstadoMovimientoTemp.Procesando);
            else
                AlertaDefault("No se pudo revertir el movimiento");

            var url = Url.Action("Detalles", "MovimientoTemp", new { id = MovimientoTempID });

            return Json(new { success = bandera, url = url });
        }


        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


    }
}
