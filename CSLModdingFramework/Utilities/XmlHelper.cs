using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CSLModdingFramework.Utilities;

public static class XmlHelper {
    public static T Deserialize<T>(string xml, string rootElementName = null) {
        if (string.IsNullOrEmpty(xml))
            throw new ArgumentNullException(nameof(xml), "Xml string cannot be null or empty when deserialize object");
        
        var xmlSerializer = string.IsNullOrEmpty(rootElementName) ? new XmlSerializer(typeof(T)) : new XmlSerializer(typeof(T), new XmlRootAttribute(rootElementName));
        using var stringReader = new StringReader(xml);
        return (T)xmlSerializer.Deserialize(stringReader);
    }

    public static string Serialize<T>(T obj, bool format = false, string rootElementName = null, XmlSerializerNamespaces namespaces = null) {
        if (obj is null)
            throw new ArgumentNullException(nameof(obj), "Target object cannot be null or empty when serialize object");
        
        var xmlSerializer = string.IsNullOrEmpty(rootElementName) ? new XmlSerializer(typeof(T)) : new XmlSerializer(typeof(T), new XmlRootAttribute(rootElementName));
        var settings = new XmlWriterSettings {
            Indent = format,
            OmitXmlDeclaration = false,
            Encoding = new UTF8Encoding(false)
        };
        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, settings);
        xmlSerializer.Serialize(xmlWriter, obj, namespaces);
        return stringWriter.ToString();
    }
}