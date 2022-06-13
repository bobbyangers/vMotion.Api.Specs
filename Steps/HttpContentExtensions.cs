using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Mime;
using System.Text;

namespace vMotion.Api.Specs.Steps;

internal static class HttpContentExtensions
{
    internal static HttpContent CreateJsonContent(object data)
    {
        return new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, MediaTypeNames.Application.Json);
    }
}