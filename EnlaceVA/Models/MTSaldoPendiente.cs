using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace EnlaceVA.Models
{
    public class MTSaldoPendiente
    {
        public string saldoPendiente { get; set; }

        public string estado { get; set; }

        public Mensaje mensajes { get; set; }

        public MTSaldoPendiente(object mTSaldoPendiente)
        {
            var mTSaldoPendienteXml = (IEnumerable<XmlNode>)mTSaldoPendiente;
            mensajes = new Mensaje();
            saldoPendiente = mTSaldoPendienteXml.Where(p => p.Name == nameof(saldoPendiente)).FirstOrDefault()?.InnerText;
            estado = mTSaldoPendienteXml.Where(p => p.Name == nameof(estado)).FirstOrDefault()?.InnerText;
            mensajes.descMsj = mTSaldoPendienteXml.Where(p => p.Name == "mensajeError").FirstOrDefault()?.InnerText;
            mensajes.CodMsj = estado;

        }
        public MTSaldoPendiente  ()
        {

        }
    }
}
