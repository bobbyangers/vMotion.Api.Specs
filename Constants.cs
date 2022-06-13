using TechTalk.SpecFlow.Tracing;

namespace vMotion.Api.Specs
{
    internal static class Constants
    {
        public const string Category = "Category";
        public const string CI = "CI";
        public const string Integration = "Integration";

        public const string ToFix = "Fix";

        public static class MediaTypeNames
        {
            public static class Application
            {
                public const string JsonProblem = "application/problem+json";

                public const string Form = "application/x-www-form-urlencoded";
            }

            public static class Images
            {
                public const string Png = "images/png";
            }
        }

        public const string TestEnv = "Test";

        public static readonly string OperatorId = nameof(OperatorId).ToIdentifierCamelCase();
        public static readonly string StaffId = nameof(StaffId).ToIdentifierCamelCase();
        public static readonly string AnotherStaffId = nameof(AnotherStaffId).ToIdentifierCamelCase();

        public static readonly string MemberId = nameof(MemberId).ToIdentifierCamelCase();

        public static readonly string UserId = nameof(UserId).ToIdentifierCamelCase();

        public static readonly string CaseId = nameof(CaseId).ToIdentifierCamelCase();
        public static readonly string CallId = nameof(CallId).ToIdentifierCamelCase();
        public static readonly string CCardId = nameof(CCardId).ToIdentifierCamelCase();

        public static readonly string NoteId = nameof(NoteId).ToIdentifierCamelCase();

        public static readonly string RoleAgent = nameof(RoleAgent).ToIdentifierCamelCase();
        public static readonly string RoleY = nameof(RoleY).ToIdentifierCamelCase();
        public static readonly string RoleX = nameof(RoleX).ToIdentifierCamelCase();
        public static readonly string RoleId = nameof(RoleId).ToIdentifierCamelCase();

        public static string NotificationId = nameof(NotificationId).ToIdentifierCamelCase();

        public static string Now = nameof(Now).ToIdentifierCamelCase();
        public static string LastWeek = nameof(LastWeek).ToIdentifierCamelCase();

        public static class ClaimTypes
        {
            public const string EmailAddress = "emailaddress";

            public static readonly string Operator = nameof(Operator).ToIdentifierCamelCase();
        }
    }
}