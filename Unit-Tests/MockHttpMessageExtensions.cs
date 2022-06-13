using RichardSzalay.MockHttp;
using RichardSzalay.MockHttp.Matchers;
using System.Net.Http;
using System.Text.RegularExpressions;
using vMotion.Unit.Tests;

namespace vMotion.Api.Specs
{
    public static class MockHttpMessageExtensions
    {
        public static MockedRequest Expect(
            this MockHttpMessageHandler handler,
            HttpMethod method,
            Regex pattern)
        {
            MockedRequest mockedRequest = new MockedRequest();
            mockedRequest.With(new MethodMatcher(method));
            mockedRequest.With(new RegexUrlMatcher(pattern));

            handler.AddRequestExpectation((IMockedRequest)mockedRequest);
            return mockedRequest;
        }

        public static MockedRequest Expect(this MockHttpMessageHandler handler, Regex pattern)
        {
            var mockedRequest = new MockedRequest();

            mockedRequest.With(new RegexUrlMatcher(pattern));

            return mockedRequest;
        }
    }
}

namespace vMotion.Unit.Tests
{
    public class RegexUrlMatcher : IMockedRequestMatcher
    {
        public Regex UrlPattern { get; set; }
        public RegexUrlMatcher(Regex pattern)
        {
            UrlPattern = pattern;
        }

        public bool Matches(HttpRequestMessage message)
        {
            return (message.RequestUri != null) && UrlPattern.IsMatch(message.RequestUri.ToString());
        }
    }
}