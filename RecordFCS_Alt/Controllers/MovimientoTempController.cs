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

namespace RecordFCS_Alt.Controllers
{
    public class MovimientoTempController : BaseController
    {
        private RecordFCSContext db = new RecordFCSContext();

        // GET: MovimientoTemp
        [CustomAuthorize(permiso = "")]
        public ActionResult Index(MovimientoTemp MovTemp = null)
        {

            var listaLetras = db.LetraFolios.Select(a => new { a.LetraFolioID, Nombre = a.Nombre, a.Status }).Where(a => a.Status).OrderBy(a => a.Nombre);
            ViewBag.LetraFolioID = new SelectList(listaLetras, "LetraFolioID", "Nombre", listaLetras.FirstOrDefault().LetraFolioID);


            var listaUbicaciones = db.Ubicaciones.Where(a => a.Status).Select(a => new { a.Nombre, a.UbicacionID }).OrderBy(a => a.Nombre);

            ViewBag.UbicacionDestinoID = new SelectList(listaUbicaciones, "UbicacionID", "Nombre");
            ViewBag.UbicacionOrigenID = new SelectList(listaUbicaciones, "UbicacionID", "Nombre");

            ViewBag.TipoMovimientoID = new SelectList(db.TipoMovimientos.Where(a => a.Status).OrderBy(a => a.Nombre), "TipoMovimientoID", "Nombre", MovTemp.TipoMovimientoID);

            if (MovTemp == null)
            {


                var mov = new MovimientoTemp()
                {
                    TieneExposicion = false
                };

                return View(mov);
            }

            return View(MovTemp);
        }

        [CustomAuthorize(permiso = "")]
        public ActionResult BuscarMovimientos(string FolioObraPieza, int? FolioMovimiento, string FechaInicial, string FechaFinal, Guid? UbicacionOrigenID, Guid? UbicacionDestinoID, int? pagina = null)
        {
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
                if (!string.IsNullOrWhiteSpace(FolioObraPieza))
                {
                    //realizar la busqueda exclusica del folio de la obra o pieza
                    listaMovimientos = listaMovimientos.Where(a => a.MovimientoTempPiezas.Any(b => (b.Pieza.Obra.LetraFolio.Nombre + b.Pieza.Obra.NumeroFolio) == FolioObraPieza || (b.Pieza.Obra.LetraFolio.Nombre + b.Pieza.Obra.NumeroFolio + b.Pieza.SubFolio) == FolioObraPieza));
                }
                else
                {
                    //Buscar por fecha Iniciar
                    if (!string.IsNullOrWhiteSpace(FechaInicial))
                    {
                        //Fecha mayor
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

                    if (UbicacionOrigenID != null)
                        listaMovimientos = listaMovimientos.Where(a => a.UbicacionOrigenID == UbicacionOrigenID);

                    if (UbicacionDestinoID != null)
                        listaMovimientos = listaMovimientos.Where(a => a.UbicacionDestinoID == UbicacionDestinoID);
                }
            }

            listaMovimientosEnPagina = listaMovimientos.OrderBy(a => a.FechaSalida).Select(x => x).ToList().ToPagedList(pagIndex, pagTamano);

            return PartialView("_Lista", listaMovimientosEnPagina);
        }


