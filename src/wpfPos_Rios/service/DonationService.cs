using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfPos_Rios.ViewModels.helperResponse;
namespace wpfPos_Rios.service
{
    class DonationService
    {
        RequestHttp _request;
        public DonationService()
        {
            _request = new RequestHttp();
        }

        public Response GetReasonDonations(string path = "donate/razones")
        {
            Response res;
            res = _request.SendRequestGet(RequestHttp.GetUrlApi(path));
            return res;
        }
         public Response PostDonate(Donation donation  , string path = "donate")
        {
            Response res;
            res = _request.SendRequestPost(RequestHttp.GetUrlApi(path) , donation);
            return res;
        }


    }
}
