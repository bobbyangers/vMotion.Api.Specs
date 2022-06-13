using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using AutoFixture;

using AutoMapper;

using FluentAssertions;

using IdentityModel.Client;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Bson;
using MongoDB.Entities;

using NSubstitute;

using vMotion.api.Data;
using vMotion.Dal;
using vMotion.Dal.MongoDb;
using vMotion.Dal.MongoDb.Entities;

using Xunit;
using Xunit.Abstractions;

namespace vMotion.Api.Specs.Unit_Tests;

#pragma warning disable S2699 // At least one assertion
[Trait(Constants.Category, Constants.CI)]
public class _EntityToDtoProfileTests : AutoFixtureTests<IMapper>
{
    public _EntityToDtoProfileTests(ITestOutputHelper output) : base(output)
    {
        CustomizeDomain(Fixture);

        var services = Services;

        services.AddAutoMapper(c =>
            {
                c.AllowNullCollections = true;
                c.AddGlobalIgnore(nameof(BaseEntity.ID));
                c.AddGlobalIgnore(nameof(BaseEntity.ModifiedOn));
                c.AddGlobalIgnore(nameof(BaseEntity.Version));
            },
            typeof(_EntityToDtoProfile).Assembly,
            typeof(MongoDbConfig).Assembly);

        services.AddSingleton(_ =>
        {
            var storage = Substitute.For<IBlobStorage>();
            storage.GetBlobUrl(Arg.Any<string>())
                .Returns(info =>
                {
                    var imgData = info.ArgAt<string>(0).Split(';');
                    return new Uri($"http://locahost/{imgData[0]}/{imgData[1]}");
                });

            return storage;
        });

        var container = Services.BuildServiceProvider();

        Sut = container.GetRequiredService<IMapper>();

    }

