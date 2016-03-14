using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RecordFCS_Alt.Models
{
    public class ItemFicha
    {
        public ItemFicha()
        {
            Campos = new List<CampoFicha>();
            Imagenes = new List<ImagenFicha>();

            LinkCrear = new Enlace();
            LinkEditar = new Enlace();
            LinkEliminar = new Enlace();
            LinkDetalles = new Enlace();
            LinkHistorial = new Enlace();
            LinkFichaCompleta = new Enlace();

        }



        public Guid PiezaID { get; set; }
        public Guid ObraID { get; set; }
        public string FolioPieza { get; set; }
        public string FolioObra { get; set; }

        public string NombrePieza { get; set; }
        public string NombreObra { get; set; }

        public bool EsPrincipal { get; set; }
        public bool EsCompleta { get; set; }
        public bool Status { get; set; }



        public Enlace LinkCrear { get; set; }
        public Enlace LinkEditar { get; set; }
        public Enlace LinkEliminar { get; set; }
        public Enlace LinkDetalles { get; set; }
        public Enlace LinkHistorial { get; set; }
        public Enlace LinkFichaCompleta { get; set; }



        public virtual List<CampoFicha> Campos { get; set; }
        public virtual List<ImagenFicha> Imagenes { get; set; }
        public virtual List<Enlace> TipoArchivosLinks { get; set; }
        public virtual List<Enlace> NuevasPiezasLinks { get; set; }



        public class CampoFicha
        {
            public CampoFicha()
            {
                Valores = new List<ValorCampoFicha>();

                LinkCrear = new Enlace();
                LinkEditar = new Enlace();
                LinkEliminar = new Enlace();
                LinkDetalles = new Enlace();
                LinkHistorial = new Enlace();
            }
            public string ID { get; set; }
            public string Nombre { get; set; }


            public Enlace LinkCrear { get; set; }
            public Enlace LinkEditar { get; set; }
            public Enlace LinkEliminar { get; set; }
            public Enlace LinkDetalles { get; set; }
            public Enlace LinkHistorial { get; set; }


            public string ClaseCss { get; set; }
            public string ToolTip { get; set; }


            public virtual List<ValorCampoFicha> Valores { get; set; }

            public class ValorCampoFicha
            {
                public ValorCampoFicha()
                {
                    LinkCrear = new Enlace();
                    LinkEditar = new Enlace();
                    LinkEliminar = new Enlace();
                    LinkDetalles = new Enlace();
                    LinkHistorial = new Enlace();
                }

                public string ID { get; set; }
                public string Nombre { get; set; }
                public bool EsValido { get; set; }

                public Enlace LinkCrear { get; set; }
                public Enlace LinkEditar { get; set; }
                public Enlace LinkEliminar { get; set; }
                public Enlace LinkDetalles { get; set; }
                public Enlace LinkHistorial { get; set; }

                public string ClaseCss { get; set; }
                public string ToolTip { get; set; }
            }

        }


        public class ImagenFicha
        {
            public ImagenFicha()
            {
                LinkCrear = new Enlace();
                LinkEditar = new Enlace();
                LinkEliminar = new Enlace();
                LinkDetalles = new Enlace();
                LinkHistorial = new Enlace();
            }

            public string Titulo { get; set; }
            public string Descripcion { get; set; }
            public bool Status { get; set; }

            public string RutaMini { get; set; }
            public string Ruta { get; set; }


            public Enlace LinkCrear { get; set; }
            public Enlace LinkEditar { get; set; }
            public Enlace LinkEliminar { get; set; }
            public Enlace LinkDetalles { get; set; }
            public Enlace LinkHistorial { get; set; }

        }





        public class Enlace
        {
            public string Icono { get; set; }
            public string Texto { get; set; }
            public string Href { get; set; }
            public string OtrasDatas { get; set; }

            public string DataTitle { get; set; }
            public string DataToggle { get; set; }
            public string DataModal { get; set; }

            public string ClaseCss { get; set; }

        }
    }
}