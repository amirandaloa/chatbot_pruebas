using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace EnlaceVA.Models
{
    public class MTConsultaEstado
    {
        public string solicitud { get; set; }

        public string estado { get; set; }

        public List<descEstado> descEstado { get; set; }

        public string codError { get; set; }

        public string mensajeError { get; set; }

        public MTConsultaEstado( object mTConsultaEstado)
        {
            var mTConsultaEstado_resXml = (IEnumerable<XmlNode>)mTConsultaEstado;
            List<string> xmlsItem = mTConsultaEstado_resXml.Where(p => p.Name == nameof(descEstado)).ToList().Select(x => x.OuterXml).ToList();
            List<descEstado> descEstado_ = DeserializeXMLFileToObject<List<descEstado>>(xmlsItem);
            solicitud = mTConsultaEstado_resXml.Where(p => p.Name == nameof(solicitud)).FirstOrDefault()?.InnerText;
            estado = descEstado_.FirstOrDefault().estado_solicitud;
            descEstado = descEstado_;
            codError = mTConsultaEstado_resXml.Where(p => p.Name == nameof(codError)).FirstOrDefault()?.InnerText;
            mensajeError = mTConsultaEstado_resXml.Where(p => p.Name == nameof(mensajeError)).FirstOrDefault()?.InnerText;
        }
        public MTConsultaEstado()
        {

        }

        public static List<descEstado> DeserializeXMLFileToObject<T>(List<string> XmlFilename)
        {


            List<descEstado> returnObject = new List<descEstado>();

            foreach (var item in XmlFilename)
            {
                if (string.IsNullOrEmpty(item)) return default(List<descEstado>);

                try
                {

                    XmlSerializer serializer = new XmlSerializer(typeof(descEstado));
                    using (TextReader reader = new StringReader(item))
                    {
                        var returnObject_ = (descEstado)serializer.Deserialize(reader);
                        returnObject.Add(returnObject_);

                    }

                }
                catch (Exception ex)
                {
                }
            }

            

            return returnObject;
        }
    }

    [XmlRoot("descEstado")]
    public class descEstado
    {
        [XmlElement("numero_solicitud")]
        public string numero_solicitud { get; set; }
        [XmlElement("codigo_tipo_sol")]
        public string codigo_tipo_sol { get; set; }
        [XmlElement("tipo_solicitud")]
        public string tipo_solicitud { get; set; }
        [XmlElement("codigo_estado_sol")]
        public string codigo_estado_sol { get; set; }
        [XmlElement("estado_solicitud")]
        public string estado_solicitud { get; set; }
        [XmlElement("orden")]
        public string orden { get; set; }
        [XmlElement("codigo_tipo_trabajo")]
        public string codigo_tipo_trabajo { get; set; }
        [XmlElement("tipo_trabajo")]
        public string tipo_trabajo { get; set; }
        [XmlElement("codigo_estado_orde")]
        public string codigo_estado_orde { get; set; }
        [XmlElement("estado_orden")]
        public string estado_orden { get; set; }
        [XmlElement("codigo_causal")]
        public string codigo_causal { get; set; }
        [XmlElement("causal_orden")]
        public string causal_orden { get; set; }
        [XmlElement("codigo_clase_causal")]
        public string codigo_clase_causal { get; set; }
        [XmlElement("clase_causal")]
        public string clase_causal { get; set; }
    }
}
