using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfPos_Rios.ViewModels.helperResponse
{
   public class UserDataResponse
    {
        public class User
        {
            public int id { get; set; }
            public string nombre { get; set; }
            public int estatus { get; set; }
            public object token { get; set; }
        }

        public class AccionPermitida
        {
            public int idAccion { get; set; }
            public string nombre { get; set; }
            public bool permitido { get; set; }
        }

        public class Data
        {
            public User user { get; set; }
            public List<AccionPermitida> accionPermitida { get; set; }
        }

        public class UserData
        {
            public int success { get; set; }
            public string message { get; set; }
            public Data data { get; set; }
        }


    }
}
