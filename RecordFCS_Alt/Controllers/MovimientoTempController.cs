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

namespace RecordFCS_Alt.Controllers
{
    public class MovimientoTempController : BaseController
    {
        private RecordFCSContext db = new RecordFCSContext();

        // GET: MovimientoTemp
        [CustomAuthorize(permiso = "")]
        public ActionResult Index(MovimientoTemp MovTemp = null)
        {

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
            if (id == null)return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            MovimientoTemp mov = db.MovimientosTemp.Find(id);
            if (mov == null)return HttpNotFound();

            // 1 2 3 4 [5] 6 7 8 9 10
            // [1] 2 3 4 5 6 7 8 9 10
            // 1 2 3 4 5 6 7 8 9 9 [10]

            MovimientoTemp movTemp = null;
            movTemp = db.MovimientosTemp.FirstOrDefault(a => a.Folio == mov.Folio - 1);
            ViewBag.MovAnterior = movTemp == null ? Guid.Empty : movTemp.MovimientoTempID;

            movTemp = db.MovimientosTemp.FirstOrDefault(a => a.Folio == mov.Folio + 1);
            ViewBag.MovSiguiente = movTemp == null ? Guid.Empty : movTemp.MovimientoTempID;


            return View(mov);
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

                AlertaSuccess($"Se registro el movimiento: <b>{movimientoTemp.Folio}</b>",true);

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
            //ViewBag.UbicacionOrigenID = new SelectList(listaUbicaciones, "UbicacionID", "Nombre");
            
            return View(movimientoTemp);
        }

        // POST: MovimientoTemp/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permiso = "")]
        public ActionResult Edit([Bind(Include = "MovimientoTempID,Folio,TipoMovimientoID,TieneExposicion,EstadoMovimiento,FechaRegistro,Observaciones,FechaSalida,FechaRetorno,UbicacionOrigenID,UbicacionDestinoID,Seguro_Asegurador,Seguro_NoPoliza,Seguro_FechaInicial,Seguro_FechaFinal,Seguro_Notas,Solicitante_Cargo,Solicitante_Institucion,Solicitante_Representante,Solicitante_RepresentanteCargo,Solicitante_DictamenCondicionEspacio,Solicitante_DictamenSeguridad,Solicitante_PeticionRecibida,Solicitante_FacilityReport,Solicitante_RevisionGuion,Solicitante_CartaAceptacion,Solicitante_ListaAvaluo,Solicitante_ContratoComodato,Solicitante_TramitesFianza,Solicitante_PolizaSeguro,Solicitante_CondicionConservacion,Solicitante_AvisoSeguridad,Solicitante_CartasEntregaRecepcion,Transporte_Empresa,Transporte_Medio,Transporte_Recorrido,Transporte_Horarios,Transporte_Notas,Autorizacion_Nombre1,Autorizacion_Nombre2,Autorizacion_Fecha,Responsable_Nombre,Responsable_Institucion,Responsable_FechaSalida,Responsable_FechaRetorno,Exposicion_Titulo,Exposicion_Curador,Exposicion_Sede,Exposicion_Pais,Exposicion_FechaInicial,Exposicion_FechaFinal,Temp")] MovimientoTemp movimientoTemp)
        {
            if (ModelState.IsValid)
            {

                db.Entry(movimientoTemp).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Detalles", new { id = movimientoTemp.MovimientoTempID });
            }

            ViewBag.TipoMovimientoID = new SelectList(db.TipoMovimientos, "TipoMovimientoID", "Nombre", movimientoTemp.TipoMovimientoID);
            ViewBag.UbicacionDestinoID = new SelectList(db.Ubicaciones, "UbicacionID", "Nombre", movimientoTemp.UbicacionDestinoID);
            ViewBag.UbicacionOrigenID = new SelectList(db.Ubicaciones, "UbicacionID", "Nombre", movimientoTemp.UbicacionOrigenID);
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
