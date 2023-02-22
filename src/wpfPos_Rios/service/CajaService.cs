using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;
using Newtonsoft.Json;
using wpfPos_Rios.ViewModels.helperResponse;

namespace wpfPos_Rios.service
{
    class CajaService
    {
        RequestHttp request;
        public CajaService()
        {
            request = new RequestHttp();
        }
        public Response GetConfigCaja(string codigoCaja)
        {
            Response res;
            res = request.SendRequestGet( RequestHttp.GetUrlApi("caja/") + codigoCaja);
            return res;
        }
        //0 o cualquier numero para suspender
        //1 para cerrar turno
        public Response CloseTurn(int idTunr, int accion = 0)
        {
            Response res;
            res = request.SendRequestGet(RequestHttp.GetUrlApi("caja/cerrar/") + idTunr + "/" + accion);
            return res;
        }
        public Response SaveZ(ReporteZeta reportZ)
        {
            Response res;
            res = request.SendRequestPost(RequestHttp.GetUrlApi("caja/guardar/zeta") , reportZ);
            return res;
        }

    }
}
