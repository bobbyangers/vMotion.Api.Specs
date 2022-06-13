using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using vMotion.Dal;

namespace vMotion.Api.Specs
{
    [Binding]
    public class CustomConversions
    {
        [StepArgumentTransformation("(.*)")]
        public HttpMethod HttpMethodTransformation(string input)
        {
            switch (input)
            {
                case "GET":
                    return HttpMethod.Get;
                case "PATCH":
                    return HttpMethod.Patch;
                case "PUT":
                    return HttpMethod.Put;
                case "POST":
                    return HttpMethod.Post;
                case "DELETE":
                    return HttpMethod.Delete;
            }

            return HttpMethod.Get;
        }

        [StepArgumentTransformation("in (\\d) days")]
        public DateTime InSomeDaysTransformation(int days)
        {
            return DateTime.Today.AddDays(days);
        }

        [StepArgumentTransformation]
        public IEnumerable<CallTableData> CallTableDataTransformation(Table data)
        {
            return data.CreateSet<CallTableData>().ToList();
        }

        [StepArgumentTransformation]
        public IEnumerable<NotificationTableData> NotificationTableDataTransformation(Table data)
        {
            return data.CreateSet<NotificationTableData>().ToList();
        }

        [StepArgumentTransformation]
        public HttpContent GetContent(Table data)
        {
            var body = data.CreateDynamicInstance();

            return JsonContent.Create(body);
        }
    }

    public class CallTableData
    {
        public string Summary { get; set; }
        public string OngoingCallBy { get; set; }
        public string InVideoCallStatus { get; set; }
        public string AssignedToUserType { get; set; }
        public TimeSpan ScheduledIn { get; set; }
        public string AssignedTo { get; set; }
    }

    public class NotificationTableData
    {
        public NotificationEnum NotificationType { get; set; }
        public string Description { get; set; }
        public bool IsRead { get; set; }
        public bool IsArchived { get; set; }
    }
}
