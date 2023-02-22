using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using wpfPos_Rios.ViewModels.helperResponse;
using Newtonsoft.Json;
using wpfPos_Rios.ViewModels;
using Models;

namespace wpfPos_Rios.service
{
    public class RequestHttp
    {
        //esta clase nos sirve para tener control del resultado regresado por la solicitud
        
        public static string GetUrlApi(string seccion)
        {
            string url = $"{ConfigPosStatic.TiendaConfig.ipServerPos}api/{seccion}";
            return url;
        }

        public static string GetUrlSecurityApi(string seccion)
        {
            string url = $"{ConfigPosStatic.TiendaConfig.ipServerSec}api/{seccion}";
            return url;
        }

        public static string GetUrlVpos(string seccion)
        {
            string url = $"{ConfigPosStatic.vposConfig.uri}api/{seccion}";
            return url;
        }


        public Response SendRequestPost<T>(string url, T objectRequest, string method = "POST")
        {

            Response oReply = new Response();
            try
            {
                //serializamos el objeto
                string json = JsonConvert.SerializeObject(objectRequest);

                //peticion
                WebRequest request = WebRequest.Create(url);
                //headers
                request.Method = method;
                request.PreAuthenticate = true;
                //request.ContentType = "application/json;charset=utf-8'";
                request.ContentType = "application/json";
                //request.Timeout = 10000; //esto es opcional

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    //streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    oReply = JsonConvert.DeserializeObject<Response>(streamReader.ReadToEnd());
                }
            }
            catch (WebException e)
            {
                if (e.Response == null && e.Status != WebExceptionStatus.Success)
                {
                    throw new Exception(e.Message);
                }

                var resp = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                oReply = JsonConvert.DeserializeObject<Response>(resp);
                
                if (string.IsNullOrEmpty(oReply.message))
                    oReply.message = e.Message;
            }
            return oReply;
        }

        public Response SendRequestGet(string url , string method = "GET")
        {
            Response oReply = new Response();
            try
            {
                //peticion
                WebRequest request = WebRequest.Create(url);
                WebResponse response = request.GetResponse();
                //headers
                request.Method = method;
                request.PreAuthenticate = true;
                request.Timeout = 10000; //esto es opcional

                Stream res = response.GetResponseStream();
                StreamReader resContent = new StreamReader(res);

                oReply = JsonConvert.DeserializeObject<Response>(resContent.ReadToEnd());
            }
            catch (WebException e)
            {
                if (e.Response == null && e.Status != WebExceptionStatus.Success)
                {
                    throw new Exception(e.Message);
                }

                var resp = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                oReply = JsonConvert.DeserializeObject<Response>(resp);
            }
            return oReply;
        }


    }
}
