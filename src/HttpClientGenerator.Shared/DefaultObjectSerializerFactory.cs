using System;

namespace HttpClientGenerator.Shared
{
    public class DefaultObjectSerializerFactory : IObjectSerializerFactory
    {
        public IObjectSerializer GetSerializer(string contentType)
        {
            switch (contentType?.Trim()?.ToLower())
            {
                case "application/json":
                    return new DefaultJsonSerializer();
                
                case "application/xml":
                    return new DefaultXmlSerializer();

                default:
                    throw new NotSupportedException($"content-type '{contentType}' is not supported.");
            }
        }
    }
}

