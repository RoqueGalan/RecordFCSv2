using RecordFCS_Alt.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace RecordFCS_Alt.Controllers
{
    public class HistorialLogController : AsyncController
    {

        private RecordFCSContext db = new RecordFCSContext();

        // GET: HistorialLog
        public ActionResult Index()
        {
            string tablaNombre = "";
            string categoria = "";
            string id = "";
            string registroNombre = "";



            ViewBag.TablaNombre = tablaNombre;
            ViewBag.Categoria = categoria;
            ViewBag.LlavePrimaria = id;
            ViewBag.RegistroNombre = registroNombre;

            return PartialView("_Index");
        }

        public ActionResult IndexSimple(string id, string tabla, string nombre)
        {

            //            var listaAgrupada = db.HistorialLogs.Where(a => a.LlavePrimaria == id && a.TablaNombre == tabla && a.CategoriaTipo == categoria).OrderByDescending(a => a.EventoFecha).ToList();


            var lista = db.HistorialLogs.Where(a => a.LlavePrimaria == id && a.TablaNombre == tabla).OrderByDescending(a => a.EventoFecha).ToList();


            ////Buscar el valor 
            //foreach (var log in lista)
            //{
            //    foreach (var detalle in log.HistorialLogDetalles)
            //    {
            //        detalle.ValorNuevo = string.IsNullOrWhiteSpace(detalle.ValorNuevo) ? detalle.ValorNuevo : ExtraerValorDeTabla(detalle.ColumnaNombre, detalle.ValorNuevo);
            //        detalle.ValorOriginal = string.IsNullOrWhiteSpace(detalle.ValorOriginal) ? detalle.ValorOriginal : ExtraerValorDeTabla(detalle.ColumnaNombre, detalle.ValorOriginal);
            //    }
            //}




            var listaAgrupada = lista.GroupBy(a => a.EventoFecha.ToString("dd MMMM yyyy")).ToList();


            nombre = nombre ?? "";
            nombre = Regex.Replace(string.Format(nombre).ToString().Trim(), @"\s+", " ");

            ViewBag.TablaNombre = tabla;
            ViewBag.LlavePrimaria = id;
            ViewBag.RegistroNombre = nombre;



            return PartialView("_Index", listaAgrupada);
        }

        //public ActionResult IndexCompuesto(string id, string tabla, string categoria, string nombre)
        //{
        //    List<HistorialLog> listaHistorial = new List<HistorialLog>();

        //    listaHistorial = db.HistorialLogs.Where(a => a.LlavePrimaria == id && a.TablaNombre == tabla && a.CategoriaTipo == categoria).OrderByDescending(a => a.EventoFecha).ToList();

        //    nombre = Regex.Replace(string.Format(nombre).ToString().Trim(), @"\s+", " ");

        //    ViewBag.TablaNombre = tabla;
        //    ViewBag.Categoria = categoria;
        //    ViewBag.LlavePrimaria = id;
        //    ViewBag.RegistroNombre = nombre;

        //    return PartialView("_Index", listaHistorial);
        //}

        public ActionResult IndexUsuarioLog(Guid? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            //Usuario usuario = db.Usuarios.Find(id);

            //if (usuario == null) return HttpNotFound();


            //var listaAgrupada = usuario.HistorialAcciones.GroupBy(a => a.EventoFecha.Date).OrderByDescending(a => a.Key).ToList();


            return PartialView("_IndexUsuarioLog", id);
        }


        public ActionResult BuscarPorFechas(string FechaInicial, string FechaFinal, Guid? UsuarioID)
        {
            IQueryable<HistorialLog> listaHistorial = UsuarioID == null ? db.HistorialLogs : db.HistorialLogs.Where(a => a.UsuarioID == UsuarioID);

            //Buscar por fechas

            //Buscar por fecha Inicial
            if (!string.IsNullOrWhiteSpace(FechaInicial))
            {
                //Fecha mayor
                DateTime Fecha = DateTime.ParseExact(FechaInicial, "dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture);
                listaHistorial = listaHistorial.Where(a => DateTime.Compare(a.EventoFecha, Fecha) > 0);
            }

            //Buscar por fecha Final
            if (!string.IsNullOrWhiteSpace(FechaFinal))
            {
                //Fecha menor
                DateTime Fecha = DateTime.ParseExact(FechaFinal, "dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture);
                listaHistorial = listaHistorial.Where(a => DateTime.Compare(a.EventoFecha, Fecha) < 0);
            }

            listaHistorial = listaHistorial.OrderByDescending(a => a.EventoFecha);


            return PartialView("_ListaPorFechas", listaHistorial);
        }


        public ActionResult Detalles(Guid? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            HistorialLog historial = db.HistorialLogs.Find(id);

            if (historial == null) return HttpNotFound();


            return PartialView("_Detalles", historial);
        }

        [HttpPost]
        public ActionResult IrA(Guid? id)
        {
            string url = "";
            bool success = false;
            bool modal = false;
            try
            {

                if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                HistorialLog historial = db.HistorialLogs.Find(id);

                if (historial == null) return HttpNotFound();

                //Guid llave1 = new Guid(historial.LlavePrimaria);

                switch (historial.TablaNombre)
                {
                    case "Autor":
                        url = Url.Action("Autor", "Editar", new { id = historial.LlavePrimaria });
                        modal = true;
                        break;


                    default:
                        return HttpNotFound();
                }



            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            return Json(new { success = success, url = url, modal = modal }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult DetallesMovimientoLog(Guid? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            HistorialLog historial = db.HistorialLogs.Find(id);
            if (historial == null) return HttpNotFound();

            List<HistorialLog> lista = new List<HistorialLog>();

            //Buscar las piezas que corresponden al movimiento

            lista = db.HistorialLogs.Where(a => a.LlavePrimaria.StartsWith(historial.LlavePrimaria + ",") && a.EventoFecha == historial.EventoFecha).ToList();

            foreach (var item in lista)
            {
                try
                {
                    Guid PiezaID = new Guid(item.LlavePrimaria.Replace(historial.LlavePrimaria + ",", ""));

                    if (PiezaID != Guid.Empty)
                        item.TablaNombre = db.Piezas.FirstOrDefault(a => a.PiezaID == PiezaID).ImprimirFolio();
                    else
                        item.TablaNombre = "Pieza sin existencia";
                }
                catch (Exception)
                {
                    item.TablaNombre = "Pieza sin existencia";
                }


            }

            return PartialView("_DetallesMovimientoLog", lista);
        }




        #region Historial de MOVIMIENTOS

        public ActionResult IndexMovimiento(string id)
        {
            var db = new RecordFCSContext();
            db.Configuration.LazyLoadingEnabled = false;

            string tabla = "MovimientoTemp";

            var lista = db.HistorialLogs.Where(a => a.LlavePrimaria == id && a.TablaNombre == tabla).OrderByDescending(a => a.EventoFecha).ToList();

            return PartialView("_IndexMovimiento", lista);
        }


        public ActionResult DetalleMovimiento(Guid? id)
        {
            // el id es el ID del HistorialLog
            var db = new RecordFCSContext();


            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            
            HistorialLog movLog = db.HistorialLogs.Find(id);
            
            if (movLog == null) return HttpNotFound();


            var icono = "";

            switch (movLog.EventoTipo)
            {
                case "Crear":
                    icono = "fa fa-plus";
                    break;

                case "Editar":
                    icono = "fa fa-pencil";
                    break;

                case "Eliminar":
                    icono = "fa fa-trash";
                    break;
                default:
                    icono = "";
                    break;
            }


            ViewBag.icono = icono;

            return PartialView("_DetalleMovimiento", movLog);
        }


        public async Task<ActionResult> PiezasEnMovimiento(Guid? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            HistorialLog historial = db.HistorialLogs.Find(id);
            if (historial == null) return HttpNotFound();

            List<HistorialLog> lista = new List<HistorialLog>();

            //Buscar las piezas que corresponden al movimiento

            lista = await db.HistorialLogs.Where(a => a.LlavePrimaria.StartsWith(historial.LlavePrimaria + ",") && a.EventoFecha == historial.EventoFecha).ToListAsync();

            foreach (var item in lista)
            {
                try
                {
                    Guid PiezaID = new Guid(item.LlavePrimaria.Replace(historial.LlavePrimaria + ",", ""));

                    if (PiezaID != Guid.Empty)
                        item.TablaNombre = db.Piezas.FirstOrDefault(a => a.PiezaID == PiezaID).ImprimirFolio();
                    else
                        item.TablaNombre = "Pieza sin existencia";

                    item.HistorialLogDetalles = item.HistorialLogDetalles.Where(a => a.ColumnaNombre != "PiezaID").OrderBy(a=> a.ColumnaNombre).ToList();

                }
                catch (Exception)
                {
                    item.TablaNombre = "Pieza sin existencia";
                }


            }


            lista = lista.OrderBy(a => a.TablaNombre).ToList();


            return PartialView("_ListaPiezasEnMovimiento",lista);
        }


        #endregion
    }
}