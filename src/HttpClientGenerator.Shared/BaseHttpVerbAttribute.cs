using System;

namespace HttpClientGenerator.Shared
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public abstract class BaseHttpVerbAttribute : Attribute
    {
        public BaseHttpVerbAttribute(HttpVerbs verb, string path)
        {
            HttpVerb = verb;
            Path = path;
        }

        public string ContentType { get; set; } = "application/json";
        public HttpVerbs HttpVerb { get; set; }
        public string Path { get; }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class HttpGetAttribute : BaseHttpVerbAttribute
    {
        public HttpGetAttribute(string path) : base(HttpVerbs.Get, path)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class HttpPostAttribute : BaseHttpVerbAttribute
    {
        public HttpPostAttribute(string path) : base(HttpVerbs.Post, path)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class HttpPutAttribute : BaseHttpVerbAttribute
    {
        public HttpPutAttribute(string path) : base(HttpVerbs.Put, path)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class HttpOptionAttribute : BaseHttpVerbAttribute
    {
        public HttpOptionAttribute(string path) : base(HttpVerbs.Option, path)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class HttpDeleteAttribute : BaseHttpVerbAttribute
    {
        public HttpDeleteAttribute(string path) : base(HttpVerbs.Delete, path)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class HttpPatchAttribute : BaseHttpVerbAttribute
    {
        public HttpPatchAttribute(string path) : base(HttpVerbs.Patch, path)
        {
        }
    }
}

