using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Models;

namespace ConnectionApi_Pos_Rios.Service
{
    /// <summary>
    ///  Clase que consume el servicio de cliente del Api Pos Rio
    /// </summary>

    public class ClientServiceApi
    {
        private ContextHttp _contextHttp { get; set; }


        public ClientServiceApi()
        {
            _contextHttp = new ContextHttp();
        }


        public string SetCliente(Client clienteModeloDatos)
        {
            //return _contextHttp.Get_Data(DefUrl.GetUrlApi("client/actualizar/sin/producto/" + Codigo + "/user/" + IdUser + "/device/" + IdDevice));
            string JsonClient = JsonConvert.SerializeObject(clienteModeloDatos);
            return _contextHttp.EnviarDatosJson("client", JsonClient);
        }

        public string PutClient(Client clienteModeloDatos)
        {
            throw new NotImplementedException();
        }

        public string GetClient(string Rif)
        {
            string JsonClient = JsonConvert.SerializeObject(Rif);
            return _contextHttp.Get_Data(DefUrl.GetUrlApi("client/") + Rif);
            
        }
    }
}
