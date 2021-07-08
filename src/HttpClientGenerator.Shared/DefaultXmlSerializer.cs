using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace HttpClientGenerator.Shared
{
    public class DefaultXmlSerializer : IObjectSerializer
    {
        public string Serialize(object obj)
        {
            if (obj == null) return string.Empty;
            var ser = new XmlSerializer(obj.GetType());

            using (var stream = new MemoryStream())
            {
                ser.Serialize(stream, obj);
                var content = stream.ToArray();
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        public T Deserialize<T>(string content)
        {
            var ser = new XmlSerializer(typeof(T));
            using (var stream = new StringReader(content))
            {
                return (T)ser.Deserialize(stream);
            }
        }
    }
}

