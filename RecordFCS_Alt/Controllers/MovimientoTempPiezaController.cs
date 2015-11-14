using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RecordFCS_Alt.Models;
using System.Text.RegularExpressions;
using RecordFCS_Alt.Models.ViewModels;

namespace RecordFCS_Alt.Controllers
{
    public class MovimientoTempPiezaController : Controller
    {
        private RecordFCSContext db = new RecordFCSContext();

        // GET: MovimientoTempPieza
        public ActionResult Index()
        {
            var movimientoTempPiezas = db.MovimientoTempPiezas.Include(m => m.MovimientoTemp).Include(m => m.Pieza);
            return View(movimientoTempPiezas.ToList());
        }

        //// GET: MovimientoTempPieza/Details/5
        //public ActionResult Details(Guid? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    MovimientoTempPieza movimientoTempPieza = db.MovimientoTempPiezas.Find(id);
        //    if (movimientoTempPieza == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(movimientoTempPieza);
        //}

        // GET: MovimientoTempPieza/Create
        public ActionResult FormCrear(Guid id)
        {
            Session["listaBuscar_" + id] = new List<Item_MovPieza>();
            Session["listaAceptar_" + id] = new List<Item_MovPieza>();
            Session["listaError_" + id] = new List<Item_MovPieza>();

            var tipoAttGuion = db.TipoAtributos.FirstOrDefault(a => a.Temp == "guion_clave");
            var listaGuiones = tipoAttGuion.ListaValores.Where(a => a.Status && a.AtributoPiezas.Count > 0).Select(a => new { Nombre = $"{a.Valor} ({a.AtributoPiezas.Count})", GuionID = a.ListaValorID }).OrderBy(a => a.Nombre);
            ViewBag.GuionID = new SelectList(listaGuiones, "GuionID", "Nombre");

            var listaLetras = db.LetraFolios.Select(a => new { a.LetraFolioID, Nombre = a.Nombre, a.Status }).Where(a => a.Status).OrderBy(a => a.Nombre);
            ViewBag.LetraFolioID = new SelectList(listaLetras, "LetraFolioID", "Nombre", listaLetras.FirstOrDefault().LetraFolioID);

            ViewBag.MovID = id;

            var mov = db.MovimientosTemp.Find(id);

            if (mov.MovimientoTempPiezas.Count > 0)
            {
                var tipoMostrarArchivo = db.TipoMostrarArchivos.FirstOrDefault(a => a.Nombre == "Guion");

                if (tipoMostrarArchivo == null)
                {
                    tipoMostrarArchivo = new TipoMostrarArchivo()
                    {
                        TipoMostrarArchivoID = Guid.NewGuid(),
                        Nombre = "Guion",
                        Status = true,
                        Descripcion = "Ficha de movimientos"
                    };
                    db.TipoMostrarArchivos.Add(tipoMostrarArchivo);
                    db.SaveChanges();
                }

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
                    obj.EsUltimo = item.Pieza.MovimientoTempPiezas.Where(a => a.Orden > item.Orden && !a.EnError).Count() > 0 ? false : true;


                    lista.Add(obj);

                }
                Session["listaBuscar_" + id] = lista;

                foreach (var item in lista.ToList())
                {
                    ValidarPieza(item.PiezaID, mov.UbicacionOrigenID, mov.MovimientoTempID);
                }
            }





