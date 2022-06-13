using AutoFixture;
using FluentAssertions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Entities;
using Newtonsoft.Json;
using NSubstitute;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using vMotion.Dal;
using vMotion.Dal.MongoDb.Entities;

namespace vMotion.Api.Specs.Steps
{
    [Binding]
    [Scope(Feature = "Staff Manager")]
    public class StaffManagerSteps : TechTalk.SpecFlow.Steps
    {
        private readonly ScenarioContext _context;
        private readonly WebHostSupport _webHost;
        private IFixture _fixture;

        public StaffManagerSteps(ScenarioContext context, WebHostSupport webSupport)
        {
            _context = context;
            _webHost = webSupport;
        }

        #region Overrides of AutoFixtureTests
        [BeforeScenario(Order = 100)]
        public void BeforeScenario(IFixture fixture)
        {
            _fixture = fixture;

            var yrb = DateTime.Today.Year - 21;

            _fixture.Customize<api._Features_.Agents.Staff.Post.Request>(_ => _
                .With(x => x.UserType, UserTypeEnum.Agent)
            );

            _fixture.Customize<api._Features_.Agents.Staff.Put.Request>(_ => _
                .With(x => x.UserType, UserTypeEnum.Agent)
            );

            _fixture.Customize<StaffEntity>(_ => _
                .With(x => x.ID, (string)null)
                .With(x => x.UserType, UserTypeEnum.Agent)
                .Without(x => x.Roles)
                .Without(x => x.Operator)
                .Without(x => x.CurrentCall)
            );

            _fixture.Customize<RoleEntity>(_ => _
                .With(x => x.ID, (string)null)
                .Without(x => x.Permissions)
            );
        }
        #endregion

        [Given("picture needs to be uploaded")]
        public void GivenPictureNeedsToBeUploaded()
        {
            var image = new MemoryStream(Properties.Resources.star);
            var stream = new StreamContent(image);
            var body = new MultipartFormDataContent
            {
                { stream, "picturefile", "star.png" }
            };

            _webHost.Content = body;
        }

        [Given("role X exists")]
        public async Task GivenARoleXExists()
        {
            var role = _fixture.Create<RoleEntity>();
            await role.SaveAsync().ConfigureAwait(false);

            _context.Set(role.ID.ObjectIdToGuidString(), Constants.RoleX);
        }

        [Given(@"role \[(.*)\] exists")]
        public async Task GivenARoleSomethingExists(string roleId)
        {
            var role = _fixture.Create<RoleEntity>()
                .Then(_ => _.ID = roleId.ToObjectId());

            await role.SaveAsync().ConfigureAwait(false);

            _context.Set(role.ID.ObjectIdToGuidString(), Constants.RoleId);
        }

        [Given("another staff exists")]
        public async Task GivenAnotherStaffExists()
        {
            var staffId = DB.Entity<StaffEntity>().GenerateNewID();

            var staff = _fixture.Create<StaffEntity>().Then(_ => _.ID = staffId);

            await staff.SaveAsync().ConfigureAwait(false);

            _context.Set(staffId.ObjectIdToGuidString(), Constants.AnotherStaffId);
        }

        [Given(@"added role \[(.*)\] to other staff")]
        public async Task GivenAnotherStaffExistsWithRoleX(string roleId)
        {
            var staff = await _context.GetRecord<StaffEntity>(Constants.AnotherStaffId).ConfigureAwait(false);

            staff.Should().NotBeNull();

            await staff.Roles.AddAsync(roleId.ToObjectId()).ConfigureAwait(false);
        }

        [Given("an add to role request")]
        public void GivenAnAddToRoleRequest()
        {
            var staffId = _context.Get<string>(Constants.AnotherStaffId);

            _webHost.Content = JsonContent.Create(new
            {
                StaffId = staffId.ToGuid()
            });
        }

        [Given("a create staff request")]
        public void GivenACreateStaffRequest()
        {
            var data = _fixture.Create<api._Features_.Agents.Staff.Post.Request>();
            var parts = data.Email.Split("@");
            data.Email = parts[0] + "+test1" + "@" + parts[1];

            _webHost.Content = JsonContent.Create(data);
        }

