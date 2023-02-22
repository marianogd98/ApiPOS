using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfPos_Rios.ViewModels.helperResponse;

namespace wpfPos_Rios.service
{
    public class UserService
    {

        RequestHttp request;
        public UserService()
        {
            request = new RequestHttp();
        }


        public Response Login(string pUsername , string pPassword , int pIdCaja)
        {
            Response res;
            loginData data = new loginData() { idCaja = pIdCaja.ToString(), password = pPassword, username = pUsername };
            res = request.SendRequestPost(RequestHttp.GetUrlSecurityApi("usuario/Login"), data );
            return res;
        }

        public Response GetUserProfile(int pIdUser)
        {
            Response res;
            res = request.SendRequestGet(RequestHttp.GetUrlSecurityApi("seguridad/perfil/") + pIdUser);
            return res;
        }

        public Response LoginActionUser(string pUsername, string pPasscode, string pAction)
        {
            object data = new { username = pUsername, password = pPasscode, accion = pAction };
            Response Result = request.SendRequestPost(RequestHttp.GetUrlSecurityApi("Seguridad/PermisoUser"), data);
            return Result;
        }

        public Response ValidateAction(int pIdUser, string pAction)
        {
            Response Result = request.SendRequestGet(RequestHttp.GetUrlSecurityApi("seguridad/permisosaccion/") + pIdUser + "/" + pAction);
            return Result;

        }

        public class loginData
        {
            public string username { get; set; }
            public string idCaja { get; set; }
            public string password { get; set; }
        }

    }
}
