using RecordFCS_Alt.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using RecordFCS_Alt.Models;
using System.Data.Entity.Infrastructure;

namespace RecordFCS_Alt.Helpers.Historial
{
    public class HistorialLogica
    {
        public static HistorialLog CrearEntidad(object objeto, string tablaNombre, string llavePrimaria, Guid usuarioID, RecordFCSContext db, DateTime? EventoFecha = null)
        {
            HistorialLog historialLog;

            EventoFecha = EventoFecha ?? DateTime.Now;

            try
            {
                #region Crear entidades


                var entidad = db.Entry(objeto);


                #endregion

                #region Validar las propiedades que sufrieron cambios

                List<HistorialLogDetalle> listaHistorialLogDetalles = new List<HistorialLogDetalle>();

                //tipo de evento: Crear, Editar, Eliminar
                string eventoTipo = "Crear";
                //motivo por el cual se realiza la accion
                string motivo = "Creación de nuevo registro";

                //Crear el HistorialLog
                historialLog = new HistorialLog()
                {
                    HistorialLogID = Guid.NewGuid(),
                    EventoFecha = EventoFecha.Value,
                    EventoTipo = eventoTipo,
                    TablaNombre = tablaNombre,
                    LlavePrimaria = llavePrimaria,
                    Motivo = motivo,
                    UsuarioID = usuarioID,
                    CategoriaTipo = ""
                };

                //Crear la lista de propiedades con cambios
                foreach (var propiedadNombre in entidad.CurrentValues.PropertyNames)
                {
                    var valorNuevo = (entidad.Property(propiedadNombre).CurrentValue ?? "").ToString();

                    //validar que la propiedad que se va a guardar no sea vacia o nula, asi evitamos basura en la base de datos
                    if (!string.IsNullOrWhiteSpace(valorNuevo))
                    {
                        listaHistorialLogDetalles.Add(new HistorialLogDetalle()
                        {
                            HistorialLogDetalleID = Guid.NewGuid(),
                            HistorialLogID = historialLog.HistorialLogID,
                            ColumnaNombre = propiedadNombre,
                            ValorOriginal = "",
                            ValorNuevo = valorNuevo
                        });
                    }
                }

                #endregion

                #region Validar el historial

                if (listaHistorialLogDetalles.Count > 0)
                    historialLog.HistorialLogDetalles = listaHistorialLogDetalles;
                else
                    historialLog = null;

                #endregion

            }
            catch (Exception)
            {

                historialLog = null;
            }


            return historialLog;
        }

        public static HistorialLog EditarEntidad(object objeto, object objetoDB, string tablaNombre, string llavePrimaria, Guid usuarioID, RecordFCSContext db, string motivo, DateTime? EventoFecha = null)
        {
            HistorialLog historialLog;

            EventoFecha = EventoFecha ?? DateTime.Now;

            try
            {
                #region Crear entidades


                //Entidad extraida de la base de datos
                var entidadDB = db.Entry(objetoDB);

                //Entidad extraida del formulario
                var entidadForm = db.Entry(objeto);


                #endregion

                #region Binding Campos Nuevos con Actuales

                foreach (var propiedadNombre in entidadDB.CurrentValues.PropertyNames)
                    entidadDB.Property(propiedadNombre).CurrentValue = entidadForm.Property(propiedadNombre).CurrentValue;

                #endregion

                #region Historial validar las propiedades que sufrieron cambios

                List<HistorialLogDetalle> listaHistorialLogDetalles = new List<HistorialLogDetalle>();

                //tipo de evento: Crear, Editar, Eliminar
                string eventoTipo = "Editar";

                //Crear el HistorialLog
                historialLog = new HistorialLog()
                {
                    HistorialLogID = Guid.NewGuid(),
                    EventoFecha = EventoFecha.Value,
                    EventoTipo = eventoTipo,
                    TablaNombre = tablaNombre,
                    LlavePrimaria = llavePrimaria,
                    Motivo = motivo,
                    UsuarioID = usuarioID,
                    CategoriaTipo = ""
                };

                //Crear la lista de propiedades con cambios
                foreach (var propiedadNombre in entidadDB.CurrentValues.PropertyNames.Where(p => entidadDB.Property(p).IsModified))
                {
                    string columnaNombre = propiedadNombre;
                    var valorOriginal = (entidadDB.Property(propiedadNombre).OriginalValue ?? "").ToString();
                    var valorNuevo = (entidadDB.Property(propiedadNombre).CurrentValue ?? "").ToString();

                    listaHistorialLogDetalles.Add(new HistorialLogDetalle()
                    {
                        HistorialLogDetalleID = Guid.NewGuid(),
                        HistorialLogID = historialLog.HistorialLogID,
                        ColumnaNombre = columnaNombre,
                        ValorOriginal = valorOriginal,
                        ValorNuevo = valorNuevo
                    });
                }

                #endregion

                #region Validar el historial

                if (listaHistorialLogDetalles.Count > 0)
                    historialLog.HistorialLogDetalles = listaHistorialLogDetalles;
                else
                    historialLog = null;

                #endregion

            }
            catch (Exception)
            {

                historialLog = null;
            }


            return historialLog;
        }

        public static HistorialLog EliminarEntidad(object objeto, string tablaNombre, string llavePrimaria, Guid usuarioID, RecordFCSContext db, DateTime? EventoFecha = null)
        {
            HistorialLog historialLog;

            EventoFecha = EventoFecha ?? DateTime.Now;

            try
            {
                #region Eliminar entidades


                var entidad = db.Entry(objeto);


                #endregion

                #region Validar las propiedades que sufrieron cambios

                List<HistorialLogDetalle> listaHistorialLogDetalles = new List<HistorialLogDetalle>();

                //tipo de evento: Crear, Editar, Eliminar
                string eventoTipo = "Eliminar";
                //motivo por el cual se realiza la accion
                string motivo = "Eliminación de registro";

                //Crear el HistorialLog
                historialLog = new HistorialLog()
                {
                    HistorialLogID = Guid.NewGuid(),
                    EventoFecha = EventoFecha.Value,
                    EventoTipo = eventoTipo,
                    TablaNombre = tablaNombre,
                    LlavePrimaria = llavePrimaria,
                    Motivo = motivo,
                    UsuarioID = usuarioID,
                    CategoriaTipo = ""
                };

                //Crear la lista de propiedades con cambios
                foreach (var propiedadNombre in entidad.OriginalValues.PropertyNames)
                {
                    var valorOriginal = (entidad.Property(propiedadNombre).OriginalValue ?? "").ToString();

                    //validar que la propiedad que se va a eliminar no sea vacia o nula, asi evitamos basura en la base de datos
                    if (!string.IsNullOrWhiteSpace(valorOriginal))
                    {
                        listaHistorialLogDetalles.Add(new HistorialLogDetalle()
                        {
                            HistorialLogDetalleID = Guid.NewGuid(),
                            HistorialLogID = historialLog.HistorialLogID,
                            ColumnaNombre = propiedadNombre,
                            ValorNuevo = "",
                            ValorOriginal = valorOriginal
                        });
                    }
                }

                #endregion

                #region Validar el historial

                if (listaHistorialLogDetalles.Count > 0)
                    historialLog.HistorialLogDetalles = listaHistorialLogDetalles;
                else
                    historialLog = null;

                #endregion

            }
            catch (Exception)
            {

                historialLog = null;
            }


            return historialLog;
        }


    }
}

