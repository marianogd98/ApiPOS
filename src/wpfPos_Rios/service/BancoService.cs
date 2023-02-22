using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfPos_Rios.ViewModels.helperResponse;

namespace wpfPos_Rios.service
{
    public class BancoService
    {
        RequestHttp request;

        public BancoService()
        {
            request = new RequestHttp();
        }
        public Response GetBancos()
        {
            Response res;
            res = request.SendRequestGet( RequestHttp.GetUrlApi("banco") );
            return res;
        }
    }
}
