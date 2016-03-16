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
using System.Threading.Tasks;
using System.Data.Entity;

namespace RecordFCS_Alt.Controllers
{
    public class MovimientoTempPiezaController : AsyncController
    {
        private RecordFCSContext db = new RecordFCSContext();

        #region Cargar Formulario para modificar las piezas en el movimiento

        // GET: MovimientoTempPieza/Create
        public async Task<ActionResult> FormCrear(Guid id)
        {

            db = new RecordFCSContext();
            db.Configuration.ValidateOnSaveEnabled = false;
            db.Configuration.AutoDetectChangesEnabled = false;

            #region Inicializar

            //Variable Movimiento ID
            ViewBag.MovID = id;

            //Listas guardadas en variables de sesion
            var listaBuscar = new List<Item_MovPieza>();
            var listaAceptar = new List<Item_MovPieza>();
            var listaError = new List<Item_MovPieza>();

            Session["listaBuscar_" + id] = listaBuscar;
            Session["listaAceptar_" + id] = listaAceptar;
            Session["listaError_" + id] = listaError;

            //atributos para el menu de carga de piezas
            var tipoAttGuion = db.TipoAtributos.FirstOrDefault(a => a.Temp == "guion_clave");
            var listaGuiones = tipoAttGuion.ListaValores.Where(a => a.Status && a.AtributoPiezas.Count > 0).Select(a => new { Nombre = a.Valor + " (" + a.AtributoPiezas.Count + ")", GuionID = a.ListaValorID }).OrderBy(a => a.Nombre);
            ViewBag.GuionID = new SelectList(listaGuiones, "GuionID", "Nombre");

            var listaLetras = db.LetraFolios.Select(a => new { a.LetraFolioID, Nombre = a.Nombre, a.Status }).Where(a => a.Status).OrderBy(a => a.Nombre);
            ViewBag.LetraFolioID = new SelectList(listaLetras, "LetraFolioID", "Nombre", listaLetras.FirstOrDefault().LetraFolioID);

            #endregion

            //Buscar el movimiento
            var mov = db.MovimientosTemp.Find(id);

            ViewBag.EstadoMovimiento = mov.EstadoMovimiento;




            //llenar las listas

            #region Seleccionar Imagenes mostradas en Guion
            //var tipoMostrarArchivo = db.TipoMostrarArchivos.FirstOrDefault(a => a.Nombre == "Guion");

            //if (tipoMostrarArchivo == null)
            //{
            //    tipoMostrarArchivo = new TipoMostrarArchivo()
            //    {
            //        TipoMostrarArchivoID = Guid.NewGuid(),
            //        Nombre = "Guion",
            //        Status = true,
            //        Descripcion = "Ficha de movimientos"
            //    };
            //    db.TipoMostrarArchivos.Add(tipoMostrarArchivo);
            //    db.SaveChanges();
            //}

            //TipoArchivo tipoArchivo = db.TipoArchivos.FirstOrDefault(a => a.Temp == "imagen_clave");

            #endregion

            //Atributo titulo
            var TipoAttTitulo = db.TipoAtributos.FirstOrDefault(a => a.Temp == "titulo");

            //lista de piezas guardadas en el movimiento

            db = new RecordFCSContext();
            db.Configuration.ValidateOnSaveEnabled = false;
            db.Configuration.AutoDetectChangesEnabled = false;


            //var listaOrdenada = db.MovimientoTempPiezas.Where(a=> a.MovimientoTempID == mov.MovimientoTempID).OrderBy(a => a.Pieza.Obra.LetraFolio.Nombre).ThenBy(a => a.Pieza.Obra.NumeroFolio).ThenBy(a => a.Pieza.SubFolio).ToList();

            var lista = db.MovimientoTempPiezas
                        .Where(a => a.MovimientoTempID == mov.MovimientoTempID)
                        .OrderBy(a => a.Pieza.Obra.LetraFolio.Nombre)
                        .ThenBy(a => a.Pieza.Obra.NumeroFolio)
                        .ThenBy(a => a.Pieza.SubFolio)
                        .Select(a => new Item_MovPieza()
                        {
                            ObraID = a.Pieza.ObraID,
                            FolioObra = a.Pieza.Obra.LetraFolio.Nombre + a.Pieza.Obra.NumeroFolio,
                            FolioPieza = a.FolioPieza,
                            PiezaID = a.PiezaID,
                            UbicacionID = a.Pieza.UbicacionID,
                            EnError = a.EnError,
                            EsPendiente = a.EsPendiente,
                            SeMovio = a.SeMovio,
                            ExisteEnMov = true,
                            Comentario = a.Comentario,
                            EsUltimo = a.Pieza.MovimientoTempPiezas.Where(b => b.Orden > a.Orden && !a.EnError).Count() > 0 ? false : true
                        })
                        .ToList();

            ////Extraer los campos que necesitan mas logica
            //foreach (var item in lista)
            //{
            //    item.FolioPieza = db.Piezas.FirstOrDefault(a => a.PiezaID == item.PiezaID).ImprimirFolio();
            //}



            #region No se usa ya

            //foreach (var obj in lista)
            //{





            //    ////var obj = new Item_MovPieza();

            //    ////obj.ObraID = item.Pieza.ObraID;
            //    ////obj.FolioObra = item.Pieza.Obra.LetraFolio.Nombre + item.Pieza.Obra.NumeroFolio;
            //    ////obj.PiezaID = item.PiezaID;
            //    ////obj.FolioPieza = item.Pieza.ImprimirFolio();
            //    ////obj.UbicacionID = item.Pieza.UbicacionID;
            //    //////var imagen = item.ArchivosPiezas.Where(b => b.Status && b.TipoArchivoID == tipoArchivo.TipoArchivoID && b.MostrarArchivos.Any(c => c.TipoMostrarArchivoID == tipoMostrarArchivo.TipoMostrarArchivoID && c.Status)).OrderBy(b => b.Orden).FirstOrDefault();
            //    //////obj.Imagen = imagen == null ? "" : imagen.RutaThumb;
            //    ////var autor = item.Pieza.AutorPiezas.OrderBy(b => b.Orden).FirstOrDefault(b => b.esPrincipal && b.Status);
            //    ////obj.Autor = autor == null ? "Sin autor" : autor.Autor == null ? "Sin autor" : autor.Autor.Seudonimo + " " + autor.Autor.Nombre + " " + autor.Autor.ApellidoPaterno + " " + autor.Autor.ApellidoMaterno; var titulo = item.Pieza.AtributoPiezas.FirstOrDefault(b => b.Atributo.TipoAtributoID == TipoAttTitulo.TipoAtributoID);
            //    ////obj.Titulo = titulo == null ? "Sin titulo" : titulo.Valor;
            //    //////obj.TotalPiezas = item.PiezasHijas.Count();
            //    ////obj.EnError = item.EnError;
            //    ////obj.EsPendiente = item.EsPendiente;
            //    ////obj.SeMovio = item.SeMovio;
            //    ////obj.ExisteEnMov = true;
            //    ////obj.Comentario = item.Comentario;
            //    ////obj.EsUltimo = item.Pieza.MovimientoTempPiezas.Where(a => a.Orden > item.Orden && !a.EnError).Count() > 0 ? false : true;


            //    ////if (!obj.EnError)
            //    ////    listaAceptar.Add(obj);
            //    ////else
            //    ////    listaError.Add(obj);

            //}

            #endregion

            listaError.AddRange(lista.Where(a => a.EnError).ToList());
            listaAceptar.AddRange(lista.Where(a => !a.EnError).ToList());

            //listaBuscar = (List<Item_MovPieza>)Session["listaBuscar_" + id];
            //listaAceptar = (List<Item_MovPieza>)Session["listaAceptar_" + id];
            //listaError = (List<Item_MovPieza>)Session["listaError_" + id];

            return PartialView("_FormCrear");
        }

