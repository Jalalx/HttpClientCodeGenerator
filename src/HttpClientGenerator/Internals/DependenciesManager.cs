using System.Collections.Generic;
using System.IO;

namespace HttpClientGenerator.Internals
{
    public static class DependenciesManager
    {
        private static object _lock = new object();
        private static Dictionary<string, string> _dict = new Dictionary<string, string>();

        public static Dictionary<string, string> GetDependenciesSourceCode()
        {
            if (_dict.Count == 0)
            {
                lock (_lock)
                {
                    if (_dict.Count == 0)
                    {
                        var sharedAssembly = typeof(HttpClientCodeGenerator).Assembly;

                        foreach (var resourceName in sharedAssembly.GetManifestResourceNames())
                        {
                            if (!resourceName.EndsWith(".cs"))
                            {
                                // Not a C# file, discarded.
                            }

                            using (var stream = sharedAssembly.GetManifestResourceStream(resourceName))
                            using (var reader = new StreamReader(stream, true))
                            {
                                _dict.Add(resourceName, reader.ReadToEnd());
                            }
                        }
                    }
                }
            }

            return _dict;
        }
    }
}
