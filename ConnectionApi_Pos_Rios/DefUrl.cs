using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionApi_Pos_Rios
{
    class DefUrl
    {
        //public const string UrlServer = "https://localhost:44357/";
       // public const string UrlServer = "http://100.100.2.140:8300/";

        public static string GetUrlApi(string seccion)
        {
            return ConfigurationManager.AppSettings.Get("IpServidorA") + "api/" + seccion;
        }

        public static string GetUrlSecurityApi(string seccion)
        {
            return ConfigurationManager.AppSettings.Get("IpServidorB") + "api/" + seccion;
        }

        public static string GetUrlVpos(string seccion)
        {
            return ConfigurationManager.AppSettings.Get("urlVpos") + "vpos/" + seccion;
        }

        public static string GetUrlImage(string imageUrl)
        {
            return GetIpConfig() + imageUrl;
        }

        ///<summary> IPInside. </summary>
        public static string GetIpInside()
        {
            return "http://100.100.2.131:8300/";
        }

        ///<summary> IPPlaya el Angel. </summary>
        public static string GetIpPlaya()
        {
            return "http://10.10.21.2:8080/";
        }

        ///<summary> IPTraki. </summary>
        public static string GetIpTraki()
        {
            return "http://10.10.10.4:8080/";
        }

        ///<summary> IP Juan Bautista. </summary>
        public static string GetIpJb()
        {
            return "http://10.10.0.77:8080/";
        }

        public static string GetIpConfig()
        {
            var appSettings = ConfigurationManager.AppSettings;
            string server = appSettings["Server"] ?? "Not Found";

            if (server.Equals("Not Found"))
            {
                return "";
            }
            else
            {
                return server;
            }
        }
    }
}
