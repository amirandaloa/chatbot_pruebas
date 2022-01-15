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
    public class MTValidarContrato
    {
		public string estado { get; set; }	

		public Mensaje mensajes { get; set; }

        public MTValidarContrato(object mTValidaContrato)
        {
            var mTmTValidaContratoXml = (IEnumerable<XmlNode>)mTValidaContrato;
            mensajes = new Mensaje();            
            estado = mTmTValidaContratoXml.Where(p => p.Name == nameof(estado)).FirstOrDefault()?.InnerText;
            mensajes.descMsj = mTmTValidaContratoXml.Where(p => p.Name == "mensajeError").FirstOrDefault()?.InnerText;
            mensajes.CodMsj = estado;

        }
        public MTValidarContrato()
        {

        }
    }
}
