using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace EnlaceVA.Models
{
    public class MTConsumoFacturado
    {
        public string contrato { get; set; }

        public string solicitud { get; set; }

        public string desviacion { get; set; }

        public string estado { get; set; }

        public Mensaje mensajes { get; set; }

        public MTConsumoFacturado(object mTConsumoFacturado)
        {
            var mTConsumoFacturadoXml = (IEnumerable<XmlNode>)mTConsumoFacturado;
            mensajes = new Mensaje();
            solicitud = mTConsumoFacturadoXml.Where(p => p.Name == nameof(solicitud)).FirstOrDefault()?.InnerText;
            desviacion = mTConsumoFacturadoXml.Where(p => p.Name == nameof(desviacion)).FirstOrDefault()?.InnerText;
            estado = mTConsumoFacturadoXml.Where(p => p.Name == nameof(estado)).FirstOrDefault()?.InnerText;
            mensajes.CodMsj = estado;

        }
        public MTConsumoFacturado()
        {

        }
    }
}
