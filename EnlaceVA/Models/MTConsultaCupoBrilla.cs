using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using static EnlaceVA.Utilities.Utility;

namespace EnlaceVA.Models
{
    public class MTConsultaCupoBrilla
    {
        public string solicitud { get; set; }

        public string cupo_aprobado { get; set; }

        public string cupo_utilizado { get; set; }

        public string cupo_disponible { get; set; }

        public string estado { get; set; }

        public Mensaje mensajes { get; set; }


        public MTConsultaCupoBrilla(object mTConsultaCupoBrilla,string company)
        {
            var mTConsultaCupoBrillaXml = (IEnumerable<XmlNode>)mTConsultaCupoBrilla;
            mensajes = new Mensaje();
            solicitud = mTConsultaCupoBrillaXml.Where(p => p.Name == nameof(solicitud)).FirstOrDefault()?.InnerText;
            cupo_aprobado = FormatDate(mTConsultaCupoBrillaXml.Where(p => p.Name == nameof(cupo_aprobado)).FirstOrDefault()?.InnerText);
            cupo_utilizado = mTConsultaCupoBrillaXml.Where(p => p.Name == nameof(cupo_utilizado)).FirstOrDefault()?.InnerText;
            cupo_disponible = mTConsultaCupoBrillaXml.Where(p => p.Name == nameof(cupo_disponible)).FirstOrDefault()?.InnerText;
            estado = mTConsultaCupoBrillaXml.Where(p => p.Name == nameof(estado)).FirstOrDefault()?.InnerText;
            mensajes.CodMsj = mTConsultaCupoBrillaXml.Where(p => p.Name == nameof(mensajes)).FirstOrDefault()?.InnerText;
            XmlNodeList mensajeError = mTConsultaCupoBrillaXml.Where(p => p.Name == nameof(mensajes)).FirstOrDefault()?.ChildNodes;
            if (mensajeError != null)
            {
                mensajes.CodMsj = mensajeError[0]?.InnerText;
                mensajes.descMsj = mensajeError[1]?.InnerText;
            }

        }
        public MTConsultaCupoBrilla()
        {

        }
    }
}
