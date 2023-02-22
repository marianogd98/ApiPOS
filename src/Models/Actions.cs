using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Action
    {
        public int id { get; set; }
        public int moduloId { get; set; }
        public string descripcion { get; set; }
    }

    public class Actions
    {
        public int success { get; set; }
        public string message { get; set; }
        public List<Action> data { get; set; }
    }
}
