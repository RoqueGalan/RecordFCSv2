using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RecordFCS_Alt.Models
{
    public class MovimientoAutorizacion
    {
        [Key]
        public int MovimientoAutorizacionID { get; set; }

        [ForeignKey("Usuario1")]
        public Guid? Usuario1ID { get; set; }
        
        [ForeignKey("Usuario2")]
        public Guid? Usuario2ID { get; set; }


        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? FechaAutorizacion1 { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? FechaAutorizacion2 { get; set; }


        //virtual
        public virtual Usuario Usuario1 { get; set; }
        public virtual Usuario Usuario2 { get; set; }


    }
}