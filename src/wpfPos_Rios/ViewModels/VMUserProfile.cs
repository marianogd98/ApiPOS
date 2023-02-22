using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;

namespace wpfPos_Rios.ViewModels
{
    public static class VMUserProfile
    {
        public static string name { get; set; }
        public static List<Accion>  ActionsList { get; set; }
        public static string rolUser { get; set; }
        public static int idTienda { get; set; }
        public static bool status { get; set; }

    }
}
