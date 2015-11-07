using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RecordFCS_Alt.Models.ViewModels
{
    public class Item_MovPieza
    {
        public Guid ObraID { get; set; }

        public string FolioObra { get; set; }

        public Guid PiezaID { get; set; }

        public string FolioPieza { get; set; }

        public Guid? UbicacionID { get; set; }

        public string Titulo { get; set; }

        public string Autor { get; set; }

        public string Imagen { get; set; }

        //
        public string Descripcion { get; set; }
        public int TotalPiezas{ get; set; }

    }
}