    private void CustomizeDomain(IFixture f)
    {
        #region Operator

        f.Customize<OperatorEntity>(x => x
            .With(_ => _.ID, () => DB.Entity<CaseEntity>().GenerateNewID())
            .With(_ => _.ImageUrl, $"Container1;filename1.jpg")
            .Without(_ => _.Members)
            .Without(_ => _.Roles)
        );

        f.Customize<RoleEntity>(x => x
            .With(_ => _.ID, () => DB.Entity<RoleEntity>().GenerateNewID())
            .With(_ => _.Operator, () => DB.Entity<OperatorEntity>().GenerateNewID())
        );


        #endregion

        #region Staff
        f.Customize<StaffEntity>(_ => _
            .With(x => x.ID, () => DB.Entity<StaffEntity>().GenerateNewID())
            .With(x => x.CurrentCall, () => DB.Entity<CallEntity>().GenerateNewID())
            .With(x => x.Operator, () => DB.Entity<OperatorEntity>().GenerateNewID())
            .Without(x => x.Roles)
        );

        f.Customize<StaffOnlineEntity>(_ => _
            .With(x => x.ID, () => DB.Entity<StaffOnlineEntity>().GenerateNewID())
        );
        #endregion

        #region Members
        f.Customize<MemberEntity>(x => x
            .With(_ => _.ID, () => DB.Entity<MemberEntity>().GenerateNewID())
            .With(_ => _.ProfilePictureUrl, $"Container1;filename1.jpg")
            .With(_ => _.Operator, () => DB.Entity<OperatorEntity>().GenerateNewID())
            .With(_ => _.CurrentCall, () => DB.Entity<CallEntity>().GenerateNewID())
        );

        f.Customize<MemberNotificationEntity>(x => x
            .With(_ => _.ID, () => DB.Entity<MemberNotificationEntity>().GenerateNewID())
            .With(_ => _.Operator, () => DB.Entity<OperatorEntity>().GenerateNewID())
            .With(_ => _.IsRead, false)
            .With(_ => _.NotificationType, NotificationEnum.Alert)
            .With(_ => _.Data, () => $"{{'data':'{Guid.NewGuid():N}'}}")
        );
        #endregion

        #region Cases
        f.Customize<CaseEntity>(x => x
            .With(_ => _.ID, () => ObjectId.GenerateNewId().ToString())
            .With(_ => _.Pictures, new[] { $"Container1;filename1.jpg" }.ToList())
            .With(_ => _.ByMember, () => ObjectId.GenerateNewId().ToString())
            .With(_ => _.Operator, () => ObjectId.GenerateNewId().ToString())
            .With(_ => _.SelectedCard, () => ObjectId.GenerateNewId().ToString())
            );

        f.Customize<CaseNoteEntity>(x => x
            .With(_ => _.ID, () => ObjectId.GenerateNewId().ToString())
            .With(_ => _.Call, () => ObjectId.GenerateNewId().ToString())
        );

        f.Customize<CasePickupWorkflowEntity>(x => x
            .With(_ => _.ID, () => ObjectId.GenerateNewId().ToString())
            .With(_ => _.Inspection, "{ 'a':'1' }")
        );

        f.Customize<WorkflowStepDto>(x => x
            .With(_ => _.Color)
            .With(_ => _.Data, new { a = 1 })
        );

        f.Customize<WorkflowStepElement>(x => x
            .With(_ => _.Color)
            .With(_ => _.Data, "{ 'a':'1' }")
        );


        f.Customize<api._Features_.Clients.Notifications.Post.Request>(_ => _
            .With(x => x.NotificationType, NotificationEnum.Alert)
        );

        f.Customize<AddMemberNotificationEvent>(_ => _
            .With(x => x.NotificationType, NotificationEnum.Alert)
        );

        f.Customize<api._Features_.Clients.Notifications.Put.Request>(_ => _
            .With(x => x.IsRead, true)
            .With(x => x.Now, DateTime.UtcNow)
        );
        #endregion

        #region Call
        f.Customize<CallEntity>(_ => _
                .With(x => x.ID, ObjectId.GenerateNewId().ToString())
                .With(x => x.RelatedCase, ObjectId.GenerateNewId().ToString())
                .With(x => x.ByMember, ObjectId.GenerateNewId().ToString())
                .With(x => x.OngoingCallBy, ObjectId.GenerateNewId().ToString())
                .With(x => x.AssignedStaff, ObjectId.GenerateNewId().ToString())
                .With(x => x.Operator, ObjectId.GenerateNewId().ToString())
                .Without(x => x.Data))
            ;

        f.Customize<DeviceElement>(x => x
            .With(_ => _.DeviceId, Guid.NewGuid().ToString("N"))
            .With(_ => _.DeviceType)
            .Without(_ => _.GeoLocation)
        );

        f.Customize<GeoLocationElement>(x => x);

        f.Customize<ExtraHeadersDto>(x => x
            .With(_ => _.DeviceId, Guid.NewGuid().ToString("N"))
            .With(_ => _.UserAgent, "iOS/14.0")
            .With(_ => _.GeoLocation, "{ 'lat': 271, 'long': 43 }")
        );


        f.Customize<DeviceDto>(x => x
            .With(_ => _.DeviceId, Guid.NewGuid().ToString("N"))
            .With(_ => _.UserAgent, "iOS/14.0")
            .Without(_ => _.GeoLocation));
        #endregion
    }

    [Fact]
    public void IsConfigValidTest()
    {
        Sut.ConfigurationProvider.AssertConfigurationIsValid();
    }

    #region Common Types

    [Fact]
    public void WhenMap_string_Guid()
    {
        var data = vMotion.Dal.StringExtensions.ToObjectId((Guid)Fixture.Create<Guid>());

        var result = Sut.Map<Guid>(data);

        ShowResult(result);
    }

    [Fact]
    public void WhenMap_string_Nullable_Guid_()
    {
        var data = vMotion.Dal.StringExtensions.ToObjectId((Guid)Fixture.Create<Guid>());

        var result = Sut.Map<Guid?>(data);

        ShowResult(result);
    }

    [Fact]
    public void WhenMap_DateTime_DateTimeOffset()
    {
        var data = DateTime.Now;

        var result = Sut.Map<DateTimeOffset>(data);

        ShowResult(result);
    }

    [Fact]
    public void WhenMap_DateTime__DateTimeOffset()
    {
        var data = new DateTime?(DateTime.Now);

        var result = Sut.Map<DateTimeOffset?>(data);

        ShowResult(result);

        result.Should().BeOnOrAfter(new DateTimeOffset(DateTime.Today));
    }

