using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace EnlaceVA.Models
{
    public class MTVisitaFinNoBanca
    {
        public string solicitud { get; set; }

        public string estado { get; set; }

        public Mensaje mensajes { get; set; }

        public MTVisitaFinNoBanca(object mTVisitaFinNoBanca)
        {
            var mTVisitaFinNoBancaXml = (IEnumerable<XmlNode>)mTVisitaFinNoBanca;
            mensajes = new Mensaje();
            solicitud = mTVisitaFinNoBancaXml.Where(p => p.Name == nameof(solicitud)).FirstOrDefault()?.InnerText;
            estado = mTVisitaFinNoBancaXml.Where(p => p.Name == nameof(estado)).FirstOrDefault()?.InnerText;
            mensajes.descMsj = mTVisitaFinNoBancaXml.Where(p => p.Name == "mensajeError").FirstOrDefault()?.InnerText;
            mensajes.CodMsj = estado;
        }

        public MTVisitaFinNoBanca()
        {

        }
    }
}
