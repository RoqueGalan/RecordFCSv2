using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordFCS_Alt.Models
{
    public partial class HistorialLogDetalle
    {
        [Key]
        public Guid HistorialLogDetalleID { get; set; }
        

        public string ColumnaNombre { get; set; }
        public string ValorOriginal { get; set; }
        public string ValorNuevo { get; set; }



        [ForeignKey("HistorialLog")]
        public Guid HistorialLogID { get; set; }

        
        //virtual
        public HistorialLog HistorialLog { get; set; }

    }
}
