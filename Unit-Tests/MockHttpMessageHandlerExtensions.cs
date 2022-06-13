using RichardSzalay.MockHttp;
using System;
using System.Net.Http;
using vMotion.Unit.Tests;
using Xunit.Abstractions;

namespace vMotion.Api.Specs
{
    internal static class MockHttpMessageHandlerExtensions
    {
        public static HttpClient ToHttpClient(this MockHttpMessageHandler subject, DelegatingHandler outerHandler)
        {
            outerHandler.InnerHandler = subject;
            return new HttpClient(outerHandler);
        }

        public static HttpClient ToHttpClient(this MockHttpMessageHandler subject, ITestOutputHelper output)
        {
            return ToHttpClient(subject, new TraceLogRequestHandler(output));
        }

        public static HttpClient ToHttpClient(this MockHttpMessageHandler subject, ITestOutputHelper output, string baseAddress)
        {
            var result = ToHttpClient(subject, new TraceLogRequestHandler(output));

            result.BaseAddress = new Uri(baseAddress);

            return result;
        }
    }
}