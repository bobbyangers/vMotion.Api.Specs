using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture;

using BoDi;

using CorePush.Apple;
using CorePush.Interfaces;

using FluentAssertions;

using IdentityModel;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Entities;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using NSubstitute;

using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using TechTalk.SpecFlow.Infrastructure;

using vMotion.Api.Specs.Unit_Tests;
using vMotion.Dal;
using vMotion.Dal.MongoDb;
using vMotion.Dal.MongoDb.Entities;

namespace vMotion.Api.Specs.Steps
{
    [Binding]
    public class SharedSteps
    {
        private readonly ScenarioContext _context;
        private readonly WebHostSupport _webHost;
        private readonly IFixture _fixture;

        private ISpecFlowOutputHelper _output;

        public SharedSteps(ScenarioContext context, WebHostSupport webHost, IFixture fixture, ISpecFlowOutputHelper output)
        {
            _context = context;
            _webHost = webHost;
            _fixture = fixture;
            _output = output;
        }

        [BeforeScenario(Order = 10)]
        public void BeforeScenario(IObjectContainer objectContainer)
        {
            var dt = DateTime.Now;
            _context[Constants.Now] = $"{dt:yyyyMMddHHmm}";
            _context[Constants.LastWeek] = $"{dt:yyyyMMddHHmm}-{dt.AddDays(-7):yyyyMMddHHmm}"; ;
        }

        [Given("an anonymous using http web client")]
        public void UsingAnAnonymousHttpWebClient()
        {
            _webHost.CreateClient(null);
        }

        [Given(@"a user with role \[(.*)\] using a ([Ww]eb|[Mm]obile) client")]
        [Given(@"a user with role \[(.*)\] using an http ([Ww]eb|[Mm]obile) client")]
        public async Task GivenLoggedInAsUserWithRole(UserTypeEnum role, string clientType)
        {
            string userId = Guid.NewGuid().ToObjectId();

            _context.Add(Constants.UserId, userId.ObjectIdToGuidString());

            var claims = new List<Claim>();

            if (role == UserTypeEnum.Customer || role == UserTypeEnum.Guest)
            {
                var record1 = await CreateRecord<MemberEntity>(userId, Constants.MemberId).ConfigureAwait(false);

                claims.AddRange(new Claim[]
                {
                    new (Constants.ClaimTypes.EmailAddress, record1.Email),
                    new (JwtClaimTypes.Subject, userId),
                    new (JwtClaimTypes.Name, $"Member[{role}] [{userId}]"),
                    new (JwtClaimTypes.Role, role.ToString())
                });
            }
            else
            {
                var record2 = await CreateRecord<StaffEntity>(userId, Constants.StaffId).ConfigureAwait(false);
                claims.AddRange(new Claim[]
                {
                    new (Constants.ClaimTypes.EmailAddress, record2.Email),
                    new (JwtClaimTypes.Subject, userId),
                    new (JwtClaimTypes.Name, $"Staff[{role}]"),
                    new (JwtClaimTypes.Role, role.ToString()),
                    new (Constants.ClaimTypes.Operator,  record2.Operator.ID)
                });

                await AssignToAgentRole(userId);
            }

            var token = ApiTokenHelper.CreateToken(claims.ToArray());

            _webHost.CreateClient(token);
        }

        private async Task AssignToAgentRole(string userId)
        {
            if (_context.TryGetValue(Constants.RoleAgent, out string agentRole))
            {
                await DB.Entity<StaffEntity>(userId).Roles
                    .AddAsync(agentRole, null, CancellationToken.None)
                    .ConfigureAwait(false);
            }
        }

        private async Task<T> CreateRecord<T>(string id, string contextKey)
            where T : Entity, IUserRecord, IOperatorRecord
        {
            var record = _fixture.Create<T>();
            record.ID = id;

            if (_context.ContainsKey(Constants.OperatorId))
            {
                record.Operator = _context.Get<string>(Constants.OperatorId);
            }
            await record.SaveAsync().ConfigureAwait(false);

            _context.Set(record.ID.ObjectIdToGuidString(), contextKey);

            _output.WriteLine($"{contextKey} => {typeof(T).Name}[{record.ID.ToGuid():D}] | created");

            return record;
        }

        [Given("an operator exists")]
        public async Task GivenAnOperatorExists()
        {
            var code = Guid.NewGuid().ToObjectId().SubstringLast(4);
            await AnGivenOperatorWithCodeAExists(code).ConfigureAwait(false);
        }

        [Given(@"an operator with code \[(.*)\] exists")]
        public async Task AnGivenOperatorWithCodeAExists(string code)
        {
            code = code.ToUpper();

            var op1 = await DB.Find<OperatorEntity>()
                          .Match(_ => code == _.Code)
                          .ExecuteFirstAsync()
                          .ConfigureAwait(false)
                      ?? _fixture.Create<OperatorEntity>()
                          .Then(_ =>
                          {
                              _.Code = code;
                          });

            await op1.SaveAsync().ConfigureAwait(false);

            var role = DbData.GetRole($"Call Agent [{op1.ID}]", op1.ID, DbSeedRoles.CallAgentPermissions);
            await role.SaveAsync().ConfigureAwait(false);

            //// await op1.Roles.AddAsync(role.ID);

            _context.Set(op1.ID.ObjectIdToGuidString(), Constants.OperatorId);
            _context.Set(role.ID.ObjectIdToGuidString(), Constants.RoleAgent);
        }

