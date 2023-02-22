using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace ConnectionApi_Pos_Rios.Service
{
   public class ReportsServiceApi
    {
        //private ContextHttp _contextHttp { get; set; }


        RequestHttp request = new RequestHttp();

        public ReportsServiceApi()
        {

        }


        //public string GetPaymentReportsClosePos(string date, int idcaja)
        //{
        //    return _contextHttp.Get_Data(DefUrl.GetUrlSecurityApi("seguridad/reportecierrecaja/fecha/") + date + "/caja/" +idcaja );
        //}
        //public string GetPaymentReportsCloseTurn(string date, int idcaja, int idTurno)
        //{
        //    return _contextHttp.Get_Data(DefUrl.GetUrlSecurityApi("seguridad/reportecierreturno/fecha/") + date + "/caja/" + idcaja+"/turno/"+idTurno);
        //}
    }
}
