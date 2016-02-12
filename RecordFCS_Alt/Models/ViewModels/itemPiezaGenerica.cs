using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RecordFCS_Alt.Models.ViewsModel
{
    public class itemPiezaGenerica
    {
        [Key]
        public Guid PiezaID { get; set; }
        public Guid ObraID { get; set; }
        public string PiezaClave { get; set; }
        public string ObraClave { get; set; }
        public string RutaImagen { get; set; }

        public virtual List<itemPiezaGenericaCampo> itemPiezaGenericaCampos { get; set; }
    }
}