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

namespace RecordFCS_Alt.Controllers
{
    public class MovimientoTempController : Controller
    {
        private RecordFCSContext db = new RecordFCSContext();

        // GET: MovimientoTemp
        public ActionResult Index(MovimientoTemp MovTemp = null)
        {

            var listaUbicaciones = db.Ubicaciones.Where(a => a.Status).Select(a => new { a.Nombre, a.UbicacionID }).OrderBy(a => a.Nombre);

            ViewBag.UbicacionDestinoID = new SelectList(listaUbicaciones, "UbicacionID", "Nombre");
            ViewBag.UbicacionOrigenID = new SelectList(listaUbicaciones, "UbicacionID", "Nombre");

            ViewBag.TipoMovimientoID = new SelectList(db.TipoMovimientos.Where(a => a.Status).OrderBy(a => a.Nombre), "TipoMovimientoID", "Nombre",MovTemp.TipoMovimientoID);

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

        // GET: MovimientoTemp/Details/5
        public ActionResult Details(Guid? id)
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

        // GET: MovimientoTemp/Create
        public ActionResult Crear(Guid? TipoMovimientoID, bool TieneExposicion)
        {
            var tipoMov = db.TipoMovimientos.Find(TipoMovimientoID);

            if (tipoMov != null)
            {

                var mov = new MovimientoTemp()
                {
                    TieneExposicion = TieneExposicion,
                    TipoMovimientoID = tipoMov.TipoMovimientoID,
                    TipoMovimiento = tipoMov
                };

                ViewBag.UbicacionDestinoID = new SelectList(db.Ubicaciones, "UbicacionID", "Nombre");
                ViewBag.UbicacionOrigenID = new SelectList(db.Ubicaciones, "UbicacionID", "Nombre");

                return View(mov);
            }

            var movTemp = new MovimientoTemp()
            {
                TieneExposicion = TieneExposicion
            };

            return View("Index", movTemp);

        }

        // POST: MovimientoTemp/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear([Bind(Include = "MovimientoTempID,Folio,TipoMovimientoID,TieneExposicion,EstadoMovimiento,FechaRegistro,Observaciones,FechaSalida,FechaRetorno,UbicacionOrigenID,UbicacionDestinoID,Seguro_Asegurador,Seguro_NoPoliza,Seguro_FechaInicial,Seguro_FechaFinal,Seguro_Notas,Solicitante_Cargo,Solicitante_Institucion,Solicitante_Representante,Solicitante_RepresentanteCargo,Solicitante_DictamenCondicionEspacio,Solicitante_DictamenSeguridad,Solicitante_PeticionRecibida,Solicitante_FacilityReport,Solicitante_RevisionGuion,Solicitante_CartaAceptacion,Solicitante_ListaAvaluo,Solicitante_ContratoComodato,Solicitante_TramitesFianza,Solicitante_PolizaSeguro,Solicitante_CondicionConservacion,Solicitante_AvisoSeguridad,Solicitante_CartasEntregaRecepcion,Transporte_Empresa,Transporte_Medio,Transporte_Recorrido,Transporte_Horarios,Transporte_Notas,Autorizacion_Nombre1,Autorizacion_Nombre2,Autorizacion_Fecha,Responsable_Nombre,Responsable_Institucion,Responsable_FechaSalida,Responsable_FechaRetorno,Exposicion_Titulo,Exposicion_Curador,Exposicion_Sede,Exposicion_Pais,Exposicion_FechaInicial,Exposicion_FechaFinal,Temp")] MovimientoTemp movimientoTemp)
        {
            if (ModelState.IsValid)
            {
                movimientoTemp.MovimientoTempID = Guid.NewGuid();
                db.MovimientosTemp.Add(movimientoTemp);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.TipoMovimientoID = new SelectList(db.TipoMovimientos, "TipoMovimientoID", "Nombre", movimientoTemp.TipoMovimientoID);
            ViewBag.UbicacionDestinoID = new SelectList(db.Ubicaciones, "UbicacionID", "Nombre", movimientoTemp.UbicacionDestinoID);
            ViewBag.UbicacionOrigenID = new SelectList(db.Ubicaciones, "UbicacionID", "Nombre", movimientoTemp.UbicacionOrigenID);
            return View(movimientoTemp);
        }

        // GET: MovimientoTemp/Edit/5
        public ActionResult Edit(Guid? id)
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
            ViewBag.TipoMovimientoID = new SelectList(db.TipoMovimientos, "TipoMovimientoID", "Nombre", movimientoTemp.TipoMovimientoID);
            ViewBag.UbicacionDestinoID = new SelectList(db.Ubicaciones, "UbicacionID", "Nombre", movimientoTemp.UbicacionDestinoID);
            ViewBag.UbicacionOrigenID = new SelectList(db.Ubicaciones, "UbicacionID", "Nombre", movimientoTemp.UbicacionOrigenID);
            return View(movimientoTemp);
        }

        // POST: MovimientoTemp/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MovimientoTempID,Folio,TipoMovimientoID,TieneExposicion,EstadoMovimiento,FechaRegistro,Observaciones,FechaSalida,FechaRetorno,UbicacionOrigenID,UbicacionDestinoID,Seguro_Asegurador,Seguro_NoPoliza,Seguro_FechaInicial,Seguro_FechaFinal,Seguro_Notas,Solicitante_Cargo,Solicitante_Institucion,Solicitante_Representante,Solicitante_RepresentanteCargo,Solicitante_DictamenCondicionEspacio,Solicitante_DictamenSeguridad,Solicitante_PeticionRecibida,Solicitante_FacilityReport,Solicitante_RevisionGuion,Solicitante_CartaAceptacion,Solicitante_ListaAvaluo,Solicitante_ContratoComodato,Solicitante_TramitesFianza,Solicitante_PolizaSeguro,Solicitante_CondicionConservacion,Solicitante_AvisoSeguridad,Solicitante_CartasEntregaRecepcion,Transporte_Empresa,Transporte_Medio,Transporte_Recorrido,Transporte_Horarios,Transporte_Notas,Autorizacion_Nombre1,Autorizacion_Nombre2,Autorizacion_Fecha,Responsable_Nombre,Responsable_Institucion,Responsable_FechaSalida,Responsable_FechaRetorno,Exposicion_Titulo,Exposicion_Curador,Exposicion_Sede,Exposicion_Pais,Exposicion_FechaInicial,Exposicion_FechaFinal,Temp")] MovimientoTemp movimientoTemp)
        {
            if (ModelState.IsValid)
            {
                db.Entry(movimientoTemp).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.TipoMovimientoID = new SelectList(db.TipoMovimientos, "TipoMovimientoID", "Nombre", movimientoTemp.TipoMovimientoID);
            ViewBag.UbicacionDestinoID = new SelectList(db.Ubicaciones, "UbicacionID", "Nombre", movimientoTemp.UbicacionDestinoID);
            ViewBag.UbicacionOrigenID = new SelectList(db.Ubicaciones, "UbicacionID", "Nombre", movimientoTemp.UbicacionOrigenID);
            return View(movimientoTemp);
        }

        // GET: MovimientoTemp/Delete/5
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