            return PartialView("_FormCrear");
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult BuscarPiezas(int? LetraFolioID, Guid? GuionID, bool esGuion, Guid MovimientoID, string NoInventarios = "", bool CargaCompleta = false)
        {
            //var mov = db.MovimientosTemp.Find(MovimientoID);


            Session["listaBuscar_" + MovimientoID] = new List<Item_MovPieza>();

            //var listaPiezasEnMov = db.MovimientoTempPiezas.Where(a => a.MovimientoTempID == MovimientoID).ToList();
            bool resultado = false;
            List<Pieza> listaPiezas = new List<Pieza>();
            /*
                listas:
                lista_tabla
                lista_temporal
                lista_validas
                lista_invalidas
                */

            if (esGuion && GuionID != Guid.Empty || GuionID != null)
            {
                var listaValor = db.ListaValores.Find(GuionID);
                listaPiezas = listaValor.AtributoPiezas.Select(a => a.Pieza).ToList();
            }
            else if (!String.IsNullOrWhiteSpace(NoInventarios) && (LetraFolioID != 0 || LetraFolioID != null))
            {
                var letra = db.LetraFolios.Find(LetraFolioID);

                if (letra != null)
                {
                    // 1 -3 , 54, 32, 7 -  6
                    NoInventarios = Regex.Replace(NoInventarios.Trim(), @"\s+", " ");

                    //listaPiezas = db.Piezas.Where(a => a.Obra.LetraFolioID == letra.LetraFolioID).ToList();
                    List<int> listaClaves = new List<int>();

                    foreach (var clavesTemp in NoInventarios.Split(','))
                    {
                        if (clavesTemp.Contains("-"))
                        {
                            string[] claveTemp = clavesTemp.Split('-');
                            var claveInicio = Convert.ToInt32(claveTemp[0]);
                            var claveFinal = Convert.ToInt32(claveTemp[1]);

                            if (claveInicio > claveFinal)
                            {
                                int temp = 0;
                                temp = claveInicio;
                                claveInicio = claveFinal;
                                claveFinal = temp;
                            }

                            for (int i = claveInicio; i <= claveFinal; i++)
                                listaClaves.Add(i);
                        }
                        else
                        {
                            listaClaves.Add(Convert.ToInt32(clavesTemp));
                        }
                    }
                    listaPiezas = db.Piezas.Where(a => a.Obra.LetraFolioID == letra.LetraFolioID && listaClaves.Contains(a.Obra.NumeroFolio)).ToList();
                }
            }


            if (CargaCompleta) //Cargar obras completas
            {
                //var listaPiezasTemp = listaPiezas.Select(a => a.ObraID);
                //listaPiezas = db.Piezas.Where(a => listaPiezas.Any(b => b.ObraID == a.ObraID)).ToList();

            }
            else //cargar solo
            {
                listaPiezas = listaPiezas.Where(a => a.TipoPieza.EsPrincipal).ToList();
            }




            if (listaPiezas.Count > 0)
            {
                var listaEnMov = db.MovimientoTempPiezas.Where(a => a.MovimientoTempID == MovimientoID).ToList();//Session["listaEnMov_" + MovimientoID] == null ? new List<MovimientoTempPieza>() : (List<MovimientoTempPieza>)Session["listaEnMov_" + MovimientoID];

                resultado = true;

                /*
                listas:
                lista_tabla
                lista_temporal
                lista_validas
                lista_invalidas
                */
                //var lista = Session["lista_temporal"] == null ? new List<Guid>() : (List<Guid>)Session[listaNombre];

                var tipoMostrarArchivo = db.TipoMostrarArchivos.FirstOrDefault(a => a.Nombre == "Guion");

                if (tipoMostrarArchivo == null)
                {
                    tipoMostrarArchivo = new TipoMostrarArchivo()
                    {
                        TipoMostrarArchivoID = Guid.NewGuid(),
                        Nombre = "Guion",
                        Status = true,
                        Descripcion = "Ficha de movimientos"
                    };
                    db.TipoMostrarArchivos.Add(tipoMostrarArchivo);
                    db.SaveChanges();
                }

                //TipoArchivo tipoArchivo = db.TipoArchivos.FirstOrDefault(a => a.Temp == "imagen_clave");
                var TipoAttTitulo = db.TipoAtributos.FirstOrDefault(a => a.Temp == "titulo");

                var lista = new List<Item_MovPieza>();

                foreach (var item in listaPiezas.OrderBy(a => a.Obra.LetraFolio.Nombre).ThenBy(a => a.Obra.NumeroFolio).ThenBy(a => a.SubFolio).ToList())
                {

                    var obj = new Item_MovPieza();


                    obj.ObraID = item.ObraID;
                    obj.FolioObra = item.Obra.LetraFolio.Nombre + item.Obra.NumeroFolio;
                    obj.PiezaID = item.PiezaID;
                    obj.FolioPieza = item.ImprimirFolio();
                    obj.UbicacionID = item.UbicacionID;
                    //var imagen = item.ArchivosPiezas.Where(b => b.Status && b.TipoArchivoID == tipoArchivo.TipoArchivoID && b.MostrarArchivos.Any(c => c.TipoMostrarArchivoID == tipoMostrarArchivo.TipoMostrarArchivoID && c.Status)).OrderBy(b => b.Orden).FirstOrDefault();
                    //obj.Imagen = imagen == null ? "" : imagen.RutaThumb;
                    var autor = item.AutorPiezas.OrderBy(b => b.Orden).FirstOrDefault(b => b.esPrincipal && b.Status);
                    obj.Autor = autor == null ? "Sin autor" : autor.Autor == null ? "Sin autor" : autor.Autor.Seudonimo + " " + autor.Autor.Nombre + " " + autor.Autor.ApellidoPaterno + " " + autor.Autor.ApellidoMaterno;
                    var titulo = item.AtributoPiezas.FirstOrDefault(b => b.Atributo.TipoAtributoID == TipoAttTitulo.TipoAtributoID);
                    obj.Titulo = titulo == null ? "Sin titulo" : titulo.Valor;
                    //obj.TotalPiezas = item.PiezasHijas.Count();

                    if (listaEnMov.Any(a => a.PiezaID == obj.PiezaID))
                    {
                        var tempObj = listaEnMov.FirstOrDefault(a => a.PiezaID == obj.PiezaID);

                        obj.EsUltimo = tempObj.Pieza.MovimientoTempPiezas.Where(a => a.Orden > tempObj.Orden && !a.EnError).Count() > 0 ? true : false;

                        obj.Comentario = tempObj.Comentario;
                        obj.EnError = false;
                        obj.EsPendiente = tempObj.EsPendiente;
                        obj.SeMovio = tempObj.SeMovio;
                        obj.ExisteEnMov = true;
                    }
                    else
                    {
                        obj.EsUltimo = true;
                        obj.Comentario = "";
                        obj.EnError = false;
                        obj.EsPendiente = true;
                        obj.SeMovio = false;
                        obj.ExisteEnMov = false;
                    }

                    lista.Add(obj);
                }

                lista = lista.OrderBy(a => a.FolioPieza).ToList();

                Session["listaBuscar_" + MovimientoID] = lista;
                // generar la lista de piezas sin validar

                //validar la lista de busqueda
                var url = Url.Action("ValidarPieza", "MovimientoTempPieza", new { MovimientoID = MovimientoID });

                return Json(new { success = resultado, url = url, lista = lista }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = resultado }, JsonRequestBehavior.AllowGet);
        }

