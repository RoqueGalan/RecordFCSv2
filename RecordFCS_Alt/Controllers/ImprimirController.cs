using RecordFCS_Alt.Models;
using RecordFCS_Alt.Models.ViewModels;
using RecordFCS_Alt.Models.ViewsModel;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RecordFCS_Alt.Controllers
{
    public class ImprimirController : Controller
    {
        private RecordFCSContext db = new RecordFCSContext();

        // GET: Imprimir
        public ActionResult Menu()
        {
            var imprimir = new itemImprimir() {
                TipoImprimir = "Busqueda"
            };

            return PartialView("_Menu", imprimir);
        }

        private void generarPiezaImprimir(Pieza pieza, itemImprimir imprimir)
        {
            itemPiezaMini piezaMini = new itemPiezaMini()
            {
                ObraID = pieza.ObraID,
                PiezaID = pieza.PiezaID,
                FolioObra = pieza.Obra.LetraFolio.Nombre + pieza.Obra.NumeroFolio,
                FolioPieza = pieza.ImprimirFolio(),
                NombreObra = pieza.Obra.TipoObra.Nombre,
                NombrePieza = pieza.TipoPieza.Nombre,
                esPrincipal = pieza.TipoPieza.EsPrincipal,
                //esBusqueda = esBusqueda,
                //ListaPiezasHijas = new List<Guid>(),
                Atributos = new List<itemPiezaMiniAtt>(),
                UbicacionOrigenID = pieza.UbicacionID
            };


            string nombreMostrarArchivos = "";
            string nombreMostrarAtributos = "";

            switch (imprimir.TipoImprimir)
            {
                case "":
                default:
                    break;
            }


            var tipoMostrarArchivos = db.TipoMostrarArchivos.FirstOrDefault(a => a.Nombre == "");
            var tipoMostrarAtributos = db.TipoMostarlos.FirstOrDefault(a=> a.Nombre == "");

            //extraer la lista de att de la pieza en guion
            //Movimiento = Guion



            List<Atributo> listaAttMovFicha = new List<Atributo>();//pieza.TipoPieza.Atributos.Where(a => a.Status && a.MostrarAtributos.Any(b => b.TipoMostrar.Nombre == "Guion" && b.Status) && a.TipoAtributo.Status).OrderBy(a => a.Orden).ToList();

            //Guion
            listaAttMovFicha = pieza.TipoPieza.Atributos.Where(a => a.Status && a.MostrarAtributos.Any(b => b.TipoMostrar.Nombre == "Guion" && b.Status) && a.TipoAtributo.Status).OrderBy(a => a.Orden).ToList();



            imprimir.ListaPiezas.Add(piezaMini);

            if (imprimir.ConPiezasAdicionales)
                foreach (var item in pieza.PiezasHijas.ToList())
                    generarPiezaImprimir(item, imprimir);

        }




        [HttpPost]
        public ActionResult Imprimir(itemImprimir imprimir, string ListaPiezasID = "")
        {
            string[] listaID = ListaPiezasID.Split(',');
            imprimir.ListaPiezas = new List<itemPiezaMini>();

            foreach (var itemID in listaID)
            {
                var piezaTemp = db.Piezas.Find(itemID);

                if (piezaTemp != null)
                {
                    //extraer toda la informacion de la pieza
                    //piezas adicionales

                

                }
                
            }


            return new ViewAsPdf("_FormatoBasicoPDF", imprimir);
        }


        public ActionResult FichaPrint(Guid? PiezaID, bool DatosCompletos, string Tipo = "Busqueda")
        {






            var itemPieza = new Item_MovPieza();

            return PartialView("_FichaPrint", itemPieza);
        }


    }


}