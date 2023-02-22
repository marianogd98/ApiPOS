using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfPos_Rios.ViewModels.helperResponse;
using Newtonsoft.Json;
using Models.Vpos;
using System.Net.Http;
using System.Net;
using wpfPos_Rios.helper;
using RestSharp;
namespace wpfPos_Rios.service
{
    class VposService
    {

        public HttpClient client;
        private const string url = "http://localhost:8085/vpos/";
        public VposService()
        {
            client = new HttpClient();
        }

        // HttpClient is intended to be instantiated once per application, rather than per-use. See Remarks.
        
        private string GetUrl(string path = "")
        {
            return url + path;
        }

        public VposResponse VposAsync(string pDataJson)
        {
            try
            {
                var url = GetUrl("metodo");
                var client = new RestClient(url);
                var request = new RestRequest(Method.POST);
                request.AddHeader("cache-control" , "no-cache");
                request.AddHeader("content-type", "application/json");
                request.AddParameter("application/json", pDataJson ,ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                return JsonConvert.DeserializeObject<VposResponse>(response.Content);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\n" + ex.StackTrace.ToString() );
            } 
          
        }


        public VposResponse BuyWithCard( string pMontoTrassaccion , string pCedula , string pAccion = "tarjeta")
        {
            try
            {
                VposResponse request = new VposResponse();
                string bodyJson = JsonConvert.SerializeObject(new { accion = pAccion, montoTransaccion = pMontoTrassaccion, cedula = pCedula });
                request = VposAsync(bodyJson);
                return request;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public VposResponse ClosePoint( string pAccion = "cierre")
        {
            try
            {
                VposResponse request = new VposResponse();
                string bodyJson = JsonConvert.SerializeObject(new { accion = pAccion});
                request = VposAsync(bodyJson);
                return request;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }  
        public VposResponse PreClosePoint( string pAccion = "precierre")
        {
            try
            {
                VposResponse request = new VposResponse();
                string bodyJson = JsonConvert.SerializeObject(new { accion = pAccion });
                request = VposAsync(bodyJson);
                return request;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        } 
        
        public VposResponse GetLastVoucher( string pAccion = "imprimeUltimoVoucher")
        {
            try
            {
                VposResponse request = new VposResponse();
                string bodyJson = JsonConvert.SerializeObject(new { accion = pAccion });
                request = VposAsync(bodyJson);
                return request;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public VposResponse GetLastVoucherProcess( string pAccion = "imprimeUltimoVoucherP")
        {
            try
            {
                VposResponse request = new VposResponse();
                string bodyJson = JsonConvert.SerializeObject(new { accion = pAccion });
                request = VposAsync(bodyJson);
                return request;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



    }
}