        // GET: MovimientoTemp/Detalles/5
        [CustomAuthorize(permiso = "")]
        public ActionResult Detalles(Guid? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            MovimientoTemp mov = db.MovimientosTemp.Find(id);
            if (mov == null) return HttpNotFound();

            // 1 2 3 4 [5] 6 7 8 9 10
            // [1] 2 3 4 5 6 7 8 9 10
            // 1 2 3 4 5 6 7 8 9 9 [10]

            MovimientoTemp movTemp = null;
            movTemp = db.MovimientosTemp.Where(a => a.Folio < mov.Folio).OrderByDescending(a=> a.Folio).FirstOrDefault();
            ViewBag.MovAnterior = movTemp == null ? Guid.Empty : movTemp.MovimientoTempID;

            movTemp = db.MovimientosTemp.Where(a => a.Folio > mov.Folio).OrderBy(a => a.Folio).FirstOrDefault();
            ViewBag.MovSiguiente = movTemp == null ? Guid.Empty : movTemp.MovimientoTempID;


            TipoArchivo tipoArchivo = db.TipoArchivos.FirstOrDefault(a => a.Temp == "imagen_clave");
            var TipoAttTitulo = db.TipoAtributos.FirstOrDefault(a => a.Temp == "titulo");

            var lista = new List<Item_MovPieza>();

            foreach (var item in mov.MovimientoTempPiezas.OrderBy(a => a.Pieza.Obra.LetraFolio.Nombre).ThenBy(a => a.Pieza.Obra.NumeroFolio).ThenBy(a => a.Pieza.SubFolio).ToList())
            {
                var obj = new Item_MovPieza();

                obj.ObraID = item.Pieza.ObraID;
                obj.FolioObra = item.Pieza.Obra.LetraFolio.Nombre + item.Pieza.Obra.NumeroFolio;
                obj.PiezaID = item.PiezaID;
                obj.FolioPieza = item.Pieza.ImprimirFolio();
                obj.UbicacionID = item.Pieza.UbicacionID;
                //var imagen = item.ArchivosPiezas.Where(b => b.Status && b.TipoArchivoID == tipoArchivo.TipoArchivoID && b.MostrarArchivos.Any(c => c.TipoMostrarArchivoID == tipoMostrarArchivo.TipoMostrarArchivoID && c.Status)).OrderBy(b => b.Orden).FirstOrDefault();
                //obj.Imagen = imagen == null ? "" : imagen.RutaThumb;
                var autor = item.Pieza.AutorPiezas.OrderBy(b => b.Orden).FirstOrDefault(b => b.esPrincipal && b.Status);
                obj.Autor = autor == null ? "Sin autor" : autor.Autor == null ? "Sin autor" : autor.Autor.Seudonimo + " " + autor.Autor.Nombre + " " + autor.Autor.ApellidoPaterno + " " + autor.Autor.ApellidoMaterno; var titulo = item.Pieza.AtributoPiezas.FirstOrDefault(b => b.Atributo.TipoAtributoID == TipoAttTitulo.TipoAtributoID);
                obj.Titulo = titulo == null ? "Sin titulo" : titulo.Valor;
                //obj.TotalPiezas = item.PiezasHijas.Count();
                obj.EnError = item.EnError;
                obj.EsPendiente = item.EsPendiente;
                obj.SeMovio = item.SeMovio;
                obj.ExisteEnMov = true;
                obj.Comentario = item.Comentario;
                obj.EsUltimo = item.Pieza.MovimientoTempPiezas.Where(a => a.Orden > item.Orden && !a.EnError).Count() > 0 ? false : true;

                lista.Add(obj);
            }

            mov.MovimientoTempPiezas = null;
            ViewData["ListaPiezasMov_" + id] = lista;

            return View(mov);
        }