    [Fact]
    public void WhenMap_DateTime_null_DateTimeOffset_should_get_null()
    {
        var result = Sut.Map<DateTimeOffset?>((DateTime?)null);

        ShowResult(result);
        result.Should().BeNull();
    }

    [Fact]
    public void WhenMap_string_Uri()
    {
        var data = new ImageData("container", "filename.png");

        var result = Sut.Map<Uri>(data.ToString());

        ShowResult(result);
    }

    [Fact]
    public void WhenMap_ImageData_Uri()
    {
        var data = new ImageData("container", "filename.png");

        var result = Sut.Map<Uri>(data);

        ShowResult(result);
    }

    [Fact]
    public async Task WhenMap_TokenResponse_AuthData()
    {
        ////var content = new StringContent("{}");

        var content = JsonContent.Create(new
        {
            access_token = Guid.NewGuid().ToString("N"),
            scope = "SomeScope",
            tokenType = "bearer",
            refresh_token = Guid.NewGuid().ToString("N"),
            expires_in = 1800
        });

        var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK);
        fakeResponse.Content = content;

        var data = await ProtocolResponse.FromHttpResponseAsync<TokenResponse>(fakeResponse).ConfigureAwait(false);

        var result = Sut.Map<AuthData>(data);

        ShowResult(result);
    }
    #endregion

    #region OperatorEntity
    [Fact]
    public void WhenMap_OperatorEntity_OperatorDto()
    {
        var data = Fixture.Create<OperatorEntity>();
        ShowResult(data);

        var result = Sut.Map<OperatorDto>(data);

        result.Should().NotBeNull();
        ShowResult(result);
    }

    [Fact]
    public void WhenMap_OperatorEntity_OperatorSummaryDto()
    {
        var data = Fixture.Create<OperatorEntity>();
        ShowResult(data);

        var result = Sut.Map<OperatorSummaryDto>(data);

        result.Should().NotBeNull();
        ShowResult(result);
    }

    [Fact]
    public void WhenMap_OperatorEntity_RecordReference()
    {
        var data = Fixture.Create<OperatorEntity>();
        ShowResult(data);

        var result = Sut.Map<RecordReference>(data);

        result.Should().NotBeNull();
        ShowResult(result);
    }

    [Fact]
    public void WhenMap_RoleEntity_RoleDto()
    {
        var data = Fixture.Create<RoleEntity>();

        var result = Sut.Map<RoleDto>(data);

        ShowResult(result);
        result.Id.Should().NotBeEmpty();
        result.Name.Should().NotBeEmpty();
    }

    [Fact]
    public void WhenMap_Operators_Post_Request_OperatorEntity()
    {
        var data = Fixture.Create<vMotion.api._Features_.Agents.Operators.Post.Request>();
        var result = Sut.Map<OperatorEntity>(data);

        ShowResult(new { result.ID, result.Code, result.Name, result.Address });

        result.ID.Should().BeNull();
    }

    [Fact]
    public void WhenMap_Operators_Put_Request_OperatorEntity()
    {
        var data = Fixture.Build<vMotion.api._Features_.Agents.Operators.Put.Request>()
            .With(_ => _.Id, DB.Entity<OperatorEntity>().GenerateNewID().ToGuid())
            .Without(x => x.Email)
            .With(x => x.Code, "AAA111")
            .Create();

        var result = Sut.Map<OperatorEntity>(data);

        ShowResult(result);

        result.ID.Should().NotBeNull();
        result.Name.Should().NotBeEmpty();
        result.Code.Should().Be("AAA111");
    }

    [Fact]
    public void WhenMap_Roles_Post_Request_RoleEntity()
    {
        var data = Fixture.Create<api._Features_.Agents.Roles.Post.Request>();

        var result = Sut.Map<RoleEntity>(data);

        ShowResult(result);

        result.ID.Should().BeNull(); ////.Be("000000000000000000000000");
        result.Name.Should().NotBeEmpty();
    }

    [Fact]
    public void WhenMap_Roles_Put_Request_RoleEntity()
    {
        var data = Fixture.Create<api._Features_.Agents.Roles.Put.Request>();

        var role = Fixture.Create<RoleEntity>();
        var result = Sut.Map(data, role);

        result.ID.Should().NotBeEmpty();
        result.Name.Should().NotBeEmpty();
    }
    #endregion

    #region StaffEntity
    [Fact]
    public void WhenMap_StaffEntity_RecordReference()
    {
        var data = Fixture.Create<StaffEntity>();

        var result = Sut.Map<RecordReference>(data);

        ShowResult(result);
        result.Id.Should().NotBeEmpty();
        result.Name.Should().NotBeEmpty();
    }

    [Fact]
    public void WhenMap_StaffEntity_UserReference()
    {
        var data = Fixture.Create<StaffEntity>();

        var result = Sut.Map<UserReference>(data);

        ShowResult(result);
        result.Id.Should().NotBeEmpty();
        result.Name.Should().NotBeEmpty();
    }

    [Fact]
    public void WhenMap_StaffOnlineEntity_StaffUserReference()
    {
        var data = Fixture.Create<StaffOnlineEntity>();

        var result = Sut.Map<StaffUserReference>(data);

        ShowResult(result);
        result.Id.Should().NotBeEmpty();
        result.Name.Should().NotBeEmpty();
    }

    [Fact]
    public void WhenMap_StaffEntity_StaffOnlineEntity()
    {
        var data = Fixture.Create<StaffEntity>();

        var result = Sut.Map<StaffOnlineEntity>(data);

        ShowResult(result);
        result.ID.Should().NotBeEmpty();
        result.Name.Should().NotBeEmpty();
    }

    [Fact]
    public void WhenMap_Staff_Post_Request_StaffEntity()
    {
        var data = Fixture.Create<api._Features_.Agents.Staff.Post.Request>();

        var result = Sut.Map<StaffEntity>(data);

        ////ShowResult(result);

        result.ID.Should().BeNullOrEmpty();
        result.Name.Should().NotBeEmpty();
    }

    [Fact]
    public void WhenMap_StaffEntity_StaffUpdatedEvent()
    {
        var data = Fixture.Create<StaffEntity>();

        var result = Sut.Map<StaffUpdatedEvent>(data);

        ShowResult(result);

        result.Should().NotBeNull();
    }

    [Fact]
    public void WhenMap_StaffEntity_Get_Response()
    {
        var data = Fixture.Create<StaffEntity>();

        var result = Sut.Map<api._Features_.Agents.Staff.Get.Response>(data);

        result.Should().NotBeNull();

        ShowResult(result);
    }

    [Fact]
    public void WhenMap_StaffEntity_GetMany_Response()
    {
        var data = Fixture.CreateMany<StaffEntity>();

        var result = Sut.Map<IEnumerable<StaffSummaryDto>>(data);

        ShowResult(result);
    }
    #endregion

    #region MembersEntity
    [Fact]
    public void WhenMap_MemberEntity_MemberDto()
    {
        var data = Fixture.Create<MemberEntity>();

        var result = Sut.Map<MemberDto>(data);

        result.Should().NotBeNull();
        ShowResult(result);
    }

    [Fact]
    public void WhenMap_MemberEntity_UserReference()
    {
        var data = Fixture.Create<MemberEntity>();

        var result = Sut.Map<UserReference>(data);

        result.Should().NotBeNull();
        ShowResult(result);
    }

    [Fact]
    public void WhenMap_MemberEntity_MemberReference()
    {
        var data = Fixture.Create<MemberEntity>();

        var result = Sut.Map<MemberReference>(data);

        result.Should().NotBeNull();
        ShowResult(result);
    }

    [Fact]
    public void WhenMap_GetUserResponse_MemberEntity()
    {
        var data = Fixture.Create<GetUserResponse>();

        var result = Sut.Map<MemberEntity>(data);

        result.Should().NotBeNull();
        ShowResult(result);
    }

    [Fact]
    public void WhenMap_MemberNotificationEntity_MemberNotificationDto()
    {
        var data = Fixture.Create<MemberNotificationEntity>();
        var result = Sut.Map<MemberNotificationDto>(data);

        ShowResult(result);

        result.NotificationType.Should().Be(NotificationEnum.Alert);
        result.IsRead.Should().BeFalse();
    }

    [Fact]
    public void WhenMap_AddMemberNotificationEvent_MemberNotificationsEntity()
    {
        var data = Fixture.Create<AddMemberNotificationEvent>();
        var result = Sut.Map<MemberNotificationEntity>(data);

        ShowResult(result);

        result.NotificationType.Should().Be(NotificationEnum.Alert);
        result.IsRead.Should().BeFalse();
    }

    [Fact]
    public void WhenMap_Clients_Notification_Put_Request_to_MemberNotificationsEntity()
    {
        var data = Fixture.Create<vMotion.api._Features_.Clients.Notifications.Put.Request>();
        var result = Sut.Map<MemberNotificationEntity>(data);

        ShowResult(result);

        result.IsRead.Should().BeTrue();
    }

    [Fact]
    public void WhenMap_UpdateNotificationCommand_nomap_MemberNotificationsEntity()
    {
        var data = Fixture.Build<vMotion.api._Features_.Clients.Notifications.Put.Request>()
            .Without(x => x.IsRead)
            .Create();

        var target = Fixture.Build<MemberNotificationEntity>()
            .With(x => x.NotificationType, NotificationEnum.Alert)
            .With(x => x.IsRead, true)
            .Create();

        Sut.Map(data, target);

        ShowResult(target);

        target.IsRead.Should().BeTrue();
    }

    [Fact]
    public void WhenMap_UpdateNotificationCommand_doesmap_MemberNotificationElement()
    {
        var data = Fixture.Build<vMotion.api._Features_.Clients.Notifications.Put.Request>()
            .With(x => x.IsRead, true)
            .Create();

        var target = Fixture.Build<MemberNotificationEntity>()
            .With(x => x.NotificationType, NotificationEnum.Alert)
            .With(x => x.IsRead, false)
            .Create();

        Sut.Map(data, target);

        ShowResult(target);

        target.IsRead.Should().BeTrue();
    }
    #endregion

    #region CaseEntity
    [Fact]
    public void WhenMap_CaseEntity_CallEntity()
    {
        var data = Fixture.Create<CaseEntity>();

        var result = Sut.Map<CallEntity>(data);

        result.Should().NotBeNull();

        ShowResult(result);
    }

    [Fact]
    public void WhenMap_CaseEntity_CaseSummaryDto()
    {
        var data = Fixture.Create<CaseEntity>();

        var result = Sut.Map<CaseSummaryDto>(data);

        result.Should().NotBeNull();

        ShowResult(result);
    }

    [Fact]
    public void WhenMap_CaseEntity_CallCaseDto()
    {
        var data = Fixture.Create<CaseEntity>();

        var result = Sut.Map<CallCaseDto>(data);

        result.Should().NotBeNull();

        ShowResult(result);
    }

    [Fact]
    public void WhenMap_CaseEntityWithoutCard_CallCaseDto()
    {
        var data = Fixture.Create<CaseEntity>();
        data.SelectedCard = null;

        var result = Sut.Map<CallCaseDto>(data);

        result.Should().NotBeNull();

        ShowResult(result);
    }

    [Fact]
    public void WhenMap_CaseEntity_CaseCreatedEvent()
    {
        var data = Fixture.Create<CaseEntity>();

        var result = Sut.Map<CaseCreatedEvent>(data);

        result.Should().NotBeNull();

        ShowResult(result);
    }

    [Fact]
    public void WhenMap_CaseNoteEntity_CaseNoteDto()
    {
        var data = Fixture.Create<CaseNoteEntity>();
        data.Call = null;

        var result = Sut.Map<CaseNoteDto>(data);

        result.Should().NotBeNull();

        ShowResult(result);
    }

    [Fact]
    public void WhenMap_CaseEntity_Response()
    {
        var data = Fixture.Create<CaseEntity>();

        var result = Sut.Map<vMotion.api._Features_.Agents.Cases.Get.Response>(data);

        ShowResult(result);
        result.CaseId.Should().Be(vMotion.Dal.StringExtensions.ToGuid(data.ID));
    }

    [Fact]
    public void WhenMap_Clients_Cases_InProgress_Response()
    {
        var data = Fixture.Create<CaseEntity>();

        var result = Sut.Map<vMotion.api._Features_.Clients.Cases.Get.InProgress.Response>(data);

        result.Should().NotBeNull();

        ShowResult(result);
    }

    [Fact]
    public void WhenMap_CasePickupWorkflowEntity_Response()
    {
        var data = Fixture.Create<CasePickupWorkflowEntity>();

        var result = Sut.Map<vMotion.api._Features_.Clients.Cases.Pickup_workflow.Get.Response>(data);

        result.Should().NotBeNull();

        ShowResult(result);
    }

    [Fact]
    public void WhenMap_CaseNote_Post_Request_CaseNoteEntity()
    {
        var data = Fixture.Create<vMotion.api._Features_.Agents.Cases.Notes.Post.Request>();

        var result = Sut.Map<CaseNoteEntity>(data);

        result.Should().NotBeNull();

        ShowResult(result);
    }

    [Fact]
    public void WhenMap_WorkflowStep_WorkflowStepElement()
    {
        var data = Fixture.Create<WorkflowStepDto>();

        var result = Sut.Map<WorkflowStepElement>(data);

        result.Should().NotBeNull();

        ShowResult(result);
    }

    [Fact]
    public void WhenMap_WorkflowStepElement_WorkflowStep()
    {
        var data = Fixture.Create<WorkflowStepElement>();

        var result = Sut.Map<WorkflowStepDto>(data);

        result.Should().NotBeNull();

        ShowResult(result);
    }

    [Fact]
    public void WhenMap_CaseNoteEntity_CaseNoteDto_with_callId()
    {
        var data = Fixture.Create<CaseNoteEntity>();

        var result = Sut.Map<CaseNoteDto>(data);

        result.Should().NotBeNull();

        ShowResult(result);
    }


    #endregion

    #region CallEntity
    [Fact]
    public void WhenMap_CallEntity_CallDto()
    {
        var data = Fixture.Create<CallEntity>();

        var result = Sut.Map<CallDto>(data);

        result.Should().NotBeNull();
        ShowResult(result);
    }

    [Fact]
    public void WhenMap_CallEntity_CallSummaryDto()
    {
        var data = Fixture.Create<CallEntity>();

        var result = Sut.Map<CallSummaryDto>(data);

        result.Should().NotBeNull();

        ShowResult(result);
    }

    [Fact]
    public void WhenMap_CallEntity_CurrentCallDto()
    {
        var data = Fixture.Create<CallEntity>();

        var result = Sut.Map<CurrentCallDto>(data);

        ShowResult(result);

        result.Should().NotBeNull();
    }

    [Fact]
    public void WhenMap_CallEntity_CallHistoryDto()
    {
        var data = Fixture.Create<CallEntity>();

        var result = Sut.Map<CallHistoryDto>(data);

        ShowResult(result);

        result.Should().NotBeNull();
    }

    [Fact]
    public void WhenMap_CallEntity_CallCreatedEvent()
    {
        var data = Fixture.Create<CallEntity>();

        var result = Sut.Map<CallCreatedEvent>(data);

        ShowResult(result);

        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void WhenMap_DeviceDto_DeviceElement()
    {
        var data = Fixture.Create<DeviceDto>();

        var result = Sut.Map<DeviceElement>(data);

        ShowResult(result);

        result.Should().NotBeNull();
    }

    [Fact]
    public void WhenMap_GeoLocationDto_GeoLocationElement()
    {
        var data = Fixture.Create<GeoLocationDto>();

        var result = Sut.Map<GeoLocationElement>(data);

        ShowResult(result);

        result.Should().NotBeNull();
    }

    [Fact]
    public void WhenMap_CallHistoryElement_CallHistoryDto()
    {
        var data = Fixture.Create<CallHistoryElement>();

        var result = Sut.Map<CallAuditDto>(data);

        ShowResult(result);

        result.Should().NotBeNull();
    }

    [Fact]
    public void WhenMap_Clients_Calls_Get_Request_Response()
    {
        var data = Fixture.Create<CallEntity>();

        var result = Sut.Map<vMotion.api._Features_.Clients.Calls.Post.Response>(data);

        result.Should().NotBeNull();

        ShowResult(result);
    }
    #endregion

    #region Registration
    [Fact]
    public void WhenMap_Clients_Register_Post_Request_CreateUserRequest()
    {
        var data = Fixture.Create<api._Features_.Clients.Register.Post.Request>();

        var result = Sut.Map<CreateUserRequest>(data);

        result.Should().NotBeNull();

        ShowResult(result);
    }
    #endregion
}
#pragma warning restore S2699 // At least one assertion