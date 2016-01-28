using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RecordFCS_Alt.Models;


namespace RecordFCS_Alt.WebService.Controllers
{
    public class ValuesController : ApiController
    {
        
        private RecordFCSContext db = new RecordFCSContext();

        // GET api/values
        public IEnumerable<string> Get()
        {

            var lista = db.Ubicaciones.Select(a => a.Nombre).ToArray();


            return lista;
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
