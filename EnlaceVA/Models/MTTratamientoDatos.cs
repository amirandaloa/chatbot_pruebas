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
    public class MTTratamientoDatos
    {
        public string estado { get; set; }

        public Mensaje mensajes { get; set; }

        public MTTratamientoDatos(object mTTratamientoDatos)
        {
            var mTTratamientoDatosXml = (IEnumerable<XmlNode>)mTTratamientoDatos;
            mensajes = new Mensaje();
            estado = mTTratamientoDatosXml.Where(p => p.Name == nameof(estado)).FirstOrDefault()?.InnerText;
            mensajes.descMsj = mTTratamientoDatosXml.Where(p => p.Name == "mensajeError").FirstOrDefault()?.InnerText;
            mensajes.CodMsj = estado;
        }

        public MTTratamientoDatos()
        {

        }
    }
}
