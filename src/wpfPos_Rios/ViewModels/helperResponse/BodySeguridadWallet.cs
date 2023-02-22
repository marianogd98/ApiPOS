using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfPos_Rios.ViewModels.helperResponse
{
    public class BodySeguridadWallet
    {
        public int IdClient { get; set; }
        public string Password { get; set; }
    }
}
