using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using static EnlaceVA.Utilities.DataValidations;

namespace EnlaceVA.Models
{
    public class MTPagoParcial
    {
        public string solicitud { get; set; }        

        public string estado { get; set; }

        public string nroDocumentoGenerado { get; set; }
        public string url { get; set; }

        public Mensaje mensajes { get; set; }

        public MTPagoParcial(object mTPagoparcial, string company)
        {
            var mTPagoparcialXml = (IEnumerable<XmlNode>)mTPagoparcial;
            mensajes = new Mensaje();
            solicitud = mTPagoparcialXml.Where(p => p.Name == nameof(solicitud)).FirstOrDefault()?.InnerText;            
            estado = mTPagoparcialXml.Where(p => p.Name == nameof(estado)).FirstOrDefault()?.InnerText;
            nroDocumentoGenerado = mTPagoparcialXml.Where(p => p.Name == nameof(nroDocumentoGenerado)).FirstOrDefault()?.InnerText;
            url = GetUrlPagosByCompany(company);

            mensajes.CodMsj = estado;            
        }
        public MTPagoParcial()
        {

        }
    }
}
