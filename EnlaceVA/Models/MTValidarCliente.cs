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
    public class MTValidarCliente
    {

		public string estado { get; set; }	

		public Mensaje mensajes { get; set; }

        public MTValidarCliente(object mTValidaCliente)
        {
            var mTmTValidaClienteXml = (IEnumerable<XmlNode>)mTValidaCliente;
            mensajes = new Mensaje();            
            estado = mTmTValidaClienteXml.Where(p => p.Name == nameof(estado)).FirstOrDefault()?.InnerText;
            mensajes.descMsj = mTmTValidaClienteXml.Where(p => p.Name == "mensajeError").FirstOrDefault()?.InnerText;
            mensajes.CodMsj = estado;

        }
        public MTValidarCliente()
        {

        }
    }
}
