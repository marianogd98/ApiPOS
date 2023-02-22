using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wpfPos_Rios.ViewModels.helperResponse;

namespace wpfPos_Rios.service
{
    class InvoiceService
    {
        RequestHttp request;
        public InvoiceService()
        {
            request = new RequestHttp();
        }

        public Response InsertInvoice(Invoice factura , string path = "invoice/facturar")
        {
            Response res;
            res  = request.SendRequestPost(RequestHttp.GetUrlApi(path) , factura );
            return res;
        }
        public Response UpdateInvoice(int id , string path = "actualizar-factura")
        {
            Response res;
            res = request.SendRequestPost(RequestHttp.GetUrlApi(path) , id);
            return res;
        } 
        public Response CancelInvoice(int id , string path = "invoice/cancel")
        {
            Response res;
            res = request.SendRequestPost(RequestHttp.GetUrlApi(path), id);
            return res;
        }
        public Response SaveInvoice(Invoice pInvoice , string path = "guardar-factura")
        {
            Response res;
            res = request.SendRequestPost(RequestHttp.GetUrlApi(path), pInvoice);
            return res;
        }
        public Response GetInvoicesPending(int IdTienda , int IdCaja)
        {
            Response res;
            res = request.SendRequestGet(RequestHttp.GetUrlApi("invoice/pendientes/") + IdCaja + "/" + IdTienda);
            return res;
        }
        public Response GetInvoicePending(int idInvoice)
        {
            Response res;
            res = request.SendRequestGet(RequestHttp.GetUrlApi("invoice/pendiente/") + idInvoice);
            return res;
        }  
        public Response GetInvoice(string pNumeroFactura, int pIdTienda, int pIdCaja, string pSerialFiscal)
        {
            BodyJsonGetInvoice data = new BodyJsonGetInvoice();
            data.IdCaja = pIdCaja;
            data.NFactura = pNumeroFactura;
            data.IdTienda = pIdTienda;
            data.SerialFiscal = pSerialFiscal;

            Response res;
            res = request.SendRequestPost(RequestHttp.GetUrlApi("invoice") , data);
            return res;
        }
        public Response GetInvoiceById(int id)
        {
            Response res;
            res = request.SendRequestPost(RequestHttp.GetUrlApi("invoiceid") , JsonConvert.SerializeObject(id));
            return res;
        }

        public Response ReturnParcialInvoice(InvoiceToReturn pInvoiceReturn)
        {
            Response res;
            res = request.SendRequestPost(RequestHttp.GetUrlApi("invoice/devolucion/parcial") , pInvoiceReturn);
            return res;
        }
        public Response ReturnTotalInvoice(InvoiceToReturn pInvoiceReturn)
        {
            Response res;
            res = request.SendRequestPost(RequestHttp.GetUrlApi("invoice/devolucion/total"), pInvoiceReturn);
            return res;
        }

        public Response GetPromocion(Invoice invoice)
        {
            Response res;
            BodyPromocion body = new BodyPromocion();
            body.ProductInToInvoice = new List<Product>();

            invoice.listaProductos.ForEach(p => {
                body.ProductInToInvoice.Add(new Product() {
                    CodigoProducto = int.Parse(p.CodigoProducto).ToString(),
                    Descripcion = p.Descripcion
                });
            });

            body.TotalInvoice = Math.Round(invoice.MontoNeto /  invoice.tasa , 2);

            res = request.SendRequestPost(RequestHttp.GetUrlApi("/promocion"), body);
            return res;
        }

        private class BodyJsonGetInvoice
        {
            public int IdCaja { get; set; }
            public string NFactura { get; set; }
            public int IdTienda { get; set; }
            public string SerialFiscal { get; set; }
        }
        public class BodyPromocion
        {
            public List<Product> ProductInToInvoice { get; set; }
            public double TotalInvoice { get; set; }
        }
    }
}
