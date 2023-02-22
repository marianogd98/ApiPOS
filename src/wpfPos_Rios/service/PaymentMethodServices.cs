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
    class PaymentMethodServices
    {
        RequestHttp request;
        public PaymentMethodServices()
        {
            request = new RequestHttp();
        }


        public Response GetPaymentMethods()
        {
            Response res = new Response();
            res = request.SendRequestGet(RequestHttp.GetUrlApi("PaymentMethod"));
            return res;
        } 
        
        public Response GetPaymentReportsClosePos(string date,int idcaja)
        {
            Response res = new Response();
            res = request.SendRequestGet(RequestHttp.GetUrlSecurityApi("seguridad/reportecierrecaja/fecha/") + date + "/caja/" + idcaja );
            return res;
        } 
        public Response GetPaymentReportsCloseTurn(string date, int idcaja, int idTurno)
        {
            Response res = new Response();
            res = request.SendRequestGet(RequestHttp.GetUrlSecurityApi("seguridad/reportecierreturno/fecha/") + date + "/caja/" + idcaja + "/turno/" + idTurno);
            return res;
        }


    }
}
