using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfPos_Rios.ViewModels.helperResponse;

namespace wpfPos_Rios.service
{
    class SeguridadWalletService
    {
        RequestHttp request;
        public SeguridadWalletService()
        {
            request = new RequestHttp();
        }

        public Response ClientPassword(int idClient)
        {
            Response res;
            res = request.SendRequestGet(RequestHttp.GetUrlApi("SeguridadWallet/") + idClient);
            return res;
        }
        public Response CreatePassword(BodySeguridadWallet data)
        {
            Response res;
            res = request.SendRequestPost(RequestHttp.GetUrlApi("SeguridadWallet/") , data );
            return res;
        }
        public Response UpdatePassword(BodySeguridadWallet data)
        {
            Response res;
            res = request.SendRequestPost(RequestHttp.GetUrlApi("SeguridadWallet/update"), data);
            return res;
        }

        public Response authenticationClient(BodySeguridadWallet data)
        {
            Response res;
            res = request.SendRequestPost(RequestHttp.GetUrlApi("SeguridadWallet/auth") , data );
            return res;
        }
    

    }
}
