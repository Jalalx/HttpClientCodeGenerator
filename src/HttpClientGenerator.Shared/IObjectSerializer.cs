namespace HttpClientGenerator.Shared
{
    public interface IObjectSerializer
    {
        string Serialize(object obj);
        T Deserialize<T>(string content);
    }
}

