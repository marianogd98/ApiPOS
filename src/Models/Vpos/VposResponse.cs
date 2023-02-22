using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Vpos
{
    public class VposResponse
    {
        public string codRespuesta { get; set; }
        public string mensajeRespuesta { get; set; }
        public string nombreVoucher { get; set; }
        public int numSeq { get; set; }
        public string numeroTarjeta { get; set; }
        public string cedula { get; set; }
        public string montoTransaccion { get; set; }
        public string montoAvance { get; set; }
        public string montoServicios { get; set; }
        public string montoServiciosAprobado { get; set; }
        public string montoDonativo { get; set; }
        public string tipoCuenta { get; set; }
        public string tipoTarjeta { get; set; }
        public string fechaExpiracion { get; set; }
        public bool existeCopiaVoucher { get; set; }
        public string fechaTransaccion { get; set; }
        public string horaTransaccion { get; set; }
        public string terminalVirtual { get; set; }
        public string tipoTransaccion { get; set; }
        public string numeroAutorizacion { get; set; }
        public string codigoAfiliacion { get; set; }
        public string tid { get; set; }
        public string numeroReferencia { get; set; }
        public string nombreAutorizador { get; set; }
        public string codigoAdquiriente { get; set; }
        public string numeroLote { get; set; }
        public string tipoProducto { get; set; }
        public string bancoEmisorCheque { get; set; }
        public string numeroCuenta { get; set; }
        public string numeroCheque { get; set; }
        public string tipoMonedaFiat { get; set; }
        public string descrMonedaFiat { get; set; }
        public string montoCriptomoneda { get; set; }
        public string tipoCriptomoneda { get; set; }
        public string descrCriptomoneda { get; set; }
        public string archivoCierre { get; set; }
        public bool flagImpresion { get; set; }
    }
}
