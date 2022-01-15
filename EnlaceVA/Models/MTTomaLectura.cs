using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using static EnlaceVA.Utilities.Utility;

namespace EnlaceVA.Models
{
    public class MTTomaLectura
    {
        public string contrato { get; set; }
        public string solicitud { get; set; }

        public string fechaDeLectura { get; set; }

        public string valorDeLectura { get; set; }

        public Mensaje mensajes { get; set; }

        public Datos datos { get; set; }


        public MTTomaLectura(object mTTomaLectura,string company)
        {
            var mTTomaLecturaXml = (IEnumerable<XmlNode>)mTTomaLectura;
            mensajes = new Mensaje();
            XmlNodeList DatosEt = mTTomaLecturaXml.Where(p => p.Name == nameof(datos)).FirstOrDefault()?.ChildNodes;
            
            solicitud = DatosEt[0]?.InnerText;

            //se ajusta para enviar solo la fecha cp 90 gdg aml 2021-01-05
            var fechahoraLectura = DatosEt[1]?.InnerText;
            if (string.IsNullOrEmpty(fechahoraLectura))
            {
                fechahoraLectura = "no-registra";
            }

            if (solicitud != "0")
            {
                solicitud = DatosEt[0]?.InnerText;
                fechaDeLectura = fechahoraLectura.Split(" ")[0];
                valorDeLectura = DatosEt[2]?.InnerText;
                mensajes.CodMsj = DatosEt[3]?.InnerText;
                mensajes.descMsj = DatosEt[4]?.InnerText;
            }
            else
            {
                solicitud = DatosEt[0]?.InnerText;
                mensajes.CodMsj = fechahoraLectura.Split(" ")[0];
                mensajes.descMsj = DatosEt[2]?.InnerText;
            }
            
            /* XmlNodeList mensajeError = mTTomaLecturaXml.Where(p => p.Name == nameof(mensajes)).FirstOrDefault()?.ChildNodes;*/
            /*if (mensajeError != null)
            {
                mensajes.CodMsj = mensajeError[0]?.InnerText;
                mensajes.descMsj = mensajeError[1]?.InnerText;
            }*/

        }
        public MTTomaLectura()
        {

        }
    }
}
