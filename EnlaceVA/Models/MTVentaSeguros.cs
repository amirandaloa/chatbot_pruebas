using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace EnlaceVA.Models
{
    public class MTVentaSeguros
    {       
        public string solicitud { get; set; }
        
        public string estado { get; set; }

        public Mensaje mensajes { get; set; }

        public MTVentaSeguros(object mTVentaSeguros)
        {
            var mTVentaSegurosXml = (IEnumerable<XmlNode>)mTVentaSeguros;
            mensajes = new Mensaje();
            solicitud = mTVentaSegurosXml.Where(p => p.Name == nameof(solicitud)).FirstOrDefault()?.InnerText;            
            estado = mTVentaSegurosXml.Where(p => p.Name == nameof(estado)).FirstOrDefault()?.InnerText;
            mensajes.descMsj = mTVentaSegurosXml.Where(p => p.Name == "mensajeError").FirstOrDefault()?.InnerText;
            mensajes.CodMsj = estado;

        }
        public MTVentaSeguros()
        {

        }
    }
}
