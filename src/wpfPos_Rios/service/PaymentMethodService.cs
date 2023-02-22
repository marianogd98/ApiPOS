using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfPos_Rios.ViewModels.helperResponse;

namespace wpfPos_Rios.service
{
    class PaymentMethodService
    {
        RequestHttp request;

        public PaymentMethodService()
        {
            request = new RequestHttp();
        }


        public Response GetPaymentMethodDonate(string path = "payment/method/donations")
        {
            Response res;
            res = request.SendRequestGet(RequestHttp.GetUrlApi(path));
            return res;
        }


    }
}
