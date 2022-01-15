using System;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace EnlaceVA.Utilities
{
    public static class SerializerExtensions
    {
        private static readonly Type[] WriteTypes = {
            typeof(string), typeof(DateTime), typeof(Enum), typeof(decimal),
            typeof(Guid)
        };

        private static bool IsSimpleType(this Type type)
        {
            return type.IsPrimitive || WriteTypes.Contains(type);
        }

        private static string GetTypeName(this Type type)
        {
            if (type == typeof(bool))
                return "boolean";
            if (type == typeof(short) ||
                type == typeof(int) ||
                type == typeof(long))
                return "int";
            if (type == typeof(byte))
                return "byte";
            if (type == typeof(DateTime))
                return "dateTime";
            if (type == typeof(double) ||
                type == typeof(decimal) ||
                type == typeof(float))
                return "double";
            if (type == typeof(string))
                return "string";
            return type.Name;
        }

        private static XElement CreateElement(
            XNamespace xmlNamespace,
            string elementName,
            object content = null)
        {
            return new XElement(xmlNamespace == null
                    ? elementName
                    : xmlNamespace + elementName,
                content is DateTime ? $"{content:o}" : content);
        }

        private static XElement GetArrayElement(
            PropertyInfo info,
            Array input,
            XNamespace xmlNamespace = null)
        {
            var name = XmlConvert.EncodeName(info.Name) ?? "Object";
            XElement rootElement = CreateElement(xmlNamespace, name);
            var arrayCount = input?.GetLength(0) ?? 0;
            for (int i = 0; i < arrayCount; i += 1)
            {
               
                if (input != null)
                {
                    var val = input?.GetValue(i);
                    var typeName = GetTypeName(val.GetType());
                    XElement childElement = val.GetType().IsSimpleType()
                        ? CreateElement(xmlNamespace, $"{typeName}", val)
                        : val.ToXml(typeName, xmlNamespace);
                    rootElement.Add(childElement);
                }
            }

            return rootElement;
        }

        private static XElement ToXml(
            this object input,
            string element,
            XNamespace xmlNamespace = null)
        {
            if (input == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(element))
            {
                string name = input.GetType().Name;
                element = name.Contains("AnonymousType")
                    ? "Object"
                    : name;
            }

            element = XmlConvert.EncodeName(element);
            var ret = CreateElement(xmlNamespace, element);

            var type = input.GetType();
            var props = type.GetProperties();

            var elements = props.Select(p =>
            {
                var pType =
                    Nullable.GetUnderlyingType(p.PropertyType) ??
                    p.PropertyType;
                var name = XmlConvert.EncodeName(p.Name);
                var val =
                    pType.IsArray ? "array" : p.GetValue(input, null);
                var value = pType.IsArray
                    ? GetArrayElement(p,
                        (Array)p.GetValue(input, null),
                        xmlNamespace)
                    : pType.IsSimpleType() || pType.IsEnum
                        ? CreateElement(xmlNamespace, name, val)
                        : val.ToXml(name, xmlNamespace);
                return value;
            })
                .Where(v => v != null);

            ret.Add(elements);

            return ret;
        }



        public static XElement ToXml(this object input, XNamespace xmlNamespace = null)
        {
            return input.ToXml(null, xmlNamespace);
        }

        public static T ToObject<T>(
            this XElement input,
            string elementName = null,
            string xmlNamespace = null)
        {
            T result;

            XElement xml;

            if (string.IsNullOrEmpty(elementName))
            {
                xml = input;
                elementName = input.Name.LocalName;
            }
            else
            {
                xml = input
                    .Descendants(string.IsNullOrEmpty(xmlNamespace)
                        ? elementName
                        : (XNamespace)xmlNamespace + elementName)
                    .FirstOrDefault();
            }

            if (xml == null)
            {
                result = default(T);
            }
            else
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T),
                    new XmlRootAttribute
                    {
                        ElementName = elementName,
                        Namespace = xmlNamespace
                    });

                result = (T)serializer.Deserialize(xml.CreateReader());
            }

            return result;
        }
    }
}