        // GET: MovimientoTemp/Detalles/5
        //[CustomAuthorize(permiso = "")]
        public ActionResult ImprimirBoletin(Guid? id)
        {
            string NombreArchivo = "Boletin_";

            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            MovimientoTemp mov = db.MovimientosTemp.Find(id);
            if (mov == null) return HttpNotFound();

            //Movimiento con piezas aceptadas
            mov.MovimientoTempPiezas = mov.MovimientoTempPiezas.Where(a => a.SeMovio && !a.EnError).OrderBy(a => a.Pieza.Obra.LetraFolio.Nombre).ThenBy(a => a.Pieza.Obra.NumeroFolio).ThenBy(a => a.Pieza.SubFolio).ToList();


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


        // GET: MovimientoTemp/Create
        [CustomAuthorize(permiso = "")]
        public ActionResult Crear(Guid? TipoMovimientoID, bool TieneExposicion)
        {
            var tipoMov = db.TipoMovimientos.Find(TipoMovimientoID);

            if (tipoMov == null)
            {
                return HttpNotFound();
            }

            var mov = new MovimientoTemp()
            {
                TieneExposicion = TieneExposicion,
                TipoMovimientoID = tipoMov.TipoMovimientoID,
                TipoMovimiento = tipoMov,
                EstadoMovimiento = EstadoMovimientoTemp.Procesando,
                //Folio = db.MovimientosTemp.Select(a => a.Folio).OrderBy(a => a).SingleOrDefault() + 1

            };
            mov.Folio = db.MovimientosTemp.OrderByDescending(a => a.Folio).FirstOrDefault().Folio + 1;


            var listaUbicaciones = db.Ubicaciones.Where(a => a.Status).Select(a => new { a.Nombre, a.UbicacionID }).OrderBy(a => a.Nombre);

            ViewBag.UbicacionDestinoID = new SelectList(listaUbicaciones, "UbicacionID", "Nombre");
            ViewBag.UbicacionOrigenID = new SelectList(listaUbicaciones, "UbicacionID", "Nombre");

            return View(mov);
        }


        // POST: MovimientoTemp/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permiso = "")]
        public ActionResult Crear(MovimientoTemp movimientoTemp)
        {

            if (ModelState.IsValid)
            {
                movimientoTemp.Folio = db.MovimientosTemp.OrderByDescending(a => a.Folio).FirstOrDefault().Folio + 1;
                movimientoTemp.EstadoMovimiento = EstadoMovimientoTemp.Procesando;
                movimientoTemp.UsuarioID = User.UsuarioID;
                movimientoTemp.MovimientoTempID = Guid.NewGuid();
                db.MovimientosTemp.Add(movimientoTemp);
                db.SaveChanges();

                AlertaSuccess($"Se registro el movimiento: <b>{movimientoTemp.Folio}</b>", true);

                return RedirectToAction("Detalles", new { id = movimientoTemp.MovimientoTempID });
            }

            var tipoMov = db.TipoMovimientos.Find(movimientoTemp.TipoMovimientoID);

            movimientoTemp.TipoMovimiento = tipoMov;
            movimientoTemp.EstadoMovimiento = EstadoMovimientoTemp.Procesando;


            var listaUbicaciones = db.Ubicaciones.Where(a => a.Status).Select(a => new { a.Nombre, a.UbicacionID }).OrderBy(a => a.Nombre);

            ViewBag.UbicacionDestinoID = new SelectList(listaUbicaciones, "UbicacionID", "Nombre");
            ViewBag.UbicacionOrigenID = new SelectList(listaUbicaciones, "UbicacionID", "Nombre");

            return View(movimientoTemp);
        }

        // GET: MovimientoTemp/Edit/5
        [CustomAuthorize(permiso = "")]
        public ActionResult Editar(Guid? id, bool EnExpo = false)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MovimientoTemp movimientoTemp = db.MovimientosTemp.Find(id);
            if (movimientoTemp == null)
            {
                return HttpNotFound();
            }

            if (!movimientoTemp.TieneExposicion && EnExpo)
                movimientoTemp.TieneExposicion = EnExpo;


            var listaUbicaciones = db.Ubicaciones.Where(a => a.Status).Select(a => new { a.Nombre, a.UbicacionID }).OrderBy(a => a.Nombre);

            ViewBag.UbicacionDestinoID = new SelectList(listaUbicaciones, "UbicacionID", "Nombre", movimientoTemp.UbicacionDestinoID);


            return View(movimientoTemp);
        }

