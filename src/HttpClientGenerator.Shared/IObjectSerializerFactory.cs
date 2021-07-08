namespace HttpClientGenerator.Shared
{
    public interface IObjectSerializerFactory
    {
        IObjectSerializer GetSerializer(string contentType);
    }
}

