using RecordFCS_Alt.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RecordFCS_Alt.WebService.Controllers
{
    public class MovimientoApiController : ApiController
    {
        private RecordFCSContext db = new RecordFCSContext();


        // GET api/values/5

        [HttpGet]
        public void ValidarMovimientoEnFecha()
        {
            //listar los movimientos que son de fecha anterior a la actual, y que esten en estado Procesando
            foreach (var mov in db.MovimientosTemp.Where(a => DateTime.Compare(a.FechaSalida.Value, DateTime.Now) < 0 && a.EstadoMovimiento == EstadoMovimientoTemp.Procesando).ToList())
            {
                if (mov.MovimientoTempPiezas.Count > 0)
                {

                    //revalidar las piezas del movimiento valdias
                    foreach (var PiezaID in mov.MovimientoTempPiezas.Where(a => !a.SeMovio && !a.EnError && a.EsPendiente).Select(a => a.PiezaID).ToList())
                    {
                        //buscar la pieza
                        var pieza = db.Piezas.Find(PiezaID);
                        var piezaEnMovReal = pieza.MovimientoTempPiezas.FirstOrDefault(a => a.MovimientoTempID == mov.MovimientoTempID);

                        piezaEnMovReal.EnError = false;
                        piezaEnMovReal.EsPendiente = true;
                        piezaEnMovReal.SeMovio = false;

                        //Validar que la pieza este disponible
                        //pieza validar que la pieza no este Pendiente y sin Error y sin Mover en ningun otro movimiento excepto este
                        var listaPiezaMov = db.MovimientoTempPiezas.Where(a => a.PiezaID == PiezaID && a.EsPendiente && !a.EnError && a.MovimientoTempID != mov.MovimientoTempID).Select(a => a.MovimientoTemp.Folio).OrderBy(a => a).ToList();
                        //validar que pieza no este asignada en otro movimiento
                        if (listaPiezaMov.Count > 0)
                        {
                            piezaEnMovReal.Comentario = "Asignada en movimiento(s): ";

                            foreach (var item in listaPiezaMov)
                                piezaEnMovReal.Comentario += "[" + item + "]";

                            piezaEnMovReal.Comentario += ". ";
                            piezaEnMovReal.EnError = true;
                        }

                        //validar que pieza y movimiento compartan la misma ubicacion de origen
                        if (pieza.UbicacionID != null)
                        {
                            if (mov.UbicacionOrigenID != pieza.UbicacionID)
                            {
                                piezaEnMovReal.Comentario += "No comparte la misma ubicación origen.";
                                piezaEnMovReal.EnError = true;
                            }
                        }

                        //Ya se revalido la pieza.
                        if (piezaEnMovReal.EnError)
                        {
                            //como no tiene errrores entonces
                            piezaEnMovReal.Orden = 0;
                        }
                        else
                        {
                            if (piezaEnMovReal.Orden == 0)
                            {
                                var ordenActual = pieza.MovimientoTempPiezas.Count > 0 ? pieza.MovimientoTempPiezas.OrderByDescending(a => a.Orden).FirstOrDefault().Orden : 0;
                                piezaEnMovReal.Orden = piezaEnMovReal.EnError ? 0 : ordenActual + 1;
                            }


                            bool esUltimo = pieza.MovimientoTempPiezas.Where(a => a.Orden > piezaEnMovReal.Orden && !a.EnError).Count() > 0 ? false : true;
                            bool conPendientes = pieza.MovimientoTempPiezas.Where(a => a.EsPendiente && a.MovimientoTempID != mov.MovimientoTempID && !a.EnError).Count() > 0 ? true : false;

                            if (esUltimo && !conPendientes)
                            {
                                //Ejecutar el cambio de ubicacion
                                //Pieza ubicacion es nula
                                if (pieza.Ubicacion == null || pieza.UbicacionID == mov.UbicacionOrigenID)
                                {
                                    pieza.UbicacionID = mov.UbicacionDestinoID;
                                    db.Entry(pieza).State = EntityState.Modified;
                                    db.SaveChanges();

                                    piezaEnMovReal.SeMovio = true;
                                    piezaEnMovReal.EsPendiente = false;
                                    piezaEnMovReal.EnError = false;
                                    piezaEnMovReal.Comentario = "";
                                }
                            }

                        }

                        pieza = null;


                        //buscar el objeto
                        var itemTemp = db.MovimientoTempPiezas.Find(mov.MovimientoTempID, PiezaID);
                        itemTemp.Comentario = piezaEnMovReal.Comentario;
                        itemTemp.EnError = piezaEnMovReal.EnError;
                        itemTemp.EsPendiente = piezaEnMovReal.EsPendiente;
                        itemTemp.Orden = piezaEnMovReal.Orden;
                        itemTemp.SeMovio = piezaEnMovReal.SeMovio;

                        piezaEnMovReal = null;

                        db.Entry(itemTemp).State = EntityState.Modified;
                        db.SaveChanges();
                    }



                    mov.EstadoMovimiento = EstadoMovimientoTemp.Concluido;

                    db.Entry(mov).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }

            //return
        }



    }
}