        // POST: MovimientoTemp/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permiso = "")]
        public ActionResult Editar(MovimientoTemp movimientoTemp, List<Item_MovPieza> ListaPiezas)
        {
            //cambiar el estado del movimiento dependiendo la fecha y hora
            if (movimientoTemp.EstadoMovimiento != EstadoMovimientoTemp.Cancelado)
            {
                if (movimientoTemp.FechaSalida < DateTime.Now)
                {
                    movimientoTemp.EstadoMovimiento = EstadoMovimientoTemp.Concluido;
                }
                else
                {
                    movimientoTemp.EstadoMovimiento = EstadoMovimientoTemp.Procesando;
                }
            }

            //lista de piezas existentes en el movimiento
            var listaGuidMov = db.MovimientoTempPiezas.Where(a => a.MovimientoTempID == movimientoTemp.MovimientoTempID).Select(a => a.PiezaID).ToList();

            //Todas las que se agregen aqui se Regresan al ultimo movimiento que tuvieron
            //var listaGuidRegresar = new List<Guid>();

            var listaAdd = new List<MovimientoTempPieza>();
            var listaEdit = new List<MovimientoTempPieza>();

            /*
            SeMovio && !EsPendiente ==
            */
            ListaPiezas = ListaPiezas == null ? new List<Item_MovPieza>() : ListaPiezas;

            foreach (var item in ListaPiezas)
            {
                var existeEnMov = listaGuidMov.Where(a => a == item.PiezaID).Count() > 0 ? true : false;

                var temp = new MovimientoTempPieza()
                {
                    Comentario = item.Comentario,
                    EnError = item.EnError,
                    EsPendiente = item.EsPendiente,
                    MovimientoTempID = movimientoTemp.MovimientoTempID,
                    PiezaID = item.PiezaID,
                    SeMovio = item.SeMovio
                    //Orden = db.MovimientoTempPiezas.OrderByDescending(a => a.Orden).FirstOrDefault(a => a.PiezaID == item.PiezaID).Orden + 1
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
                    listaGuidMov.Remove(item.PiezaID);
                }
            }

            //las piezas que no estuvieron eliminarlas
            //y buscar el movimiento anterior donde fueron validas
            //para regresar el estatus.
            if (ModelState.IsValid)
            {
                db.Entry(movimientoTemp).State = EntityState.Modified;
                db.SaveChanges();



                AlertaSuccess($"Se edito el movimiento: <b>{movimientoTemp.Folio}</b>", true);


                //Agregar, Editar y Eliminar las piezas

                //Agregar
                foreach (var item in listaAdd)
                {
                    var pieza = db.Piezas.Find(item.PiezaID);

                    //ejecutar el movimiento de la pieza al estar el movimiento concluido
                    if (movimientoTemp.EstadoMovimiento == EstadoMovimientoTemp.Concluido && !item.EnError)
                    {

                        //ejecutar el movimiento de ubicacion
                        pieza.UbicacionID = movimientoTemp.UbicacionDestinoID;
                        db.Entry(pieza).State = EntityState.Modified;
                        db.SaveChanges();

                        item.SeMovio = true;
                        item.EsPendiente = false;
                    }

                    var ordenActual = pieza.MovimientoTempPiezas.Count > 0 ? pieza.MovimientoTempPiezas.OrderByDescending(a => a.Orden).FirstOrDefault().Orden : 0;

                    item.Orden = item.EnError ? 0 : ordenActual + 1;

                    db.MovimientoTempPiezas.Add(item);
                    db.SaveChanges();

                    var temp = ListaPiezas.FirstOrDefault(a => a.PiezaID == item.PiezaID);
                    AlertaSuccess($"Se agrego la pieza [<b>{temp.FolioPieza}</b>]", true);
                }

                //Editar
                foreach (var item in listaEdit)
                {
                    //buscar la pieza
                    var pieza = db.Piezas.Find(item.PiezaID);
                    var piezaEnMovReal = pieza.MovimientoTempPiezas.FirstOrDefault(a => a.MovimientoTempID == movimientoTemp.MovimientoTempID);
                    //saber si la pieza cambio en error
                    bool ConCambios = piezaEnMovReal.EnError != item.EnError;

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
                            if (piezaEnMovReal.Orden == 0)
                            {
                                //generar un orden
                                var ordenActual = pieza.MovimientoTempPiezas.Count > 0 ? pieza.MovimientoTempPiezas.OrderByDescending(a => a.Orden).FirstOrDefault().Orden : 0;
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
                        item.Comentario = piezaEnMovReal.Comentario;
                        item.EnError = piezaEnMovReal.EnError;
                        item.EsPendiente = piezaEnMovReal.EsPendiente;
                        item.Orden = piezaEnMovReal.Orden;
                        item.SeMovio = piezaEnMovReal.SeMovio;
                    }

                    //Si concluido y sin error
                    if (movimientoTemp.EstadoMovimiento == EstadoMovimientoTemp.Concluido && !item.EnError)
                    {
                        //validar que la pieza este en su ultimo movimiento
                        bool esUltimo = pieza.MovimientoTempPiezas.Where(a => a.Orden > item.Orden && !a.EnError).Count() > 0 ? false : true;
                        bool conPendientes = pieza.MovimientoTempPiezas.Where(a => a.EsPendiente && a.MovimientoTempID != movimientoTemp.MovimientoTempID && !a.EnError).Count() > 0 ? true : false;

                        if (esUltimo && !conPendientes)
                        {
                            //Ejecutar el cambio de ubicacion
                            //Pieza ubicacion es nula
                            if (pieza.Ubicacion == null || pieza.UbicacionID == movimientoTemp.UbicacionOrigenID)
                            {
                                pieza.UbicacionID = movimientoTemp.UbicacionDestinoID;
                                db.Entry(pieza).State = EntityState.Modified;
                                db.SaveChanges();

                                item.SeMovio = true;
                                item.EsPendiente = false;
                                item.EnError = false;
                                item.Comentario = "";

                                var temp = ListaPiezas.FirstOrDefault(a => a.PiezaID == item.PiezaID);
                                AlertaInfo($"{temp.FolioObra}. Se edito la pieza [<b>{temp.FolioPieza}</b>]", true);
                            }
                        }
                    }

                    pieza = null;
                    piezaEnMovReal = null;

                    //buscar el objeto
                    var itemTemp = db.MovimientoTempPiezas.Find(item.MovimientoTempID, item.PiezaID);
                    itemTemp.Comentario = item.Comentario;
                    itemTemp.EnError = item.EnError;
                    itemTemp.EsPendiente = item.EsPendiente;
                    itemTemp.Orden = item.Orden;
                    itemTemp.SeMovio = item.SeMovio;

                    db.Entry(itemTemp).State = EntityState.Modified;
                    db.SaveChanges();
                }

                //Eliminar
                foreach (var gPiezaID in listaGuidMov)
                {
                    var item = db.MovimientoTempPiezas.FirstOrDefault(a => a.PiezaID == gPiezaID && a.MovimientoTempID == movimientoTemp.MovimientoTempID);

                    bool esEliminable = false;
                    string Folio = "";
                    bool RegresarHistorial = false;

                    if (item != null)
                    {
                        bool esUltimo = item.Pieza.MovimientoTempPiezas.Where(a => a.Orden > item.Orden && !a.EnError).Count() > 0 ? false : true;
                        bool conPendientes = item.Pieza.MovimientoTempPiezas.Where(a => a.EsPendiente && a.MovimientoTempID != movimientoTemp.MovimientoTempID && !a.EnError).Count() > 0 ? true : false;
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


                    if (esEliminable)
                    {
                        db.MovimientoTempPiezas.Remove(item);
                        db.SaveChanges();
                        AlertaDanger($"Se elimino la pieza [<b>{Folio}</b>]", true);


                        //Regresar el estado de la pieza
                        if (RegresarHistorial)
                        {
                            item = null;
                            var pieza = db.Piezas.Find(gPiezaID);
                            if (pieza != null)
                            {
                                if (pieza.UbicacionID == movimientoTemp.UbicacionDestinoID)
                                {
                                    pieza.UbicacionID = movimientoTemp.UbicacionOrigenID;
                                    db.Entry(pieza).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                }


                return RedirectToAction("Detalles", new { id = movimientoTemp.MovimientoTempID });
            }

            var listaUbicaciones = db.Ubicaciones.Where(a => a.Status).Select(a => new { a.Nombre, a.UbicacionID }).OrderBy(a => a.Nombre);

            ViewBag.UbicacionDestinoID = new SelectList(listaUbicaciones, "UbicacionID", "Nombre", movimientoTemp.UbicacionDestinoID);
            //ViewBag.UbicacionOrigenID = new SelectList(listaUbicaciones, "UbicacionID", "Nombre");


            movimientoTemp.TipoMovimiento = db.TipoMovimientos.Find(movimientoTemp.TipoMovimientoID);
            movimientoTemp.UbicacionOrigen = db.Ubicaciones.Find(movimientoTemp.UbicacionOrigen);


            return View(movimientoTemp);
        }

        // GET: MovimientoTemp/Delete/5
        [CustomAuthorize(permiso = "")]
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MovimientoTemp movimientoTemp = db.MovimientosTemp.Find(id);
            if (movimientoTemp == null)
            {
                return HttpNotFound();
            }
            return View(movimientoTemp);
        }

        // POST: MovimientoTemp/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permiso = "")]
        public ActionResult DeleteConfirmed(Guid id)
        {
            MovimientoTemp movimientoTemp = db.MovimientosTemp.Find(id);
            db.MovimientosTemp.Remove(movimientoTemp);
            db.SaveChanges();
            return RedirectToAction("Index");
        }



        public ActionResult FichaPrint(Guid? id, int i)
        {
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
            var listaAttMovFicha = pieza.TipoPieza.Atributos.Where(a => a.Status && a.MostrarAtributos.Any(b => b.TipoMostrar.Nombre == "Guion" && b.Status) && a.TipoAtributo.Status).OrderBy(a => a.Orden).ToList();

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
                    piezaMini.Atributos.Add(attFicha);
                }
            }

            //var imagen = pieza.ImagenPiezas.OrderBy(a => a.Orden).FirstOrDefault(a => a.Status && a.EsPrincipal);
            TipoArchivo tipoArchivo = db.TipoArchivos.FirstOrDefault(a => a.Temp == "imagen_clave");

            var imagen = pieza.ArchivosPiezas.Where(a => a.Status && a.TipoArchivoID == tipoArchivo.TipoArchivoID && a.MostrarArchivos.Any(b => b.TipoMostrarArchivo.Nombre == "Guion" && b.Status)).OrderBy(a => a.Orden).FirstOrDefault();


            if (imagen != null)
            {
                piezaMini.ImagenID = imagen.ArchivoPiezaID;
                piezaMini.RutaImagenMini = imagen.RutaThumb;
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