        public string Test(Guid PiezaID)
        {
            return PiezaID.ToString();
        }


        public void ValidarPieza(Guid PiezaID, Guid? UbicacionID, Guid? MovimientoID, string Tipo = "busqueda")
        {
            //definir listas
            var listaBuscar = Session["listaBuscar_" + MovimientoID] == null ? new List<Item_MovPieza>() : (List<Item_MovPieza>)Session["listaBuscar_" + MovimientoID];

            var listaAceptar = Session["listaAceptar_" + MovimientoID] == null ? new List<Item_MovPieza>() : (List<Item_MovPieza>)Session["listaAceptar_" + MovimientoID];
            var listaError = Session["listaError_" + MovimientoID] == null ? new List<Item_MovPieza>() : (List<Item_MovPieza>)Session["listaError_" + MovimientoID];

            MovimientoTemp mov = db.MovimientosTemp.Find(MovimientoID);

            Item_MovPieza piezaTemp = listaBuscar.FirstOrDefault(a => a.PiezaID == PiezaID);
            piezaTemp.Comentario = "";

            bool enError = false;

            //Validar que la pieza este disponible
            //pieza validar que la pieza no este Pendiente y sin Error y sin Mover en ningun otro movimiento excepto este
            var listaPiezaMov = db.MovimientoTempPiezas.Where(a => a.PiezaID == PiezaID && a.EsPendiente && !a.EnError && a.MovimientoTempID != MovimientoID).Select(a => a.MovimientoTemp.Folio).OrderBy(a => a).ToList();

            //validar que pieza no este asignada en otro movimiento
            if (listaPiezaMov.Count > 0)
            {
                piezaTemp.Comentario = "Asignada en movimiento(s): ";

                foreach (var item in listaPiezaMov)
                    piezaTemp.Comentario += $" [{item}]";

                piezaTemp.Comentario += ". ";
                enError = true;
            }

            //validar que la ubicacion de la pieza sea la misma que este movimiento
            //puede haber piezas que UbicacionId sea null y es valida

            if (piezaTemp.UbicacionID != null)
            {
                if (UbicacionID != piezaTemp.UbicacionID)
                {
                    piezaTemp.Comentario += "No comparte la misma ubicación origen.";
                    enError = true;
                }
            }

            //Eliminar de todas las listas la obra
            foreach (var item in listaBuscar.Where(a => a.PiezaID == piezaTemp.PiezaID).ToList())
                listaBuscar.Remove(item);

            foreach (var item in listaAceptar.Where(a => a.PiezaID == piezaTemp.PiezaID).ToList())
                listaAceptar.Remove(item);

            foreach (var item in listaError.Where(a => a.PiezaID == piezaTemp.PiezaID).ToList())
                listaError.Remove(item);

            //si obra es valida

            //ignorar si en Obra


            if (piezaTemp.ExisteEnMov)
            {
                if (piezaTemp.EsPendiente)
                {
                    if (enError)
                    {
                        piezaTemp.EnError = true;
                        listaError.Add(piezaTemp);
                    }
                    else
                    {
                        piezaTemp.EnError = false;
                        listaAceptar.Add(piezaTemp);
                    }
                }
                else
                {
                    piezaTemp.EnError = false;
                    listaAceptar.Add(piezaTemp);
                }
            }
            else
            {
                if (enError)
                {
                    piezaTemp.EnError = true;
                    listaError.Add(piezaTemp);
                }
                else
                {
                    piezaTemp.EnError = false;
                    listaAceptar.Add(piezaTemp);
                }
            }

            

            ////solo para busquedas
            //if (Tipo.ToLower() == "busqueda")
            //{
            //    //si la pieza tiene piezas adicionales preguntar si se desea agregarlas
            //    if (piezaTemp.TotalPiezas > 0)//url = Url.Action("Pregunta_PiezasAdicionales", "MovimientoTempPieza", new { id = piezaTemp.PiezaID });
            //    {
            //        url = Url.Action("Pregunta_PiezasAdicionales", "MovimientoTempPieza", new { id = piezaTemp.PiezaID });
            //        preguntar = true;
            //    }

            //}


            //Session["listaBuscar_" + MovimientoID] = listaBuscar;
            //Session["listaAceptar_" + MovimientoID] = null;
            //Session["listaError_" + MovimientoID] = null;
            //Session["listaBuscar_" + MovimientoID] = null;
            Session["listaAceptar_" + MovimientoID] = listaAceptar.OrderBy(a => a.FolioPieza).ToList();
            Session["listaError_" + MovimientoID] = listaError.OrderBy(a => a.FolioPieza).ToList();
            Session["listaBuscar_" + MovimientoID] = listaBuscar;


            //return Json(new { success = true, piezaID = piezaTemp.PiezaID },JsonRequestBehavior.AllowGet);
        }


