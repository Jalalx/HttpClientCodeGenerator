using System;

namespace HttpClientGenerator.Shared
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class HttpRequestHeaderAttribute : Attribute
    {
        public HttpRequestHeaderAttribute(string name, string values)
        {
            Name = name;
            Values = values;
        }

        public string Name { get; }
        public string Values { get; }
    }
}
