using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;
using wpfPos_Rios.ViewModels;
using wpfPos_Rios.ViewModels.helperResponse;
using Newtonsoft.Json;


namespace wpfPos_Rios.service.Security
{
    public class SecurityService
    {

        UserService _ServiceApi;

        public SecurityService()
        {
            _ServiceApi = new UserService();
        }


        public Response ValidarAccciones(int pId, string pAccion)
        {
            Response res = _ServiceApi.ValidateAction(pId, pAccion);
            return res;

        }

        public Response UserLogin(string pUser, string pPasscode , int idCaja)
        {
            Response res = _ServiceApi.Login(pUser, pPasscode, idCaja);
            return res;
        }

        public Response UserLoginAction(string pUser, string pPasscode, string pAction)
        {
            Response res = _ServiceApi.LoginActionUser(pUser, pPasscode, pAction);
            return res;        
        }



    }
}
