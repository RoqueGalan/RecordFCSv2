using RecordFCS_Alt.Helpers.Historial;
using RecordFCS_Alt.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

//Record Administrador Automatico de Movimientos
//usr:  admAutomatico
//id:   0EE81876-A715-4380-9B76-82C9DF323D1E
//pwd:  adm@Rec2016

namespace RecordFCS_Alt.WebServices.Movimientos.Controllers
{
    public class MovimientoApiController : ApiController
    {
        private RecordFCSContext db = new RecordFCSContext();

        //Usuario Record Automatico de movimientos
        private Guid UsuarioID = new Guid("0EE81876-A715-4380-9B76-82C9DF323D1E");

        [HttpGet]
        public string Iniciar()
        {
            //Declararacion
            var fechaActual = DateTime.Now;
            var limiteDias = 2;
            var limiteHoras = 2;
            var limiteMinutos = 15;


            //Siempre estar iniciando esta funcion para verificar si hay o no movimientos 

            //Los movimientos que se verifican son:
            //Movimientos en Estado: Procesando, Concluido_SinValidar

            //Ejecutar un movimiento
            //Procesados: Procesar solo movimientos con fecha de antiguiedad 2 horas, y tengan piezas disponibles y no tengan piezas movidas

            //listar los movimientos que son de fecha anterior a la actual, y que esten en estado Procesando
            foreach (var mov in db.MovimientosTemp.Where(a => a.EstadoMovimiento == EstadoMovimientoTemp.Procesando && a.FechaSalida <= fechaActual).ToList())
            {
                //Si fecha ya es muy vieja hacer el movimiento manual, esto es para evitar posibles complicaciones
                //No realizar el movimiento no tiene piezas validas y tampoco realizarlo si tiene piezas movidas

                //Solo ejecutar cuando la fecha + limite es mayor a la fecha actual
                if (mov.FechaSalida.Value.AddDays(limiteDias) >= fechaActual)
                    if (mov.MovimientoTempPiezas.Where(a => a.EsPendiente && !a.EnError).Count() > 0 && mov.MovimientoTempPiezas.Where(a => a.SeMovio).Count() == 0)
                    {
                        mov.FechaUltimaEjecucion = fechaActual;
                        EjecutarMovimiento(mov);
                    }


            }


            //Revertir un movimiento
            //Concluidos_SinValidar: Procesar todos los movimientos antiguos con 2 dias o mas a la actual, y tengan piezas en movimiento o disponibles, solo mover las que estuvieron en movimiento.

            foreach (var mov in db.MovimientosTemp.Where(a => a.EstadoMovimiento == EstadoMovimientoTemp.Concluido_SinValidar && a.FechaSalida <= fechaActual).ToList())
            {
                //Validar el limite de dias, si el limite se supero, pero no debe de ser mas antiguo por 2 dias
                // 10  -    10 + 2  = -2 x
                // 10  -    9 + 2  = -1 x
                // 10  -    8 + 2  = 0 x
                // 10  -    7 + 2  = 1
                // 10  -    6 + 2  = 2
                // 10  -    5 + 2  = 3 x 

                var fechaLimite = mov.FechaSalida.Value.AddDays(limiteDias);

                bool SuperoLimiteParaSerValidado = fechaActual >= fechaLimite ? true : false;


                //int TotalDiasPasadosPorLimite = (fechaActual.AddDays(2) - fechaLimite).Days;






                if (SuperoLimiteParaSerValidado)
                {
                    //Validar que las piezas que se movieron sean ultimas

                    int totalMovidas = mov.MovimientoTempPiezas.Where(a => a.SeMovio).Count();
                    int totalMovidasSonUltimas = 0;

                    #region Validacion 3: En ultimo movimiento

                    foreach (var piezaEnMovReal in mov.MovimientoTempPiezas.Where(a => a.SeMovio).ToList())
                    {
                        if ((piezaEnMovReal.Pieza.MovimientoTempPiezas.Where(a => a.Orden > piezaEnMovReal.Orden && !a.EnError).Count() > 0))
                            totalMovidasSonUltimas = 0;
                        else
                            totalMovidasSonUltimas++;
                    }

                    #endregion

                    mov.FechaUltimaEjecucion = fechaActual;

                    if (totalMovidas == totalMovidasSonUltimas)
                    {

                        RevertirMovimiento(mov);
                    }
                    else
                    {
                        //Guardar un error en el movimiento
                        string error = "Error: No se a podido ejecutar el movimiento automatico, ya que existen piezas que fueron asignadas a otro movimiento despues de este.";
                        GuardarErrorEnMovimiento(mov, error);
                    }


                }


            }



            var fechaTemp = DateTime.Now;
            var totalMinutos = (fechaTemp - fechaActual).Minutes;

            fechaTemp = fechaTemp.AddMinutes(totalMinutos);
            var totalSegundos = (fechaTemp - fechaActual).Seconds;



            var x = "Tiempo total: " + totalMinutos + ":" + totalSegundos + " mins.";

            return x;
        }


