using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfPos_Rios.ViewModels.helperResponse;

namespace wpfPos_Rios.service
{
    public class ProductService
    {
        RequestHttp request;

        public ProductService()
        {
            request = new RequestHttp();
        }

        public Response GetProductsbyParameter(string pDescripcion)
        {
            Response res = request.SendRequestGet(RequestHttp.GetUrlApi("products/") + pDescripcion);
            return res;
        }


        public Response GetProductsbyCode(string pCode)
        {
            Response res = request.SendRequestGet(RequestHttp.GetUrlApi("product/") + pCode);
            return res;
        }
       
        public Response GetProductsArea(int codArea)
        {
            Response res = request.SendRequestGet(RequestHttp.GetUrlApi("products-area/") + codArea);
            return res;
        }

       
    }
}
