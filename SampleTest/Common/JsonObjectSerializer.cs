using Microsoft.Toolkit.Uwp.Helpers;
using System.Text.Json;

namespace SampleTest.Samples.Common
{
    public class JsonObjectSerializer : IObjectSerializer
    {
        public string Serialize<T>(T value) => JsonSerializer.Serialize(value);

        public T Deserialize<T>(string value) => JsonSerializer.Deserialize<T>(value);
    }
}