        #endregion


        public JsonResult BuscarPiezas(int? LetraFolioID, Guid? GuionID, bool esGuion, Guid MovimientoID, string NoInventarios = "", bool CargaCompleta = false)
        {
            bool resultado = false;

            Session["listaBuscar_" + MovimientoID] = new List<Item_MovPieza>();

            List<Pieza> listaPiezas = new List<Pieza>();

            if (esGuion && GuionID != Guid.Empty || GuionID != null)
            {
                //Carga el guion seleccionado
                var listaValor = db.ListaValores.Find(GuionID);
                listaPiezas = listaValor.AtributoPiezas.Select(a => a.Pieza).ToList();
            }
            else if (!String.IsNullOrWhiteSpace(NoInventarios) && (LetraFolioID != 0 || LetraFolioID != null))
            {
                //Realiza la busqueda por inventario
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



            //Crear la estructura de Item Movimiento Pieza
            if (listaPiezas.Count > 0)
            {
                resultado = true;

                //var listaEnMov = db.MovimientoTempPiezas.Where(a => a.MovimientoTempID == MovimientoID).ToList();//Session["listaEnMov_" + MovimientoID] == null ? new List<MovimientoTempPieza>() : (List<MovimientoTempPieza>)Session["listaEnMov_" + MovimientoID];

                //var lista = Session["lista_temporal"] == null ? new List<Guid>() : (List<Guid>)Session[listaNombre];

                var tipoMostrarArchivo = db.TipoMostrarArchivos.FirstOrDefault(a => a.Nombre == "Guion");

                //TipoArchivo tipoArchivo = db.TipoArchivos.FirstOrDefault(a => a.Temp == "imagen_clave");
                var TipoAttTitulo = db.TipoAtributos.FirstOrDefault(a => a.Temp == "titulo");

                var lista = new List<Item_MovPieza>();

                foreach (var item in listaPiezas.OrderBy(a => a.Obra.LetraFolio.Nombre).ThenBy(a => a.Obra.NumeroFolio).ThenBy(a => a.SubFolio).ToList())
                {

                    var obj = new Item_MovPieza()
                    {
                        ObraID = item.ObraID,
                        PiezaID = item.PiezaID,
                        UbicacionID = item.UbicacionID,
                    };


                    obj.FolioObra = item.Obra.LetraFolio.Nombre + item.Obra.NumeroFolio;
                    obj.FolioPieza = item.ImprimirFolio();

                    //var imagen = item.ArchivosPiezas.Where(b => b.Status && b.TipoArchivoID == tipoArchivo.TipoArchivoID && b.MostrarArchivos.Any(c => c.TipoMostrarArchivoID == tipoMostrarArchivo.TipoMostrarArchivoID && c.Status)).OrderBy(b => b.Orden).FirstOrDefault();
                    //obj.Imagen = imagen == null ? "" : imagen.RutaThumb;

                    ////Extraer el autor
                    //try
                    //{
                    //    var autor = item.AutorPiezas.OrderBy(b => b.Orden).FirstOrDefault(b => b.esPrincipal && b.Status);
                    //    obj.Autor = autor == null ? "Sin autor" : autor.Autor == null ? "Sin autor" : autor.Autor.Seudonimo + " " + autor.Autor.Nombre + " " + autor.Autor.ApellidoPaterno + " " + autor.Autor.ApellidoMaterno;
                    //}
                    //catch (Exception)
                    //{
                    //    obj.Autor = "Sin autor";
                    //}

                    ////Extraer el titulo
                    //try
                    //{
                    //    var titulo = item.AtributoPiezas.FirstOrDefault(b => b.Atributo.TipoAtributoID == TipoAttTitulo.TipoAtributoID);
                    //    obj.Titulo = titulo.Valor;
                    //}
                    //catch (Exception)
                    //{
                    //    obj.Titulo = "Sin titulo";
                    //}

                    //obj.TotalPiezas = item.PiezasHijas.Count();

                    //Buscar si la pieza ya esta en el movimiento
                    if (db.MovimientoTempPiezas.Where(a => a.PiezaID == obj.PiezaID && a.MovimientoTempID == MovimientoID).Count() > 0)
                    {
                        var piezaMovTemp = db.MovimientoTempPiezas.FirstOrDefault(a => a.PiezaID == obj.PiezaID && a.MovimientoTempID == MovimientoID);

                        obj.EsUltimo = piezaMovTemp.Pieza.MovimientoTempPiezas.Where(a => a.Orden > piezaMovTemp.Orden && !a.EnError).Count() > 0 ? true : false;

                        obj.Comentario = piezaMovTemp.Comentario;
                        obj.EnError = piezaMovTemp.EnError;
                        obj.EsPendiente = piezaMovTemp.EsPendiente;
                        obj.SeMovio = piezaMovTemp.SeMovio;
                        obj.ExisteEnMov = true;
                        //obj.Indice = piezaMovTemp.Orden;
                    }
                    else
                    {
                        obj.EsUltimo = false;
                        obj.Comentario = "";
                        obj.EnError = false;
                        obj.EsPendiente = false;
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


        public void ValidarPieza(Guid PiezaID, Guid? UbicacionID, Guid? MovimientoID, string Tipo = "busqueda")
        {
            #region Listas
            //definir las listas

            var listaBuscar = Session["listaBuscar_" + MovimientoID] == null ? new List<Item_MovPieza>() : (List<Item_MovPieza>)Session["listaBuscar_" + MovimientoID];
            var listaAceptar = Session["listaAceptar_" + MovimientoID] == null ? new List<Item_MovPieza>() : (List<Item_MovPieza>)Session["listaAceptar_" + MovimientoID];
            var listaError = Session["listaError_" + MovimientoID] == null ? new List<Item_MovPieza>() : (List<Item_MovPieza>)Session["listaError_" + MovimientoID];

            #endregion

            #region Declaracion

            MovimientoTemp mov = db.MovimientosTemp.Find(MovimientoID);

            Item_MovPieza piezaTemp = null;


            string comentario = "";
            bool enError = false;

            #endregion

            #region Buscar la Pieza en las listas
            //Buscar la piezaID en alguna lista para validarla

            switch (Tipo)
            {
                case "recargaError":
                    piezaTemp = listaError.FirstOrDefault(a => a.PiezaID == PiezaID);
                    break;

                case "recargaAceptar":
                    piezaTemp = listaAceptar.FirstOrDefault(a => a.PiezaID == PiezaID);
                    break;

                case "busqueda":
                default:
                    piezaTemp = listaBuscar.FirstOrDefault(a => a.PiezaID == PiezaID);
                    break;
            }

            piezaTemp.Comentario = "";

            #endregion



            #region Validacion 1: Asignada en movimientos


            #region Buscar donde sea Pendiente y sin errores

            //lista de movimientos INT, Buscar la pieza en todos los movimientos excepto en este, donde sea pendiente y no tenga errores.
            List<int> listaMov_ID =
                db.MovimientoTempPiezas
                .Where(a =>
                    a.PiezaID == PiezaID &&
                    a.MovimientoTempID != MovimientoID &&
                    a.EsPendiente && !a.EnError
                    )
                .Select(a => a.MovimientoTemp.Folio).ToList();

            #endregion

            #region Buscar en Concluido sin validar

            listaMov_ID.AddRange(
                db.MovimientoTempPiezas.Where(a => a.PiezaID == PiezaID && a.MovimientoTempID != MovimientoID &&
                a.MovimientoTemp.EstadoMovimiento == EstadoMovimientoTemp.Concluido_SinValidar &&
                a.SeMovio).Select(a => a.MovimientoTemp.Folio).ToList()
                );

            #endregion

            #region Buscar en Cancelado

            listaMov_ID.AddRange(
                db.MovimientoTempPiezas.Where(a => a.PiezaID == PiezaID && a.MovimientoTempID != MovimientoID &&
                a.MovimientoTemp.EstadoMovimiento == EstadoMovimientoTemp.Cancelado &&
                a.SeMovio).Select(a => a.MovimientoTemp.Folio).ToList()
                );

            #endregion

            #region Buscar en Retornado

            listaMov_ID.AddRange(
                db.MovimientoTempPiezas.Where(a => a.PiezaID == PiezaID && a.MovimientoTempID != MovimientoID &&
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
                piezaTemp.Comentario = "Asignada en:" + comentario + ". ";
                enError = true;
            }

            #endregion

            #region (Sin utilizar)


            ////pieza validar que la pieza no este Pendiente y sin Error y sin Mover en ningun otro movimiento excepto este
            //var listaPiezaMov = db.MovimientoTempPiezas.Where(a => a.PiezaID == PiezaID && 
            //    a.MovimientoTempID != MovimientoID && 
            //    ((a.MovimientoTemp.EstadoMovimiento == EstadoMovimientoTemp.Concluido_SinValidar) || (a.EsPendiente && !a.EnError))).Select(a => a.MovimientoTemp.Folio).OrderBy(a => a).ToList();

            ////validar que pieza no este asignada en otro movimiento
            //if (listaPiezaMov.Count > 0)
            //{
            //    piezaTemp.Comentario = "Asignada en movimiento(s): ";

            //    foreach (var item in listaPiezaMov)
            //        piezaTemp.Comentario += "[" + item + "]";

            //    piezaTemp.Comentario += ". ";
            //    enError = true;
            //}


            ////validar que pieza no este en algun movimiento Concluido_SinValidar
            //var listaPiezaMovSinValidar = db.MovimientoTempPiezas.Where(a => a.PiezaID == PiezaID &&  && a.MovimientoTempID != MovimientoID).Select(a=> a.MovimientoTemp.Folio).OrderBy(a=> a).ToList();
            //if (listaPiezaMovSinValidar.Count > 0)
            //{
            //    piezaTemp.Comentario += "Aginada en movimiento(s) sin validar: ";
            //    foreach (var item in listaPiezaMovSinValidar)
            //        piezaTemp.Comentario += "[" + item + "]";

            //    piezaTemp.Comentario += ". ";
            //    enError = true;
            //}

            #endregion


            #region Validacion 2: Ubicacion

            //validar que la ubicacion de la pieza sea la misma que este movimiento

            //puede haber piezas que UbicacionID sea null y en este caso se concidera valida.
            if (piezaTemp.UbicacionID != null)
            {
                //Validar que la ubicacion Origen sea igual a la de la pieza
                if (UbicacionID != piezaTemp.UbicacionID)
                {
                    piezaTemp.Comentario += "No comparte la misma ubicación origen.";
                    enError = true;
                }
            }

            #endregion

            #region Validación 3: Folio Pieza
            if (string.IsNullOrWhiteSpace(piezaTemp.FolioPieza))
            {
                piezaTemp.FolioPieza = db.Piezas.Find(piezaTemp.PiezaID).ImprimirFolio();
            }

            #endregion

            #region Eliminar la pieza de las listas temporales

            //Eliminar de todas las listas la obra
            foreach (var item in listaBuscar.Where(a => a.PiezaID == piezaTemp.PiezaID).ToList())
                listaBuscar.Remove(item);

            foreach (var item in listaAceptar.Where(a => a.PiezaID == piezaTemp.PiezaID).ToList())
                listaAceptar.Remove(item);

            foreach (var item in listaError.Where(a => a.PiezaID == piezaTemp.PiezaID).ToList())
                listaError.Remove(item);

            #endregion


            #region Agregar la pieza a la lista

            //Comprobar que la exista en el movimiento            
            if (piezaTemp.ExisteEnMov)
            {

                var temp = db.MovimientoTempPiezas.Find(MovimientoID, PiezaID);

                piezaTemp.EsPendiente = temp.EsPendiente;
                piezaTemp.ExisteEnMov = true;
                piezaTemp.SeMovio = temp.SeMovio;

                //Si existe en el movimiento

                //Validar que sea la ultima
                piezaTemp.EsUltimo = db.MovimientoTempPiezas.Where(a => a.PiezaID == PiezaID && a.Orden > temp.Orden && !a.EnError).Count() > 0 ? false : true;


                if (piezaTemp.EsPendiente)
                {
                    piezaTemp.EnError = enError;

                    //Agregar a la lista temporal correspondiente 

                    if (enError)
                        listaError.Add(piezaTemp);
                    else
                        listaAceptar.Add(piezaTemp);
                }
                else
                {
                    piezaTemp.EnError = false;
                    listaAceptar.Add(piezaTemp);
                }


            }
            else
            {
                //No existe en movimiento
                piezaTemp.EnError = enError;
                piezaTemp.EsUltimo = !enError;
                piezaTemp.EsPendiente = !enError;

                piezaTemp.ExisteEnMov = false;
                piezaTemp.SeMovio = false;

                //Agregar a la lista temporal correspondiente 
                if (enError)
                    listaError.Add(piezaTemp);
                else
                    listaAceptar.Add(piezaTemp);

            }

            #endregion

            #region Guardar las listas

            Session["listaAceptar_" + MovimientoID] = listaAceptar.OrderBy(a => a.FolioPieza).ToList();
            Session["listaError_" + MovimientoID] = listaError.OrderBy(a => a.FolioPieza).ToList();
            Session["listaBuscar_" + MovimientoID] = listaBuscar;

            #endregion
        }


        public async Task<ActionResult> RefrescarLista(Guid MovimientoID, string NombreLista = "listaAceptar")
        {
            var listaError = Session["listaError_" + MovimientoID] == null ? new List<Item_MovPieza>() : ((List<Item_MovPieza>)Session["listaError_" + MovimientoID]).OrderBy(a => a.FolioPieza).ToList();
            var listaAceptar = Session["listaAceptar_" + MovimientoID] == null ? new List<Item_MovPieza>() : ((List<Item_MovPieza>)Session["listaAceptar_" + MovimientoID]).OrderBy(a => a.FolioPieza).ToList();

            int i = 0;

            foreach (var item in listaAceptar) item.Indice = i++;

            foreach (var item in listaError) item.Indice = i++;

            Session["listaError_" + MovimientoID] = listaError;
            Session["listaAceptar_" + MovimientoID] = listaAceptar;

            ViewBag.NombreLista = NombreLista;

            var EstadoMov = db.MovimientosTemp.Find(MovimientoID).EstadoMovimiento;
            bool AddPiezaEnabled = (EstadoMov == EstadoMovimientoTemp.Procesando || EstadoMov == EstadoMovimientoTemp.Concluido_SinValidar || EstadoMov == EstadoMovimientoTemp.Concluido) ? true : false;

            ViewBag.AddPiezaEnabled = AddPiezaEnabled;

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
            {
                listaBuscar.Remove(item);
                Session["listaBuscar_" + MovimientoID] = listaBuscar;
            }

            foreach (var item in listaAceptar.Where(a => a.PiezaID == PiezaID).ToList())
            {
                listaAceptar.Remove(item);
                Session["listaAceptar_" + MovimientoID] = listaAceptar.OrderBy(a => a.FolioPieza).ToList();
            }

            foreach (var item in listaError.Where(a => a.PiezaID == PiezaID).ToList())
            {
                listaError.Remove(item);
                Session["listaError_" + MovimientoID] = listaError.OrderBy(a => a.FolioPieza).ToList();
            }

        }

        #region Sin uso


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