        [Given("an update staff request")]
        public void GivenAnUpdateStaffRequest()
        {
            var data = _fixture.Create<api._Features_.Agents.Staff.Put.Request>();

            _webHost.Content = JsonContent.Create(data);
        }

        [Given("a CreateRoleCommand payload")]
        public void GivenACreateRoleCommandPayload()
        {
            var data = _fixture.Build<api._Features_.Agents.Roles.Post.Request>()
                .Without(_ => _.OperatorId)
                .Create();

            _webHost.Content = JsonContent.Create(data);
        }

        [Given("a UpdateRoleCommand payload")]
        public void GivenAUpdateRoleCommandPayload()
        {
            var data = _fixture.Build<api._Features_.Agents.Roles.Put.Request>()
                .Without(_ => _.Id)
                .Create();

            _webHost.Content = JsonContent.Create(data);
        }

        [Given("configure authentication service")]
        public void WhenConfigureAuthenticationService()
        {
            var staffId = DB.Entity<StaffEntity>().GenerateNewID();

            var service = _webHost.GetActor<IAuthenticationService>();

            service.CreateUser(Arg.Any<CreateUserRequest>(), Arg.Any<CancellationToken>())
                .Returns(staffId);

            var response = _fixture.Build<GetUserResponse>()
                .With(x => x.Id, staffId)
                .Create();

            service.GetUser(Arg.Any<GetUserRequest>(), Arg.Any<CancellationToken>())
                .Returns(response);

            _context.Set(staffId.ObjectIdToGuidString(), Constants.AnotherStaffId);
        }

        [Then("operator picture was updated")]
        public async Task ThenOperatorPictureWasUpdated()
        {
            var @operator = await _context.GetRecord<OperatorEntity>(Constants.OperatorId).ConfigureAwait(false);

            @operator.ImageUrl.Should().NotBeEmpty();
        }

        [Then(@"staff record roles were added to \[(.*)\]")]
        public async Task ThenOtherStaffRecordRolesWereAdded(string roleId)
        {
            var staff = await _context.GetRecord<StaffEntity>(Constants.AnotherStaffId).ConfigureAwait(false);
            staff.Should().NotBeNull();

            var roles = staff.Roles.ChildrenQueryable().Select(r => r.ID).ToList();
            roles.Should().Contain(r => r == roleId.ToObjectId());
        }

        [Then(@"other staff role \[(.*)\] record removed")]
        public async Task ThenOtherStaffRecordRolesWereRemoved(string roleId)
        {
            var staff = await _context.GetRecord<StaffEntity>(Constants.AnotherStaffId).ConfigureAwait(false);

            staff.Should().NotBeNull();

            var roles = staff.Roles.ChildrenQueryable().ToList();
            roles.Should().NotContain(r => r.ID == roleId);
        }

        [Then("staff record was created")]
        public async Task ThenStaffRecordWasCreated()
        {
            //Does NOT throw
            var @staff = await _context.GetRecord<StaffEntity>(Constants.AnotherStaffId).ConfigureAwait(false);
        }

        [Then("other staff record was updated")]
        public async Task ThenOtherStaffRecordWasUpdated()
        {
            var @staff = await _context.GetRecord<StaffEntity>(Constants.AnotherStaffId).ConfigureAwait(false);

            staff.Name.Should().Contain("Joe");
        }

        [Then("the member was deleted")]
        public async Task ThenTheMemberWasDeleted()
        {
            var memberId = _context.Get<string>(Constants.MemberId);
            var member = await DB.Find<MemberEntity>().OneAsync(memberId.ToObjectId()).ConfigureAwait(false);

            member.Should().BeNull();
        }

        [Then("a list of operators")]
        public async Task ThenAListOfOperators()
        {
            var responseData = await _webHost.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var body = JsonConvert.DeserializeObject<api._Features_.Agents.Roles.GetMany.Response>(responseData);
            body.Should().HaveCountGreaterThan(0);
        }
    }
}