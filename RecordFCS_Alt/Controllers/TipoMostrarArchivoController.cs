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
using RecordFCS_Alt.Helpers.Historial;

namespace RecordFCS_Alt.Controllers
{
    public class TipoMostrarArchivoController : BaseController
    {
        private RecordFCSContext db = new RecordFCSContext();

        // GET: TipoMostrarArchivo
        [CustomAuthorize(permiso = "tMosList")]
        public ActionResult Index()
        {
            return View();
        }

        [CustomAuthorize(permiso = "")]
        public ActionResult Lista()
        {
            var lista = db.TipoMostrarArchivos.OrderBy(a => a.Nombre).ToList();

            ViewBag.totalRegistros = lista.Count;

            return PartialView("_Lista", lista);
        }


        //// GET: TipoMostrarArchivo/Details/5
        //public ActionResult Details(Guid? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    TipoMostrarArchivo tipoMostrarArchivo = db.TipoMostrarArchivos.Find(id);
        //    if (tipoMostrarArchivo == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(tipoMostrarArchivo);
        //}

        // GET: TipoMostrarArchivo/Crear
        [CustomAuthorize(permiso = "tMosNew")]
        public ActionResult Crear()
        {
            var tipoMostrarArchivo = new TipoMostrarArchivo()
            {
                Status = true
            };

            return PartialView("_Crear", tipoMostrarArchivo);
        }

        // POST: TipoMostrarArchivo/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permiso = "tMosNew")]
        public ActionResult Crear(TipoMostrarArchivo tipoMostrarArchivo)
        {
            try
            {
                #region Validaciones previas

                //validar nombre
                var tma = db.TipoMostrarArchivos.Select(a => new { a.TipoMostrarArchivoID, a.Nombre }).FirstOrDefault(a => a.Nombre == tipoMostrarArchivo.Nombre);

                if (tma != null)
                    ModelState.AddModelError("Nombre", "Ya existe.");

                #endregion
                
                if (ModelState.IsValid)
                {
                    //Crear la entidad
                    tipoMostrarArchivo.TipoMostrarArchivoID = Guid.NewGuid();
                    db.TipoMostrarArchivos.Add(tipoMostrarArchivo);

                    //------ Logica HISTORIAL

                    #region Generar el historial

                    // Generar el historial
                    var historialLog =
                        HistorialLogica.CrearEntidad(
                        tipoMostrarArchivo,
                        tipoMostrarArchivo.GetType().Name,
                        tipoMostrarArchivo.TipoMostrarArchivoID.ToString(),
                        User.UsuarioID,
                        db);

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
                    AlertaSuccess(string.Format("Tipo de mostrar archivo <b>{0}</b> creada.", tipoMostrarArchivo.Nombre), true);

                    string url = Url.Action("Lista", "TipoMostrarArchivo");
                    return Json(new { success = true, url = url });
                }

            }
            catch (Exception)
            {

                ModelState.AddModelError("", "Error desconocido.");
            }


            return PartialView("_Crear", tipoMostrarArchivo);
        }

        // GET: TipoMostrarArchivo/Editar/5
        [CustomAuthorize(permiso = "tMosEdit")]
        public ActionResult Editar(Guid? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            TipoMostrarArchivo tipoMostrarArchivo = db.TipoMostrarArchivos.Find(id);
            if (tipoMostrarArchivo == null)
                return HttpNotFound();

            return PartialView("_Editar", tipoMostrarArchivo);
        }

        // POST: TipoMostrarArchivo/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permiso = "tMosEdit")]
        public ActionResult Editar(TipoMostrarArchivo tipoMostrarArchivo, string Motivo)
        {
            try
            {
                #region Validaciones previas

                //validar el nombre
                var tm = db.TipoMostrarArchivos.Select(a => new { a.Nombre, a.TipoMostrarArchivoID }).FirstOrDefault(a => a.Nombre == tipoMostrarArchivo.Nombre);

                if (tm != null)
                    if (tm.TipoMostrarArchivoID != tipoMostrarArchivo.TipoMostrarArchivoID)
                        ModelState.AddModelError("Nombre", "Ya existe.");

                if (string.IsNullOrWhiteSpace(Motivo))
                    ModelState.AddModelError("Motivo", "Motivo vacio.");

                #endregion
               
                if (ModelState.IsValid)
                {

                    //------ Logica HISTORIAL

                    #region Generar el historial

                    //objeto del formulario
                    var objeto = tipoMostrarArchivo;
                    //Objeto de la base de datos
                    var objetoDB = db.TipoMostrarArchivos.Find(tipoMostrarArchivo.TipoMostrarArchivoID);
                    //tabla o clase a la que pertenece
                    var tablaNombre = objeto.GetType().Name;
                    //llave primaria del objeto
                    var llavePrimaria = objetoDB.TipoMostrarArchivoID.ToString();

                    //generar el historial
                    var historialLog = HistorialLogica.EditarEntidad(
                        objeto,
                        objetoDB,
                        tablaNombre,
                        llavePrimaria,
                        User.UsuarioID,
                        db,
                        Motivo
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
                    else
                    {
                        throw new Exception();
                    }
                    #endregion

                    //------

                    //Logica para terminar la instruccion
                    AlertaInfo(string.Format("Tipo de mostrar archivo: <b>{0}</b> se editó.", tipoMostrarArchivo.Nombre), true);

                    string url = Url.Action("Lista", "TipoMostrarArchivo");
                    return Json(new { success = true, url = url });
                }

            }
            catch (Exception)
            {

                ModelState.AddModelError("", "Error desconocido.");
            }


            return PartialView("_Editar", tipoMostrarArchivo);
        }

        // GET: TipoMostrarArchivo/Eliminar/5
        [CustomAuthorize(permiso = "tMosDel")]
        public ActionResult Eliminar(Guid? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            TipoMostrarArchivo tipoMostrarArchivo = db.TipoMostrarArchivos.Find(id);

            if (tipoMostrarArchivo == null)
                return HttpNotFound();


            return PartialView("_Eliminar", tipoMostrarArchivo);
        }

        // POST: TipoMostrarArchivo/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permiso = "tMosDel")]
        public ActionResult EliminarConfirmado(Guid id)
        {
            string btnValue = Request.Form["accionx"];

            TipoMostrarArchivo tipoMostrarArchivo = db.TipoMostrarArchivos.Find(id);

            var textoNombre = tipoMostrarArchivo.Nombre;
            switch (btnValue)
            {
                case "deshabilitar":
                    tipoMostrarArchivo.Status = false;
                    db.Entry(tipoMostrarArchivo).State = EntityState.Modified;
                    db.SaveChanges();
                    AlertaDefault(string.Format("Se deshabilito <b>{0}</b>", textoNombre), true);
                    break;
                case "eliminar":
                    db.TipoMostrarArchivos.Remove(tipoMostrarArchivo);
                    db.SaveChanges();
                    AlertaDanger(string.Format("Se elimino <b>{0}</b>", textoNombre), true);
                    break;
                default:
                    AlertaDanger(string.Format("Ocurrio un error."), true);
                    break;
            }


            string url = Url.Action("Lista", "TipoMostrarArchivo");
            return Json(new { success = true, url = url });
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
