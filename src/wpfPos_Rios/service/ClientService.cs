using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;
using wpfPos_Rios.ViewModels.helperResponse;

namespace wpfPos_Rios.service
{
    public class ClientService
    {
        RequestHttp request;

        public ClientService()
        {
            request = new RequestHttp();
        }


        public Response GuardarCambios(Client cliente)
        {
            Response res;
            res = request.SendRequestPost(RequestHttp.GetUrlApi("client") , cliente);
            return res;
        }

        public Response GetClient(string pRif)
        {
            Response res;
            res = request.SendRequestGet(RequestHttp.GetUrlApi("client/") + pRif);
            return res;
        }
     

    }
}