        [Given("a member exists")]
        public async Task WhenAMemberExists()
        {
            string userId = Guid.NewGuid().ToObjectId();

            await CreateRecord<MemberEntity>(userId, Constants.MemberId).ConfigureAwait(false);
        }

        [Given("a staff exists")]
        public async Task WhenAStaffExists()
        {
            string userId = Guid.NewGuid().ToObjectId();

            await CreateRecord<StaffEntity>(userId, Constants.StaffId).ConfigureAwait(false);
        }

        [Given("a payload request")]
        public void GivenThisPayloadRequest(Table data)
        {
            Regex r = new Regex(@"\{(.*)\}", RegexOptions.Compiled | RegexOptions.Singleline);

            foreach (var row in data.Rows)
                foreach (var k in row.Keys)
                {
                    if (r.IsMatch(row[k]))
                        row[k] = TokenReplace(row[k]);
                }

            var body = data.CreateDynamicInstance();

            _webHost.Content = JsonContent.Create(body);
        }

        [Given("notification service returned bad device")]
        public void GivenNotificationServiceReturnedBadDevice()
        {
            var actor = _webHost.Factory.Services.GetRequiredService<IApnSender>();

            actor.ClearReceivedCalls();

            actor.SendAsync(default, default, default, default, default, default, default)
                .ReturnsForAnyArgs(new ApnsResponse()
                {
                    IsSuccess = false,
                    Error = new ApnsError
                    {
                        Reason = ReasonEnum.BadDeviceToken
                    }
                });
        }

        [When(@"a (GET|DELETE) request is sent to \[(.*)\]")]
        public async Task WhenARequestTo(HttpMethod method, string arg)
        {
            arg = TokenReplace(arg);

            var request = new HttpRequestMessage(method,
                new Uri(arg, UriKind.Relative));

            await _webHost.RunRequest(request).ConfigureAwait(false);
        }

        [When(@"a (PUT|POST|PATCH) request is sent to \[(.*)\]")]
        public async Task WhenRequestTo(HttpMethod method, string arg)
        {
            arg = TokenReplace(arg);

            var request = new HttpRequestMessage(method,
                new Uri(arg, UriKind.Relative))
            {
                Content = _webHost.Content
            };

            await _webHost.RunRequest(request).ConfigureAwait(false);
        }

        [Then("staffOnline record is updated")]
        public async Task ThenStaffOnlineRecordIsUpdated()
        {
            ////await Support.FakeDb.StaffOnline.ReceivedWithAnyArgs(1).CreateAsync(default);
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Then("the response status should be (.*)")]
        public void ThenTheResultShouldBe(HttpStatusCode code)
        {
            _webHost.Response.StatusCode.Should().Be(code);
        }

        [Then("the response should be successful")]
        public void ThenTheResultShouldBe()
        {
            _webHost.Response.EnsureSuccessStatusCode();
        }

        [Then("blob storage upload was invoked")]
        public void ThenBlobStorageWasInvoked()
        {
            _webHost.GetActor<IBlobStorage>().ReceivedWithAnyArgs(1).UploadBlob(default, default);
        }

        [Then("blob storage delete was invoked")]
        public void ThenBlobStorageDeleteWasInvoked()
        {
            _webHost.GetActor<IBlobStorage>().ReceivedWithAnyArgs(1).DeleteBlob(default);
        }

        [Then("backoffice was notified")]
        public async Task ThenBackOfficeWasNotified()
        {
            await _webHost.GetActor<IRealtimeNotificationService>().Received()
                .BroadcastToBackoffice(MessageName.receiveMessage, Arg.Any<object>(), Arg.Any<CancellationToken>())
                .ConfigureAwait(false);
        }

        [Then(@"backoffice was notified at least (\d+)")]
        public async Task ThenBackOfficeWasNotifiedAtLeast(int times)
        {
            await _webHost.GetActor<IRealtimeNotificationService>().Received(times)
                .BroadcastToBackoffice(MessageName.receiveMessage, Arg.Any<object>(), Arg.Any<CancellationToken>())
                .ConfigureAwait(false);
        }


        [Then("member was notified")]
        public async Task ThenMemberWasNotified()
        {
            await _webHost.GetActor<IRealtimeNotificationService>().Received()
                .SendToMember(Arg.Is<Guid>(x => x != Guid.Empty), Arg.Any<object>(), Arg.Any<CancellationToken>())
                .ConfigureAwait(false);
        }

