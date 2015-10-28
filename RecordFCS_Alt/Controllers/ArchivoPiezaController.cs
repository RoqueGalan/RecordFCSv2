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
using System.IO;
using RecordFCS_Alt.Helpers;

namespace RecordFCS_Alt.Controllers
{
    public class ArchivoPiezaController : BaseController
    {
        private RecordFCSContext db = new RecordFCSContext();

        [CustomAuthorize(permiso = "")]
        public ActionResult ContenedorImagen(Guid? id, Guid? TipoMostrarArchivoID, bool esCompleta = false)
        {
            if (id == null && TipoMostrarArchivoID == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Pieza pieza = db.Piezas.Find(id);

            if (pieza == null) { return HttpNotFound(); }

            TipoArchivo tipoArchivo = db.TipoArchivos.FirstOrDefault(a => a.Temp == "imagen_clave");

            List<ArchivoPieza> lista = new List<ArchivoPieza>();

            if (esCompleta)
                lista = pieza.ArchivosPiezas.Where(a => a.TipoArchivoID == tipoArchivo.TipoArchivoID).OrderBy(a => a.Orden).ToList();
            else
                lista = pieza.ArchivosPiezas.Where(a => a.Status && a.TipoArchivoID == tipoArchivo.TipoArchivoID && a.MostrarArchivos.Any(b => b.TipoMostrarArchivoID == TipoMostrarArchivoID.Value && b.Status)).OrderBy(a => a.Orden).ToList();

            ViewBag.esCompleta = esCompleta;
            ViewBag.id = id;
            ViewBag.tipoMostrarArchivoID = TipoMostrarArchivoID;
            ViewBag.tipoArchivoID = tipoArchivo.TipoArchivoID;

            return PartialView("_CarruselImagenes", lista);
        }


        // GET: ArchivoPieza
        public ActionResult Index(Guid? id, Guid? TipoArchivoID, Guid? TipoMostrarArchivoID, bool esCompleta = false)
        {
            if (id == null || TipoArchivoID == null || TipoMostrarArchivoID == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var tipoArchivo = db.TipoArchivos.Find(TipoArchivoID);
            if (tipoArchivo == null) return HttpNotFound();

            ViewBag.esCompleta = esCompleta;
            ViewBag.id = id;
            ViewBag.TipoArchivoID = TipoArchivoID;
            ViewBag.TipoMostrarArchivoID = TipoMostrarArchivoID;

            return PartialView("_Index", tipoArchivo);
        }


        public ActionResult Lista(Guid? id, Guid? TipoArchivoID, Guid? TipoMostrarArchivoID, bool esCompleta = false)
        {
            if (id == null || TipoArchivoID == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            if (TipoMostrarArchivoID == null && esCompleta == false) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var pieza = db.Piezas.Find(id);
            if (pieza == null) return HttpNotFound();

            var tipoArchivo = db.TipoArchivos.Find(TipoArchivoID);
            if (tipoArchivo == null) return HttpNotFound();

            var lista = pieza.ArchivosPiezas.Where(a => a.TipoArchivoID == tipoArchivo.TipoArchivoID).OrderBy(a=> a.Orden).ToList();

            if (!esCompleta)
                lista = lista.Where(a => a.Status && a.MostrarArchivos.Any(b=> b.TipoMostrarArchivoID == TipoMostrarArchivoID.Value && b.Status)).ToList();

            ViewBag.esCompleta = esCompleta;
            ViewBag.id = id;
            ViewBag.TipoArchivoID = TipoArchivoID;
            ViewBag.TipoMostrarArchivoID = TipoMostrarArchivoID;
            return PartialView("_Lista", lista);
        }

        //// GET: ArchivoPieza/Details/5
        public ActionResult Detalles(Guid? id , string tipo = "Magnificent")
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ArchivoPieza archivoPieza = db.ArchivoPiezas.Find(id);

            if (archivoPieza == null)
            {
                return HttpNotFound();
            }

            FileInfo infoThumb = new FileInfo(Server.MapPath(archivoPieza.Ruta));
          

            switch (archivoPieza.TipoArchivo.Temp)
            {
                case "audio_clave":
                case "video_clave":
                case "interactivo_clave":
                    return PartialView("_MultimediaPlay", archivoPieza);
                case "imagen_clave":
                    string vistaText = "_ImagenZoom_" + tipo;
                    return PartialView(vistaText, archivoPieza);
                default:
                    return PartialView("_OtrosPlay", archivoPieza);
            }
        }

        //// GET: ArchivoPieza/Create
        [CustomAuthorize(permiso = "arcNew")]
        public ActionResult Crear(Guid? id, Guid? TipoArchivoID)
        {
            if (id == null || TipoArchivoID == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var tipoArchivo = db.TipoArchivos.Find(TipoArchivoID);
            if (tipoArchivo == null) return HttpNotFound();
            var pieza = db.Piezas.Find(id);
            if (pieza == null) return HttpNotFound();

            var archivoPieza = new ArchivoPieza()
            {
                PiezaID = pieza.PiezaID,
                Status = true,
                TipoArchivoID = tipoArchivo.TipoArchivoID,
                TipoArchivo = tipoArchivo,
                Orden = pieza.ArchivosPiezas.Where(a => a.TipoArchivoID == tipoArchivo.TipoArchivoID).Count() + 1
            };

            archivoPieza.MostrarArchivos = new List<MostrarArchivo>();

            foreach (var item in db.TipoMostrarArchivos.Where(a => a.Status).ToList())
            {
                //crear en el archivo objetos temporales

                var tempMostrarArchivo = new MostrarArchivo()
                {
                    ArchivoPiezaID = archivoPieza.ArchivoPiezaID,
                    Status = false,
                    TipoMostrarArchivo = item,
                    TipoMostrarArchivoID = item.TipoMostrarArchivoID,
                };
                archivoPieza.MostrarArchivos.Add(tempMostrarArchivo);
            }

            if (!Directory.Exists(Server.MapPath(tipoArchivo.Ruta)))
                Directory.CreateDirectory(Server.MapPath(tipoArchivo.Ruta));


            //ViewBag.nombreArchivo = tipoArchivo.Nombre;
            //ViewBag.Lista_TipoMostrarArchivos = ;

            return PartialView("_Crear", archivoPieza);
        }

        // POST: ArchivoPieza/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permiso = "arcNew")]
        public ActionResult Crear(ArchivoPieza archivoPieza, HttpPostedFileBase FileArchivo)
        {
            var listaMostrarArchivos = new List<MostrarArchivo>();

            foreach (var item in db.TipoMostrarArchivos.Where(a => a.Status).ToList())
            {
                string keyStatus = Request.Form["mosArc_" + item.TipoMostrarArchivoID + "_Status"];
                bool valorStatus = keyStatus == "true,false" ? true : false;
                var tempMostrarArchivo = new MostrarArchivo()
                {
                    Status = valorStatus,
                    TipoMostrarArchivo = item,
                    TipoMostrarArchivoID = item.TipoMostrarArchivoID
                };

                listaMostrarArchivos.Add(tempMostrarArchivo);
            }

            //comprobar las extensiones validas
            var tipoArchivo = db.TipoArchivos.Find(archivoPieza.TipoArchivoID);

            archivoPieza.Extension = Path.GetExtension(FileArchivo.FileName);

            if (!tipoArchivo.ExtensionesAceptadas.Replace(" ", "").Split(',').Any(a => "." + a == archivoPieza.Extension))
                ModelState.AddModelError("", "Archivo no compatible.");

            if (FileArchivo == null)
                ModelState.AddModelError("", "Seleccione un archivo");




            if (ModelState.IsValid)
            {
                if (!Directory.Exists(Server.MapPath(tipoArchivo.Ruta)))
                    Directory.CreateDirectory(Server.MapPath(tipoArchivo.Ruta));

                if (!Directory.Exists(Server.MapPath(tipoArchivo.Ruta + "thumb/")))
                    Directory.CreateDirectory(Server.MapPath(tipoArchivo.Ruta + "thumb/"));

                archivoPieza.ArchivoPiezaID = Guid.NewGuid();

                archivoPieza.NombreArchivo = Guid.NewGuid().ToString();

                archivoPieza.Orden = db.ArchivoPiezas.Where(a => a.TipoArchivoID == archivoPieza.TipoArchivoID && a.PiezaID == archivoPieza.PiezaID).Count() + 1;

                archivoPieza.TipoArchivo = tipoArchivo;

                string rutaGuardar = Server.MapPath(archivoPieza.Ruta);

                FileArchivo.SaveAs(rutaGuardar);

                FileArchivo.InputStream.Dispose();
                FileArchivo.InputStream.Close();
                GC.Collect();

                //generar la mini
                switch (archivoPieza.Extension)
                {
                    //imagenes
                    case ".jpg":
                    case ".png":
                    case ".tiff":
                        Thumbnail mini = new Thumbnail()
                        {
                            OrigenSrc = rutaGuardar,
                            DestinoSrc = Server.MapPath(archivoPieza.RutaThumb),
                            LimiteAnchoAlto = 220
                        };
                        mini.GuardarThumbnail();
                        break;
                    //videos
                    case ".mp4":
                    case ".avi":
                        break;
                }
                //validar que la carpeta del tipo de archivo exista
                db.ArchivoPiezas.Add(archivoPieza);
                db.SaveChanges();

                AlertaSuccess(string.Format("Se guardo archivo {0} <b>{1}</b>", tipoArchivo.Nombre, archivoPieza.Titulo), true);

                foreach (var item in listaMostrarArchivos)
                {
                    item.ArchivoPiezaID = archivoPieza.ArchivoPiezaID;
                }
                db.MostrarArchivos.AddRange(listaMostrarArchivos);
                db.SaveChanges();

                string url = "";

                bool esImagen = tipoArchivo.Temp == "imagen_clave" ? true : false;

                var tipoMostrarArchivo = db.TipoMostrarArchivos.FirstOrDefault(a => a.Nombre == "Ficha completa");


                if (esImagen)
                    url = Url.Action("ContenedorImagen", "ArchivoPieza", new { id = archivoPieza.PiezaID, tipoMostrarArchivoID = tipoMostrarArchivo.TipoMostrarArchivoID, esCompleta = true });
                else
                    url = Url.Action("Lista", "ArchivoPieza", new { id = archivoPieza.PiezaID, TipoArchivoID = tipoArchivo.TipoArchivoID, esCompleta = true });

                return Json(new { success = true, url = url, idPieza = archivoPieza.PiezaID, esImagen = esImagen });
            }

            archivoPieza.MostrarArchivos = listaMostrarArchivos;

            return PartialView("_Crear", archivoPieza);
        }

        // GET: ArchivoPieza/Edit/5
        [CustomAuthorize(permiso = "arcEdit")]
        public ActionResult Editar(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArchivoPieza archivoPieza = db.ArchivoPiezas.Find(id);
            if (archivoPieza == null)
            {
                return HttpNotFound();
            }


            foreach (var item in db.TipoMostrarArchivos.Where(a => a.Status).ToList())
            {
                //crear en el archivo objetos temporales

                if (!archivoPieza.MostrarArchivos.Any(a => a.TipoMostrarArchivoID == item.TipoMostrarArchivoID))
                {

                    db.MostrarArchivos.Add(new MostrarArchivo()
                    {
                        ArchivoPiezaID = archivoPieza.ArchivoPiezaID,
                        Status = false,
                        TipoMostrarArchivoID = item.TipoMostrarArchivoID
                    });
                    db.SaveChanges();
                }
            }


            if (!Directory.Exists(Server.MapPath(archivoPieza.TipoArchivo.Ruta)))
                Directory.CreateDirectory(Server.MapPath(archivoPieza.TipoArchivo.Ruta));


            archivoPieza = null;

            archivoPieza = db.ArchivoPiezas.Find(id);


            bool esImagen = archivoPieza.TipoArchivo.Temp == "imagen_clave" ? true : false;

            ViewBag.esImagen = esImagen;


            return PartialView("_Editar", archivoPieza);
        }

        // POST: ArchivoPieza/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permiso = "arcEdit")]
        public ActionResult Editar(ArchivoPieza archivoPieza, HttpPostedFileBase FileArchivo)
        {

            bool eliminarArchivo = false;
            string rutaThumb = "";
            string rutaOriginal = "";

            var listaMostrarArchivos = new List<MostrarArchivo>();

            foreach (var item in db.TipoMostrarArchivos.Where(a => a.Status).ToList())
            {
                string keyStatus = Request.Form["mosArc_" + item.TipoMostrarArchivoID + "_Status"];
                bool valorStatus = keyStatus == "true,false" ? true : false;

                var tempMostrarArchivo = db.MostrarArchivos.Find(item.TipoMostrarArchivoID, archivoPieza.ArchivoPiezaID);

                tempMostrarArchivo.Status = valorStatus;

                db.Entry(tempMostrarArchivo).State = EntityState.Modified;
                //db.SaveChanges();
            }


            //comprobar las extensiones validas
            var tipoArchivo = db.TipoArchivos.Find(archivoPieza.TipoArchivoID);
            archivoPieza.TipoArchivo = tipoArchivo;


            if (FileArchivo != null)
            {
                eliminarArchivo = true;
                archivoPieza.Extension = Path.GetExtension(FileArchivo.FileName);

                if (!tipoArchivo.ExtensionesAceptadas.Replace(" ", "").Split(',').Any(a => "." + a == archivoPieza.Extension))
                {
                    ModelState.AddModelError("", "Archivo no compatible.");
                    eliminarArchivo = false;
                }

                rutaThumb = "~" + archivoPieza.RutaThumb;
                rutaOriginal = "~" + archivoPieza.Ruta;
            }


            if (ModelState.IsValid)
            {

                if (eliminarArchivo)
                {
                    if (!Directory.Exists(Server.MapPath(tipoArchivo.Ruta)))
                        Directory.CreateDirectory(Server.MapPath(tipoArchivo.Ruta));

                    if (!Directory.Exists(Server.MapPath(tipoArchivo.Ruta + "thumb/")))
                        Directory.CreateDirectory(Server.MapPath(tipoArchivo.Ruta + "thumb/"));

                    archivoPieza.NombreArchivo = Guid.NewGuid().ToString();

                    string rutaGuardar = Server.MapPath(archivoPieza.Ruta);

                    FileArchivo.SaveAs(rutaGuardar);

                    FileArchivo.InputStream.Dispose();
                    FileArchivo.InputStream.Close();
                    GC.Collect();

                    //generar la mini
                    switch (archivoPieza.Extension)
                    {
                        //imagenes
                        case ".jpg":
                        case ".png":
                        case ".tiff":
                            Thumbnail mini = new Thumbnail()
                            {
                                OrigenSrc = rutaGuardar,
                                DestinoSrc = Server.MapPath(archivoPieza.RutaThumb),
                                LimiteAnchoAlto = 220
                            };
                            mini.GuardarThumbnail();
                            break;
                        //videos
                        case ".mp4":
                        case ".avi":
                            break;
                    }

                    //Eliminar las imagenes
                    FileInfo infoThumb = new FileInfo(Server.MapPath(rutaThumb));
                    if (infoThumb.Exists) infoThumb.Delete();
                    FileInfo infoOriginal = new FileInfo(Server.MapPath(rutaOriginal));
                    if (infoOriginal.Exists) infoOriginal.Delete();
                }

                db.Entry(archivoPieza).State = EntityState.Modified;
                db.SaveChanges();

                AlertaInfo(string.Format("Se edito archivo {0} <b>{1}</b>", tipoArchivo.Nombre, archivoPieza.Titulo), true);

                string url = "";

                bool esImagen = tipoArchivo.Temp == "imagen_clave" ? true : false;

                var tipoMostrarArchivo = db.TipoMostrarArchivos.FirstOrDefault(a => a.Nombre == "Ficha completa");


                if (esImagen)
                    url = Url.Action("ContenedorImagen", "ArchivoPieza", new { id = archivoPieza.PiezaID, tipoMostrarArchivoID = tipoMostrarArchivo.TipoMostrarArchivoID, esCompleta = true });
                else
                    url = Url.Action("Lista", "ArchivoPieza", new { id = archivoPieza.PiezaID, TipoArchivoID = tipoArchivo.TipoArchivoID, esCompleta = true });

                return Json(new { success = true, url = url, idPieza = archivoPieza.PiezaID, esImagen = esImagen });
            }

            return PartialView("_Editar", archivoPieza);
        }

        // GET: ArchivoPieza/Delete/5
        [CustomAuthorize(permiso = "arcDel")]
        public ActionResult Eliminar(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArchivoPieza archivoPieza = db.ArchivoPiezas.Find(id);
            if (archivoPieza == null)
            {
                return HttpNotFound();
            }

            bool esImagen = archivoPieza.TipoArchivo.Temp == "imagen_clave" ? true : false;

            ViewBag.esImagen = esImagen;

            return PartialView("_Eliminar", archivoPieza);
        }

        // POST: ArchivoPieza/Delete/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(permiso = "arcDel")]
        public ActionResult EliminarConfirmado(Guid id)
        {
            string btnValue = Request.Form["accionx"];

            ArchivoPieza archivoPieza = db.ArchivoPiezas.Find(id);

            bool esImagen = archivoPieza.TipoArchivo.Temp == "imagen_clave" ? true : false;
            var tipoMostrarArchivo = db.TipoMostrarArchivos.FirstOrDefault(a => a.Nombre == "Ficha completa");

            var url = "";
            if (esImagen)
                url = Url.Action("ContenedorImagen", "ArchivoPieza", new { id = archivoPieza.PiezaID, tipoMostrarArchivoID = tipoMostrarArchivo.TipoMostrarArchivoID, esCompleta = true });
            else
                url = Url.Action("Lista", "ArchivoPieza", new { id = archivoPieza.PiezaID, TipoArchivoID = archivoPieza.TipoArchivo.TipoArchivoID, esCompleta = true });

            var piezaID = archivoPieza.PiezaID;

            switch (btnValue)
            {
                case "deshabilitar":
                    archivoPieza.Status = false;
                    db.Entry(archivoPieza).State = EntityState.Modified;
                    db.SaveChanges();
                    AlertaDefault(string.Format("Se deshabilito <b>{0}</b>", archivoPieza.Titulo), true);

                    break;
                case "eliminar":
                    var ruta = archivoPieza.TipoArchivo.Ruta;
                    var rutaThumb = archivoPieza.TipoArchivo.Ruta + "thumb/";

                    var imgRuta = "~" + archivoPieza.RutaThumb;
                    var imgThumbRuta = "~" + archivoPieza.Ruta;

                    var titulo = archivoPieza.Titulo;
                    db.ArchivoPiezas.Remove(archivoPieza);
                    db.SaveChanges();
                    
                     if (!Directory.Exists(Server.MapPath(ruta)))
                        Directory.CreateDirectory(Server.MapPath(ruta));

                    if (!Directory.Exists(Server.MapPath(rutaThumb)))
                        Directory.CreateDirectory(Server.MapPath(rutaThumb));

                    //------------ Eliminar el archivo normal y el thumbnail

                    FileInfo infoThumb = new FileInfo(Server.MapPath(imgThumbRuta));
                    if (infoThumb.Exists) infoThumb.Delete();

                    FileInfo infoOriginal = new FileInfo(Server.MapPath(imgRuta));
                    if (infoOriginal.Exists) infoOriginal.Delete();

                    AlertaDanger(string.Format("Se elimino <b>{0}</b>", titulo), true);

                    break;
                default:
                    AlertaDefault(string.Format("Sin accion seleccionada."), true);
                    break;

            }



            
            return Json(new { success = true, url = url, idPieza = piezaID, esImagen = esImagen });

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
