using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using static EnlaceVA.Utilities.DataValidations;
using static EnlaceVA.Utilities.Utility;

namespace EnlaceVA.Models
{
    public class MTEstadoCuenta
    {
        public string contrato { get; set; }

        public string solicitud { get; set; }

        public string valorFactura { get; set; }

        public string saldoPendiente { get; set; }

        public string saldosPendDiferido { get; set; }

        public string codigoError { get; set; }

        public string mensajeError { get; set; }

        public Mensaje mensajes { get; set; }

        public List<Saldo> saldo { get; set; }

        public string url { get; set; }

        public MTEstadoCuenta(object mTEstadoCuenta, string company)
        {
            var mTEstadoCuentaXml = (IEnumerable<XmlNode>)mTEstadoCuenta;
            mensajes = new Mensaje();
            valorFactura = Formatmoney(mTEstadoCuentaXml.Where(p => p.Name == nameof(valorFactura)).FirstOrDefault()?.InnerText, company);
            var saldoVerificar = mTEstadoCuentaXml.Where(p => p.Name == nameof(saldoPendiente)).FirstOrDefault()?.InnerText;
            saldoPendiente = Formatmoney(saldoVerificar, company);
            mensajes.CodMsj = mTEstadoCuentaXml.Where(p => p.Name == nameof(mensajes)).FirstOrDefault()?.InnerText;
            solicitud = mTEstadoCuentaXml.FirstOrDefault(p => p.Name == nameof(solicitud))?.InnerText;
            codigoError = mTEstadoCuentaXml.Where(p => p.Name == nameof(codigoError)).FirstOrDefault()?.InnerText;
            mensajeError = mTEstadoCuentaXml.Where(p => p.Name == nameof(mensajeError)).FirstOrDefault()?.InnerText;

            if (saldoVerificar == "0")
            {
                mensajes.CodMsj = "-12";
            }
            else
            {
                mensajes.CodMsj = codigoError;
                mensajes.descMsj = mensajeError;
            }
            

            XmlNodeList saldosPendDiferidoXml = mTEstadoCuentaXml.Where(p => p.Name == nameof(saldosPendDiferido)).FirstOrDefault()?.ChildNodes;
            if (saldosPendDiferidoXml != null)
            {
                saldo = new List<Saldo>();
                for (int i = 0; i < saldosPendDiferidoXml.Count; i++)
                {
                    XmlNodeList saldoItem = saldosPendDiferidoXml[i].ChildNodes;
                    Saldo saldoPendienteDif = new Saldo();

                    saldoPendienteDif.valor = Formatmoney(saldoItem[1]?.InnerText.ToString(), company);
                    saldoPendienteDif.servicio = saldoItem[0]?.InnerText == "Servicios Financieros" ? "Brilla":saldoItem[0]?.InnerText;
                  
                    saldo.Add(saldoPendienteDif);
                }
            }

            url = GetUrlPagosByCompany(company);
        }
        public MTEstadoCuenta()
        {

        }
    }

    public class Saldo
    {
        public string servicio { get; set; }

        public string valor { get; set; }
    }
}