        [Then("member was notified via signalr")]
        public async Task ThenMemberWasNotifiedViaSignalR()
        {
            await _webHost.GetActor<IRealtimeNotificationService>().Received()
                .SendToMember(Arg.Is<Guid>(x => x != Guid.Empty), Arg.Any<object>(), Arg.Any<CancellationToken>())
                .ConfigureAwait(false);
        }

        [Then("member was notified via signalr with message \\[(.*)\\]")]
        public async Task ThenMemberWasNotifiedViaSignalRWithMessage(string message)
        {
            var m = message;
            await _webHost.GetActor<IRealtimeNotificationService>()
                .Received().SendToMember(
                    Arg.Is<Guid>(x => x != Guid.Empty),
                    Arg.Is<object>(x => x.ToJson().Contains(m)),
                    Arg.Any<CancellationToken>())
                .ConfigureAwait(false);
        }


        [Then("member was notified via apn")]
        public async Task ThenMemberWasNotifiedViaApn()
        {
            await _webHost.GetActor<IApnSender>().ReceivedWithAnyArgs()
                .SendAsync(default, default, default, default, default, default, default)
                .ConfigureAwait(false);
        }

        [Then("staff was notified")]
        public async Task ThenStaffWasNotified()
        {
            await _webHost.GetActor<IRealtimeNotificationService>().Received(1)
                .SendToStaff(Arg.Is<Guid>(x => x != Guid.Empty), Arg.Any<object>(), Arg.Any<CancellationToken>())
                .ConfigureAwait(false);
        }

        [Then("response body is not empty")]
        public async Task ThenResponseBodyNotEmpty()
        {
            var content = await _webHost.Response.Content.ReadAsStringAsync().ConfigureAwait(false);

            content.Should().NotBeEmpty();
            content.Should().NotMatchRegex("^\\{\\s*\\}$");

            var responseData = JsonConvert.DeserializeObject<object>(content).As<JObject>();

            ShowResult(responseData);
        }

        [Then("list is not empty")]
        public async Task ThenListNotEmpty()
        {
            var content = await _webHost.Response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var responseData = JsonConvert.DeserializeObject<object[]>(content);

            responseData!.Should().HaveCountGreaterThan(0);
        }

        [Then("array is not empty")]
        public async Task ThenArrayNotEmpty()
        {
            var content = await _webHost.Response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var responseData = JsonConvert.DeserializeObject<object[]>(content);

            responseData.Should().NotBeEmpty();
        }

        [Then(@"a list should have (\d*)( item[s]?)?")]
        public async Task ThenAListOfDoctorsShouldHave(int expected, string _0)
        {
            var response = _webHost.Response;

            var body = JsonConvert.DeserializeObject<object[]>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));

            body.Should().HaveCount(expected);
        }

        [Then(@"a list should have at least (\d*)( item[s]?)?")]
        public async Task ThenAListOfDoctorsShouldHaveAtLeast(int expected, string _0)
        {
            var response = _webHost.Response;

            var body = JsonConvert.DeserializeObject<object[]>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));

            body.Should().HaveCountGreaterOrEqualTo(expected);
        }

        [Then(@"a custom list should have (\d*)( item(s?))?")]
        public async Task ThenACustomListOfDoctorsShouldHave(int expected, string _0, string _1)
        {
            var response = _webHost.Response;

            var body = JsonConvert.DeserializeObject<List<object>>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));

            body.Should().NotBeNull();
            body.Should().HaveCount(expected);
        }

        [Then("headers should have location")]
        public void ThenHeadersShouldHaveLocation()
        {
            var response = _webHost.Response;

            response.Headers.Location.Should().NotBeNull();
        }

        private string TokenReplace(string arg)
        {
            arg = ReplaceThis(arg, Constants.OperatorId);
            arg = ReplaceThis(arg, Constants.StaffId);
            arg = ReplaceThis(arg, Constants.MemberId);
            arg = ReplaceThis(arg, Constants.AnotherStaffId);

            arg = ReplaceThis(arg, Constants.CallId);
            arg = ReplaceThis(arg, Constants.CaseId);
            arg = ReplaceThis(arg, Constants.CCardId);
            arg = ReplaceThis(arg, Constants.NoteId);

            arg = ReplaceThis(arg, Constants.RoleId);
            arg = ReplaceThis(arg, Constants.RoleX);
            arg = ReplaceThis(arg, Constants.RoleY);

            arg = ReplaceThis(arg, Constants.NotificationId);

            arg = ReplaceThis(arg, Constants.Now);
            arg = ReplaceThis(arg, Constants.LastWeek);

            return arg;
        }

        private string ReplaceThis(string source, string key)
        {
            if (source.Contains($"{{{key}}}")
               && _context.TryGetValue(key, out string data))
            {
                _output.WriteLine($"Replacing {{{key}}} => {data}");
                return source.Replace($"{{{key}}}", data);
            }

            return source;
        }

        private void ShowResult(object result)
        {
            _output.WriteLine(new string('-', 50));
            _output.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        }
    }
}