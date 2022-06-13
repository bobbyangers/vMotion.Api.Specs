using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using AutoFixture;

using MongoDB.Entities;

using TechTalk.SpecFlow;

using vMotion.Dal;
using vMotion.Dal.MongoDb.Entities;

namespace vMotion.Api.Specs.Steps
{
    [Binding]
    [Scope(Feature = "Call Agent Members")]
    public class CallAgentMemberSteps : TechTalk.SpecFlow.Steps
    {
        private readonly ScenarioContext _context;
        private readonly WebHostSupport _webHost;
        private IFixture _fixture;

        public CallAgentMemberSteps(ScenarioContext context, WebHostSupport webHost)
        {
            _context = context;
            _webHost = webHost;
        }

        #region Overrides of AutoFixtureTests
        [BeforeScenario(Order = 100)]
        public void BeforeScenario(IFixture fixture)
        {
            _fixture = fixture;

            fixture.Customize<api._Features_.Agents.Members.Put.Request>(x => x
                .Without(_ => _.Id)
            );

            fixture.Customize<MemberCreditCardEntity>(x => x
                .With(_ => _.ID, (string)null)
                .With(_ => _.Last4, "9960")
                .With(_ => _.Brand, "VI")
                .With(_ => _.Country, "CA")
                .With(_ => _.ValidationFields, "{}")
                .With(_ => _.IsDefault, false)
                .With(_ => _.IsDeleted, false)
                .Without(_ => _.Member)
                .Without(_ => _.CreatedOn)
            );

            fixture.Customize<StripeTransactionEntity>(x => x
                .With(_ => _.Amount, 10.0M)
                .With(_ => _.Currency, "cad")
                .With(_ => _.StripeId)
                .With(_ => _.Status, PaymentStatus.Pending)
                .WithAutoProperties()
            );

            _context.Set(DB.Entity<CallEntity>().GenerateNewID().ObjectIdToGuidString(), Constants.CallId);
        }
        #endregion

        [Given("a member update request")]
        public void GivenAMemberUpdateRequest()
        {
            var data = _fixture.Build<api._Features_.Agents.Members.Put.Request>()
                .Create();

            _webHost.Content = JsonContent.Create(data);
        }

        [Given(@"a signalr payload")]
        public void GivenASignalRPayload()
        {
            var data = new api._Features_.Agents.Members.Post.Events.Request
            {
                Data = new { data = "value" },
                Type = "Refresh"
            };
            _webHost.Content = JsonContent.Create(data);
        }

        [Given(@"a credit card exists")]
        public async Task GivenACreditCardExists()
        {
            var member = await _context.GetRecord<MemberEntity>(Constants.MemberId).ConfigureAwait(false);
            var cc = _fixture.Create<MemberCreditCardEntity>();

            cc.Member = member.ID;

            await DB.SaveAsync(cc);

            await member.CreditCards.AddAsync(cc.ID);

            _context.Set(cc.ID.ObjectIdToGuidString(), Constants.CCardId);
        }

        [Given(@"payments data exists")]
        public async Task GivenPaymentsDataExists()
        {
            var member = await _context.GetRecord<MemberEntity>(Constants.MemberId);
            var call = ((string)_context[Constants.CallId]).ToObjectId();

            var payment = _fixture.Create<StripeTransactionEntity>();
            payment.Member = member.ID;
            payment.Call = call;

            await DB.SaveAsync(payment);

            await member.Payments.AddAsync(payment.ID);
        }


        [Given("a picture with ext (JPG|PNG) needs to be uploaded")]
        public void GivenAPictureWithExtensionNeedsToBeUploaded(string ext)
        {
            var image = ext == "PNG"
                ? new MemoryStream(Properties.Resources.star)
                : new MemoryStream(Properties.Resources.file_example_JPG_100kB);

            var content = new StreamContent(image);
            var request = new MultipartFormDataContent
            {
                { content, "memberfile", $"star.{ext.ToLower()}" }
            };

            _webHost.Content = request;
        }

        [Given(@"an update credit card validation payload")]
        public void GivenAnUpdateCreditCardValidationPayload()
        {

            var request = JsonContent.Create(new { validationFields = new { some = "data" } });
            _webHost.Content = request;
        }

    }
}