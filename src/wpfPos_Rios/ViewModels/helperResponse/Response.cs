using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfPos_Rios.ViewModels.helperResponse
{
    public class Response
    {
        public int success { get; set; }
        public string message { get; set; }
        public object data { get; set; }
    }
}
