using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace EnlaceVA.Models
{
    public class MTReconexionPorPago
    {
        public string solicitud { get; set; }

        public string existe_reconexion { get; set; }

        public string tiempo_reconexion { get; set; }

        public string estado { get; set; }

        public Mensaje mensajes { get; set; }

        public MTReconexionPorPago(object mTReconexionPorPago)
        {
            var mTReconexionPorPagoXml = (IEnumerable<XmlNode>)mTReconexionPorPago;
            mensajes = new Mensaje();
            solicitud = mTReconexionPorPagoXml.Where(p => p.Name == nameof(solicitud)).FirstOrDefault()?.InnerText;
            existe_reconexion = mTReconexionPorPagoXml.Where(p => p.Name == nameof(existe_reconexion)).FirstOrDefault()?.InnerText;
            tiempo_reconexion = mTReconexionPorPagoXml.Where(p => p.Name == nameof(tiempo_reconexion)).FirstOrDefault()?.InnerText;
            estado = mTReconexionPorPagoXml.Where(p => p.Name == nameof(estado)).FirstOrDefault()?.InnerText;
            mensajes.descMsj = mTReconexionPorPagoXml.Where(p => p.Name == "mensajeError").FirstOrDefault()?.InnerText;
            mensajes.CodMsj = estado;

        }
        public MTReconexionPorPago()
        {

        }
    }
}