        private bool GuardarErrorEnMovimiento(MovimientoTemp mov, string Error)
        {
            bool bandera = false;

            DateTime Fecha = mov.FechaUltimaEjecucion.Value;

            //------ Logica HISTORIAL

            #region Generar el historial

            //objeto del formulario
            var objeto3 = mov;
            //Objeto de la base de datos
            var objetoDB3 = db.MovimientosTemp.Find(mov.MovimientoTempID);
            //tabla o clase a la que pertenece
            var tablaNombre3 = "MovimientoTemp";
            //llave primaria del objeto
            var llavePrimaria3 = objetoDB3.MovimientoTempID.ToString();

            //generar el historial
            var historialLog3 = HistorialLogica.EditarEntidad(
                objeto3,
                objetoDB3,
                tablaNombre3,
                llavePrimaria3,
                UsuarioID,
                db,
                Error,
                Fecha
                );

            #endregion

            #region Guardar el historial

            if (historialLog3 != null)
            {
                //Cambiar el estado a la entidad a modificada
                db.Entry(objetoDB3).State = EntityState.Modified;
                //Guardamos la entidad modificada
                db.SaveChanges();

                //Guardar el historial
                db.HistorialLogs.Add(historialLog3);
                db.SaveChanges();

                bandera = true;
            }
            else
            {
                bandera = false;
            }
            #endregion

            //------

            return bandera;
        }


