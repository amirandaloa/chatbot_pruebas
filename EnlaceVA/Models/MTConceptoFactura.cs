using EnlaceVA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using static EnlaceVA.Utilities.DataValidations;
using static EnlaceVA.Utilities.Utility;

namespace EnlaceVA.Models
{
	public class MTConceptoFactura
	{
		public string contrato { get; set; }

		public string solicitud { get; set; }

		public string ultimoPagoRealizado { get; set; }

		public string valorTotalPagar { get; set; }

		public string fechaLimPago { get; set; }

		public Mensaje mensajes { get; set; }

        public string cupon { get; set; }

        public string url { get; set; }

        public MTConceptoFactura(object mT_ConceptoFactura_res, string company)
		{
			var mT_ConceptoFactura_resXml = (IEnumerable<XmlNode>)mT_ConceptoFactura_res;
			mensajes = new Mensaje();
			solicitud = mT_ConceptoFactura_resXml.Where(p => p.Name == nameof(solicitud)).FirstOrDefault()?.InnerText;
			ultimoPagoRealizado = Formatmoney(mT_ConceptoFactura_resXml.Where(p => p.Name == nameof(ultimoPagoRealizado)).FirstOrDefault()?.InnerText,company);
			valorTotalPagar = Formatmoney(mT_ConceptoFactura_resXml.Where(p => p.Name == nameof(valorTotalPagar)).FirstOrDefault()?.InnerText,company);
			fechaLimPago = FormatDate(mT_ConceptoFactura_resXml.Where(p => p.Name == nameof(fechaLimPago)).FirstOrDefault()?.InnerText);
			cupon = mT_ConceptoFactura_resXml.Where(p => p.Name == nameof(cupon)).FirstOrDefault()?.InnerText;
			XmlNodeList mensajeError = mT_ConceptoFactura_resXml.Where(p => p.Name == nameof(mensajes)).FirstOrDefault()?.ChildNodes;
			if (mensajeError != null)
			{
				mensajes.CodMsj = mensajeError[0]?.InnerText;
				mensajes.descMsj = mensajeError[1]?.InnerText;
			}

			url = GetUrlPagosByCompany(company);
		}
		public MTConceptoFactura()
		{

		}
	}
}