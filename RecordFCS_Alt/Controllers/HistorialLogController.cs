using RecordFCS_Alt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace RecordFCS_Alt.Controllers
{
    public class HistorialLogController : Controller
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


        public ActionResult IndexAutor(string id)
        {
            string tablaNombre = "Autor";
            string categoria = "Catalogo";
            string registroNombre = "";

            List<HistorialLog> listaHistorial = new List<HistorialLog>();

            listaHistorial = db.HistorialLogs.Where(a => a.LlavePrimaria == id && a.TablaNombre == tablaNombre && a.CategoriaTipo == categoria).OrderByDescending(a => a.EventoFecha).ToList();

            var autor = db.Autores.FirstOrDefault(a => a.AutorID == new Guid(id));
            registroNombre = Regex.Replace(string.Format(autor.Seudonimo + " " + autor.Nombre + " " + autor.ApellidoPaterno + " " + autor.ApellidoMaterno).ToString().Trim(), @"\s+", " ");


            ViewBag.TablaNombre = tablaNombre;
            ViewBag.Categoria = categoria;
            ViewBag.LlavePrimaria = id;
            ViewBag.RegistroNombre = registroNombre;


            return PartialView("_Index", listaHistorial);
        }


        public ActionResult IndexUbicacion(string id)
        {
            string tablaNombre = "Ubicacion";
            string categoria = "Catalogo";
            string registroNombre = "";

            List<HistorialLog> listaHistorial = new List<HistorialLog>();


            listaHistorial = db.HistorialLogs.Where(a => a.LlavePrimaria == id && a.TablaNombre == tablaNombre && a.CategoriaTipo == categoria).OrderByDescending(a => a.EventoFecha).ToList();


            var ubicacion = db.Ubicaciones.FirstOrDefault(a => a.UbicacionID == new Guid(id));

            registroNombre = Regex.Replace(string.Format(ubicacion.Nombre).ToString().Trim(), @"\s+", " ");


            ViewBag.TablaNombre = tablaNombre;
            ViewBag.Categoria = categoria;
            ViewBag.LlavePrimaria = id;
            ViewBag.RegistroNombre = registroNombre;


            return PartialView("_Index", listaHistorial);
        }


        public ActionResult IndexTipoTecnica(string id)
        {
            string tablaNombre = "TipoTecnica";
            string categoria = "Catalogo";
            string registroNombre = "";

            List<HistorialLog> listaHistorial = new List<HistorialLog>();


            listaHistorial = db.HistorialLogs.Where(a => a.LlavePrimaria == id && a.TablaNombre == tablaNombre && a.CategoriaTipo == categoria).OrderByDescending(a => a.EventoFecha).ToList();


            var tipoTecnica = db.TipoTecnicas.FirstOrDefault(a => a.TipoTecnicaID == new Guid(id));

            registroNombre = Regex.Replace(string.Format(tipoTecnica.Nombre).ToString().Trim(), @"\s+", " ");


            ViewBag.TablaNombre = tablaNombre;
            ViewBag.Categoria = categoria;
            ViewBag.LlavePrimaria = id;
            ViewBag.RegistroNombre = registroNombre;

            return PartialView("_Index", listaHistorial);
        }


        public ActionResult IndexSimple(string id, string tabla, string nombre)
        {

//            var listaAgrupada = db.HistorialLogs.Where(a => a.LlavePrimaria == id && a.TablaNombre == tabla && a.CategoriaTipo == categoria).OrderByDescending(a => a.EventoFecha).ToList();


            var lista = db.HistorialLogs.Where(a => a.LlavePrimaria == id && a.TablaNombre == tabla).OrderByDescending(a=> a.EventoFecha).ToList();

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


        public ActionResult LogAccionesUsuario(Guid? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Usuario usuario = db.Usuarios.Find(id);

            if (usuario == null) return HttpNotFound();


            var listaAgrupada = usuario.HistorialAcciones.GroupBy(a => a.EventoFecha.Date).OrderByDescending(a=> a.Key).ToList();


            return PartialView("_LogAccionesUsuario", listaAgrupada);
        }

    }
}