        private bool EjecutarMovimiento(MovimientoTemp mov)
        {
            bool bandera = false;

            DateTime Fecha = mov.FechaUltimaEjecucion.Value;
            string Motivo = "Ejecución automática del movimiento.";

            try
            {


                int contadorPMovidas = 0;

                //saber que piezas se van a mover
                //listar piezas que no tengan error, sean pendientes


                /*Guardar el movimiento*/
                foreach (var PiezaID in mov.MovimientoTempPiezas.Where(a => !a.EnError && a.EsPendiente && !a.SeMovio).Select(a => a.PiezaID))
                {

                    #region Buscar la pieza con su movimiento

                    //Buscar la pieza y la pieza en el movimiento
                    var pieza = db.Piezas.Find(PiezaID);
                    var piezaEnMovReal = pieza.MovimientoTempPiezas.FirstOrDefault(a => a.MovimientoTempID == mov.MovimientoTempID);

                    #endregion

                    #region Declaracion y asignaciones

                    piezaEnMovReal.EnError = false;
                    piezaEnMovReal.EsPendiente = true;
                    piezaEnMovReal.SeMovio = false;

                    string comentario = "";

                    #endregion


                    #region Validacion 1: Asignada en movimientos

                    #region Buscar donde sea Pendiente y sin errores

                    //lista de movimientos ID
                    //Buscar la pieza en todos los movimientos excepto este, donde sea pendiente y no tenga errrores.

                    List<int> listaMov_ID =
                        db.MovimientoTempPiezas
                        .Where(a =>
                            a.PiezaID == PiezaID &&
                            a.MovimientoTempID != mov.MovimientoTempID &&
                            a.EsPendiente && !a.EnError
                            )
                            .Select(a => a.MovimientoTemp.Folio).ToList();

                    #endregion

                    #region Buscar en Concluido sin validar

                    listaMov_ID.AddRange(
                        db.MovimientoTempPiezas.Where(a => a.PiezaID == PiezaID && a.MovimientoTempID != mov.MovimientoTempID &&
                        a.MovimientoTemp.EstadoMovimiento == EstadoMovimientoTemp.Concluido_SinValidar &&
                        a.SeMovio).Select(a => a.MovimientoTemp.Folio).ToList()
                        );

                    #endregion

                    #region Buscar en Cancelado

                    listaMov_ID.AddRange(
                        db.MovimientoTempPiezas.Where(a => a.PiezaID == PiezaID && a.MovimientoTempID != mov.MovimientoTempID &&
                        a.MovimientoTemp.EstadoMovimiento == EstadoMovimientoTemp.Cancelado &&
                        a.SeMovio).Select(a => a.MovimientoTemp.Folio).ToList()
                        );

                    #endregion

                    #region Buscar en Retornado

                    listaMov_ID.AddRange(
                        db.MovimientoTempPiezas.Where(a => a.PiezaID == PiezaID && a.MovimientoTempID != mov.MovimientoTempID &&
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
                        piezaEnMovReal.Comentario = "Asignada en:" + comentario + ". ";
                        piezaEnMovReal.EnError = true;
                    }

                    #endregion

                    #region Validacion 2: Ubicacion

                    //validar que la ubicacion de la pieza sea la misma que este movimiento

                    //puede haber piezas que UbicacionID sea null y en este caso se concidera valida.
                    if (pieza.UbicacionID != null)
                    {
                        //Validar que la ubicacion Origen sea igual a la de la pieza
                        if (mov.UbicacionOrigenID != pieza.UbicacionID)
                        {
                            piezaEnMovReal.Comentario += "No comparte la misma ubicación origen. ";
                            piezaEnMovReal.EnError = true;
                        }
                    }



                    #endregion

                    #region Validacion 3: En ultimo movimiento

                    if (pieza.MovimientoTempPiezas.Where(a => a.Orden > piezaEnMovReal.Orden && !a.EnError).Count() > 0)
                    {
                        piezaEnMovReal.Comentario += "No esta en su último movimiento. ";
                        piezaEnMovReal.EnError = true;
                    }

                    #endregion


                    //Sin errores entoces proceder a realizar el cambio de ubicacion

                    #region Cambio de Ubicacion

                    if (!piezaEnMovReal.EnError)
                    {
                        //Sin errores

                        #region Asignar un orden al movimiento de la pieza

                        if (piezaEnMovReal.Orden == 0)
                        {
                            var ordenActual = pieza.MovimientoTempPiezas.Count > 0 ? pieza.MovimientoTempPiezas.OrderByDescending(a => a.Orden).FirstOrDefault().Orden : 0;
                            piezaEnMovReal.Orden = piezaEnMovReal.EnError ? 0 : ordenActual + 1;
                        }

                        #endregion

                        //Ejecutar el cambio de ubicacion
                        pieza.UbicacionID = mov.UbicacionDestinoID;

                        //------ Logica HISTORIAL

                        #region Generar el historial

                        //objeto del formulario
                        var objeto = pieza;
                        //Objeto de la base de datos
                        var objetoDB = db.Piezas.Find(pieza.PiezaID);
                        //tabla o clase a la que pertenece
                        var tablaNombre = "Pieza";
                        //llave primaria del objeto
                        var llavePrimaria = objetoDB.PiezaID.ToString();

                        //generar el historial
                        var historialLog = HistorialLogica.EditarEntidad(
                            objeto,
                            objetoDB,
                            tablaNombre,
                            llavePrimaria,
                            UsuarioID,
                            db,
                            Motivo,
                            Fecha
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

                            contadorPMovidas++;

                            piezaEnMovReal.SeMovio = true;
                            piezaEnMovReal.EsPendiente = false;
                            piezaEnMovReal.EnError = false;
                            piezaEnMovReal.Comentario = "";
                        }

                        #endregion

                        //------

                    }
                    else
                    {
                        //Con errores
                        piezaEnMovReal.Orden = 0;
                    }

                    pieza = null;

                    #endregion


                    #region Asignacion de nuevos valores

                    //buscar el objeto
                    var itemTemp = db.MovimientoTempPiezas.Find(mov.MovimientoTempID, PiezaID);
                    itemTemp.Comentario = piezaEnMovReal.Comentario;
                    itemTemp.EnError = piezaEnMovReal.EnError;
                    itemTemp.EsPendiente = piezaEnMovReal.EsPendiente;
                    itemTemp.Orden = piezaEnMovReal.Orden;
                    itemTemp.SeMovio = piezaEnMovReal.SeMovio;

                    piezaEnMovReal = null;


                    #endregion


                    //------ Logica HISTORIAL

                    #region Generar el historial

                    //objeto del formulario
                    var objeto2 = itemTemp;
                    //Objeto de la base de datos
                    var objetoDB2 = db.MovimientoTempPiezas.Find(mov.MovimientoTempID, PiezaID);
                    //tabla o clase a la que pertenece
                    var tablaNombre2 = "MovimientoTempPieza";
                    //llave primaria del objeto
                    var llavePrimaria2 = objetoDB2.MovimientoTempID + "," + objetoDB2.PiezaID;

                    //generar el historial
                    var historialLog2 = HistorialLogica.EditarEntidad(
                        objeto2,
                        objetoDB2,
                        tablaNombre2,
                        llavePrimaria2,
                        UsuarioID,
                        db,
                        Motivo,
                        Fecha
                        );

                    #endregion

                    #region Guardar el historial

                    if (historialLog2 != null)
                    {
                        //Cambiar el estado a la entidad a modificada
                        db.Entry(objetoDB2).State = EntityState.Modified;
                        //Guardamos la entidad modificada
                        db.SaveChanges();

                        //Guardar el historial
                        db.HistorialLogs.Add(historialLog2);
                        db.SaveChanges();

                    }

                    #endregion

                    //------

                }

                //Cambio del estatus
                //Solo si se actualizo la ubicacion de por lo menos 1 pieza

                //saber cual sera el estatus Concluido ó Concluido_SinValidar

                if (contadorPMovidas > 0)
                {
                    if (mov.EsValido)
                        mov.EstadoMovimiento = EstadoMovimientoTemp.Concluido;
                    else
                        mov.EstadoMovimiento = EstadoMovimientoTemp.Concluido_SinValidar;
                }

                //------ Logica HISTORIAL

                #region Generar el historial

                //objeto del formulario
                var objeto3 = mov;
                //Objeto de la base de datos
                var objetoDB3 = db.MovimientosTemp.Find(mov.MovimientoTempID);
                //tabla o clase a la que pertenece
                var tablaNombre3 = "MovimientoTemp";
                //llave primaria del objeto
                var llavePrimaria3 = objetoDB3.MovimientoTempID.ToString();

                //generar el historial
                var historialLog3 = HistorialLogica.EditarEntidad(
                    objeto3,
                    objetoDB3,
                    tablaNombre3,
                    llavePrimaria3,
                    UsuarioID,
                    db,
                    Motivo,
                    Fecha
                    );

                #endregion

                #region Guardar el historial

                if (historialLog3 != null)
                {
                    //Cambiar el estado a la entidad a modificada
                    db.Entry(objetoDB3).State = EntityState.Modified;
                    //Guardamos la entidad modificada
                    db.SaveChanges();

                    //Guardar el historial
                    db.HistorialLogs.Add(historialLog3);
                    db.SaveChanges();
                }

                #endregion

                //------


            }
            catch (Exception)
            {
                bandera = false;
            }


            return bandera;
        }


        private bool RevertirMovimiento(MovimientoTemp mov)
        {
            bool bandera = false;

            DateTime Fecha = mov.FechaUltimaEjecucion.Value;
            string Motivo = "Revertir movimiento.";

            try
            {

                bandera = true;

                //Revertir las piezas que estan en el estado SeMovio
                foreach (var PiezaIDDel in mov.MovimientoTempPiezas.Where(a => a.SeMovio).Select(a => a.PiezaID).ToList())
                {
                    bandera = false;
                    //la pieza en el movimiento
                    var item = db.MovimientoTempPiezas.FirstOrDefault(a => a.PiezaID == PiezaIDDel && a.MovimientoTempID == mov.MovimientoTempID);

                    bool RegresarHistorial = false;

                    //saber si es eliminable y si se puede regresar el estado
                    if (item != null)
                    {
                        bool esUltimo = item.Pieza.MovimientoTempPiezas.Where(a => a.Orden > item.Orden && !a.EnError).Count() > 0 ? false : true;
                        bool conPendientes = item.Pieza.MovimientoTempPiezas.Where(a => a.EsPendiente && a.MovimientoTempID != mov.MovimientoTempID && !a.EnError).Count() > 0 ? true : false;

                        if (item.SeMovio && esUltimo)
                            RegresarHistorial = true;

                    }

                    if (RegresarHistorial)
                    {
                        var pieza = db.Piezas.Find(PiezaIDDel);

                        RegresarHistorial = false;

                        if (pieza != null)
                        {
                            if (pieza.UbicacionID == mov.UbicacionDestinoID)
                            {
                                pieza.UbicacionID = mov.UbicacionOrigenID;

                                //------ Logica HISTORIAL

                                #region Generar el historial

                                //objeto del formulario
                                var objetoDelUbi = pieza;
                                //Objeto de la base de datos
                                var objetoDBDelUbi = db.Piezas.Find(pieza.PiezaID);
                                //tabla o clase a la que pertenece
                                var tablaNombreDelUbi = "Pieza";
                                //llave primaria del objeto
                                var llavePrimariaDelUbi = objetoDBDelUbi.PiezaID.ToString();

                                //generar el historial
                                var historialLogDelUbi = HistorialLogica.EditarEntidad(
                                    objetoDelUbi,
                                    objetoDBDelUbi,
                                    tablaNombreDelUbi,
                                    llavePrimariaDelUbi,
                                    UsuarioID,
                                    db,
                                    "Retornar Movimiento: " + mov.Folio,
                                    mov.FechaUltimaEjecucion
                                    );

                                #endregion

                                #region Guardar el historial

                                if (historialLogDelUbi != null)
                                {
                                    //Cambiar el estado a la entidad a modificada
                                    db.Entry(objetoDBDelUbi).State = EntityState.Modified;
                                    //Guardamos la entidad modificada
                                    db.SaveChanges();

                                    //Guardar el historial
                                    db.HistorialLogs.Add(historialLogDelUbi);
                                    db.SaveChanges();

                                    RegresarHistorial = true;
                                }


                                #endregion

                                //------


                            }

                        }



                        if (RegresarHistorial)
                        {

                            item.SeMovio = false;
                            item.EsPendiente = true;
                            item.EnError = true;
                            item.Comentario = "Validar de nuevo [Movimiento Revertido].";


                            //------ Logica HISTORIAL

                            #region Generar el historial

                            //objeto del formulario
                            var objetoEdit = item;
                            //Objeto de la base de datos
                            var objetoDBEdit = db.MovimientoTempPiezas.Find(item.MovimientoTempID, item.PiezaID);
                            //tabla o clase a la que pertenece
                            var tablaNombreEdit = "MovimientoTempPieza";
                            //llave primaria del objeto
                            var llavePrimariaEdit = objetoDBEdit.MovimientoTempID + "," + objetoDBEdit.PiezaID;


                            //generar el historial
                            var historialLogEdit = HistorialLogica.EditarEntidad(
                                objetoEdit,
                                objetoDBEdit,
                                tablaNombreEdit,
                                llavePrimariaEdit,
                                UsuarioID,
                                db,
                                Motivo,
                                mov.FechaUltimaEjecucion
                                );

                            #endregion

                            #region Guardar el historial

                            if (historialLogEdit != null)
                            {
                                //Cambiar el estado a la entidad a modificada
                                db.Entry(objetoDBEdit).State = EntityState.Modified;
                                //Guardamos la entidad modificada
                                db.SaveChanges();

                                //Guardar el historial
                                db.HistorialLogs.Add(historialLogEdit);
                                db.SaveChanges();

                                bandera = true;
                            }

                            #endregion

                            //------


                        }


                    }

                }

                if (bandera)
                {
                    //Como todas las piezas que tenia se revertieron 
                    //REgresar el Estatus

                    mov.EstadoMovimiento = EstadoMovimientoTemp.Retornado;


                    //------ Logica HISTORIAL

                    #region Generar el historial

                    //objeto del formulario
                    var objeto3 = mov;
                    //Objeto de la base de datos
                    var objetoDB3 = db.MovimientosTemp.Find(mov.MovimientoTempID);
                    //tabla o clase a la que pertenece
                    var tablaNombre3 = "MovimientoTemp";
                    //llave primaria del objeto
                    var llavePrimaria3 = objetoDB3.MovimientoTempID.ToString();

                    //generar el historial
                    var historialLog3 = HistorialLogica.EditarEntidad(
                        objeto3,
                        objetoDB3,
                        tablaNombre3,
                        llavePrimaria3,
                        UsuarioID,
                        db,
                        Motivo + "[Automatico]",
                        Fecha
                        );

                    #endregion

                    #region Guardar el historial

                    if (historialLog3 != null)
                    {
                        //Cambiar el estado a la entidad a modificada
                        db.Entry(objetoDB3).State = EntityState.Modified;
                        //Guardamos la entidad modificada
                        db.SaveChanges();

                        //Guardar el historial
                        db.HistorialLogs.Add(historialLog3);
                        db.SaveChanges();
                    }
                    else
                    {
                        throw new Exception();
                    }
                    #endregion

                    //------

                }

            }
            catch (Exception)
            {
                bandera = false;
            }

            return bandera;
        }
    }

}
