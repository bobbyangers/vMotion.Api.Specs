using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using Xunit.Abstractions;

namespace vMotion.Api.Specs
{
    [Binding]
    public class TraceLogRequestHandler : DelegatingHandler
    {
        public ITestOutputHelper Trace { get; }

        public TraceLogRequestHandler(ITestOutputHelper output)
        {
            Trace = output;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Trace.WriteLine($"{request.Method} --> {request.RequestUri?.PathAndQuery ?? "Unknown Uri"}");
            if (request.Headers.Any())
                Trace.WriteLine($"  Headers --> {request.Headers}");

            TraceWriteContent(request.Content);

            return base.SendAsync(request, cancellationToken).ContinueWith(ctx =>
            {
                var result = ctx.Result;

                Trace.WriteLine($"   ---> Response Status = {result.StatusCode} ({(int)ctx.Result.StatusCode})");

                if (result.StatusCode == HttpStatusCode.Created)
                {
                    Trace.WriteLine($"   ---> Location => {result.Headers.Location}");
                }

                TraceWriteContent(result.Content);

                return ctx.Result;
            }, cancellationToken);
        }

        protected void TraceWriteContent(HttpContent content)
        {
            if (content == null) return;

            var (contentType, contentText) = ReadBody(content);

            Trace.WriteLine($"    --> ContentType: {contentType}");

            if (contentType.Equals(System.Net.Mime.MediaTypeNames.Application.Json)
                || contentType.Equals(Constants.MediaTypeNames.Application.JsonProblem))
            {
                contentText = contentText.FormattedJson();
            }
            else if (contentType.Equals(System.Net.Mime.MediaTypeNames.Text.Plain))
            {
                contentText = contentText.FormattedJson();
            }
            ////else if (contentType.Equals(Constants.MediaTypeNames.Application.Form)) { }
            else
            {
                Trace.WriteLine("       --> [content type is not traced]");
                Trace.WriteLine(new string('-', 50));
                return;
            }

            Trace.WriteLine(new string('-', 50));
            Trace.WriteLine(contentText);
        }

        /// <summary>
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        protected virtual (string, string) ReadBody(HttpContent content)
        {
            var body = string.Empty;
            var contentType = content.Headers.ContentType?.MediaType ?? "<NOT SET>";
            if (new[]
            {
                System.Net.Mime.MediaTypeNames.Application.Json,
                System.Net.Mime.MediaTypeNames.Text.Plain,
                System.Net.Mime.MediaTypeNames.Text.Html,
                Constants.MediaTypeNames.Application.JsonProblem,
                Constants.MediaTypeNames.Application.Form
            }.Any(x => x.Equals(contentType)))
            {
                body = content.ReadAsStringAsync().Result;

                if (string.IsNullOrWhiteSpace(body)) Trace.WriteLine("   ---> No Content");
            }

            return (contentType, body);
        }
    }
}