        public ActionResult RefrescarLista(Guid MovimientoID, string NombreLista = "listaAceptar")
        {
            var listaError = Session["listaError_" + MovimientoID] == null ? new List<Item_MovPieza>() : ((List<Item_MovPieza>)Session["listaError_" + MovimientoID]).OrderBy(a => a.FolioPieza).ToList();
            var listaAceptar = Session["listaAceptar_" + MovimientoID] == null ? new List<Item_MovPieza>() : ((List<Item_MovPieza>)Session["listaAceptar_" + MovimientoID]).OrderBy(a => a.FolioPieza).ToList();

            int i = 0;
            foreach (var item in listaAceptar)
            {
                item.Indice = i;
                i++;
            }
            foreach (var item in listaError)
            {
                item.Indice = i;
                i++;
            }

            Session["listaError_" + MovimientoID] = listaError;
            Session["listaAceptar_" + MovimientoID] = listaAceptar;

            ViewBag.NombreLista = NombreLista;

            switch (NombreLista)
            {
                case "listaError":
                    return PartialView("_ListaFormCrear", listaError);
                default:
                    return PartialView("_ListaFormCrear", listaAceptar);
            }
        }


        public void EliminarPieza(Guid PiezaID, Guid MovimientoID)
        {
            //definir listas
            var listaAceptar = Session["listaAceptar_" + MovimientoID] == null ? new List<Item_MovPieza>() : (List<Item_MovPieza>)Session["listaAceptar_" + MovimientoID];
            var listaBuscar = Session["listaBuscar_" + MovimientoID] == null ? new List<Item_MovPieza>() : (List<Item_MovPieza>)Session["listaBuscar_" + MovimientoID];
            var listaError = Session["listaError_" + MovimientoID] == null ? new List<Item_MovPieza>() : (List<Item_MovPieza>)Session["listaError_" + MovimientoID];

            //Eliminar de todas las listas la obra
            foreach (var item in listaBuscar.Where(a => a.PiezaID == PiezaID).ToList())
                listaBuscar.Remove(item);

            foreach (var item in listaAceptar.Where(a => a.PiezaID == PiezaID).ToList())
                listaAceptar.Remove(item);

            foreach (var item in listaError.Where(a => a.PiezaID == PiezaID).ToList())
                listaError.Remove(item);

            Session["listaAceptar_" + MovimientoID] = listaAceptar.OrderBy(a => a.FolioPieza).ToList();
            Session["listaError_" + MovimientoID] = listaError.OrderBy(a => a.FolioPieza).ToList();
            Session["listaBuscar_" + MovimientoID] = listaBuscar;

        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "MovimientoTempID,PiezaID,Comentario,Status")] MovimientoTempPieza movimientoTempPieza)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        movimientoTempPieza.MovimientoTempID = Guid.NewGuid();
        //        db.MovimientoTempPiezas.Add(movimientoTempPieza);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    ViewBag.MovimientoTempID = new SelectList(db.MovimientosTemp, "MovimientoTempID", "ColeccionTexto", movimientoTempPieza.MovimientoTempID);
        //    ViewBag.PiezaID = new SelectList(db.Piezas, "PiezaID", "SubFolio", movimientoTempPieza.PiezaID);
        //    return View(movimientoTempPieza);
        //}

        ////GET: MovimientoTempPieza/Edit/5
        //public ActionResult Edit(Guid? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    MovimientoTempPieza movimientoTempPieza = db.MovimientoTempPiezas.Find(id);
        //    if (movimientoTempPieza == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.MovimientoTempID = new SelectList(db.MovimientosTemp, "MovimientoTempID", "ColeccionTexto", movimientoTempPieza.MovimientoTempID);
        //    ViewBag.PiezaID = new SelectList(db.Piezas, "PiezaID", "SubFolio", movimientoTempPieza.PiezaID);
        //    return View(movimientoTempPieza);
        //}

        //// POST: MovimientoTempPieza/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "MovimientoTempID,PiezaID,Comentario,Status")] MovimientoTempPieza movimientoTempPieza)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(movimientoTempPieza).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.MovimientoTempID = new SelectList(db.MovimientosTemp, "MovimientoTempID", "ColeccionTexto", movimientoTempPieza.MovimientoTempID);
        //    ViewBag.PiezaID = new SelectList(db.Piezas, "PiezaID", "SubFolio", movimientoTempPieza.PiezaID);
        //    return View(movimientoTempPieza);
        //}

        //// GET: MovimientoTempPieza/Delete/5
        //public ActionResult Delete(Guid? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    MovimientoTempPieza movimientoTempPieza = db.MovimientoTempPiezas.Find(id);
        //    if (movimientoTempPieza == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(movimientoTempPieza);
        //}

        //// POST: MovimientoTempPieza/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(Guid id)
        //{
        //    MovimientoTempPieza movimientoTempPieza = db.MovimientoTempPiezas.Find(id);
        //    db.MovimientoTempPiezas.Remove(movimientoTempPieza);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

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
