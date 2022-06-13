using AutoFixture;
using AutoMapper;
using FluentAssertions;
using MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using vMotion.Dal;
using vMotion.Dal.MongoDb.Entities;

namespace vMotion.Api.Specs.Steps
{
    [Binding]
    [Scope(Feature = "MemberNotifications")]
    public class MemberNotificationsSteps
    {
        private readonly ScenarioContext _context;
        private readonly IFixture _fixture;
        private readonly IMapper _mapper;


        public MemberNotificationsSteps(ScenarioContext context, IFixture fixture)
        {
            _context = context;
            _fixture = fixture;

            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<NotificationTableData, MemberNotificationEntity>()
                    .ForMember(_ => _.ID, _ => _.MapFrom(s => (string)null))
                    .ForMember(_ => _.NotificationType, _ => _.MapFrom(s => s.NotificationType))
                    .ForMember(_ => _.Description, _ => _.MapFrom(s => s.Description))
                    .ForMember(_ => _.IsRead, _ => _.MapFrom(s => s.IsRead))
                    .ForMember(_ => _.IsArchived, _ => _.MapFrom(s => s.IsArchived))
                    ;
            });

            _mapper = config.CreateMapper();
        }

        [BeforeScenario(Order = 100)]
        public void BeforeScenario()
        {
            _fixture.Customize<CaseEntity>(_ => _
                .With(x => x.ID, (string)null)
                .With(x => x.Status, CaseStatus.New)
                .With(x => x.InfoDateValidated, DateTime.UtcNow.AddMinutes(-1))
                .With(x => x.ReservationId, Guid.NewGuid().ToString("N"))
                .OmitAutoProperties()
            );

            _fixture.Customize<MemberNotificationEntity>(_ => _
                .With(x => x.ID, (string)null)
                .With(x => x.Description)
                .With(x => x.NotificationType, NotificationEnum.Alert)
                .OmitAutoProperties()
            );

        }

        [Given("the member has a current call set")]
        public async Task GivenTheMemberHasACurrentCallSet()
        {
            var @operator = await _context.GetRecord<OperatorEntity>(Constants.OperatorId).ConfigureAwait(false);
            var member = await _context.GetRecord<MemberEntity>(Constants.MemberId).ConfigureAwait(false);

            var @case = _fixture.Create<CaseEntity>().Then(_ =>
            {
                _.ByMember = member.ID;
                _.Operator = @operator.ID;
            });
            @case.CaseNumber = await DB.Entity<CaseEntity>().NextSequentialNumberAsync();
            await @case.SaveAsync();

            var call = _fixture.Create<CallEntity>();
            call.ByMember = member.ID;
            call.RelatedCase = @case.ID;
            await call.SaveAsync();

            await @case.Calls.AddAsync(@call.ID);

            var result = await DB.Update<MemberEntity>()
                .MatchID(member.ID)
                .Modify(_ => _.CurrentCall, @call.ID)
                .ExecuteAsync()
                .ConfigureAwait(false);
            result.ModifiedCount.Should().Be(1);

            _context.Set(@case.ID.ObjectIdToGuidString(), Constants.CaseId);
            _context.Set(call.ID.ObjectIdToGuidString(), Constants.CallId);

        }

        [Given("a notification record exists")]
        public async Task GivenANotificationRecordExists()
        {
            var opId = _context.Get<string>(Constants.OperatorId).ToObjectId();

            var member = await _context.GetRecord<MemberEntity>(Constants.MemberId)
                .ConfigureAwait(false);

            if (!member.Notifications.Any())
            {
                var data = _fixture.Create<MemberNotificationEntity>();
                data.Operator = opId;
                data.Member = member.ID;

                await data.SaveAsync();

                await member.Notifications.AddAsync(data.ID);
            }

            _context.Set(member.Notifications.First().ID.ObjectIdToGuidString(), Constants.NotificationId);
        }

        [Given("with these notifications in database")]
        public async Task GivenWithTheseCallsInDatabase(IEnumerable<NotificationTableData> table)
        {
            var op = await _context.GetRecord<OperatorEntity>(Constants.OperatorId);

            var member = await _context.GetRecord<MemberEntity>(Constants.MemberId)
                .ConfigureAwait(false);

            var records = new List<string>();
            foreach (var row in table)
            {
                var record = _mapper.Map<MemberNotificationEntity>(row);

                record.Operator = op.ID;
                record.Member = member.ID;

                await record.SaveAsync().ConfigureAwait(false);

                records.Add(record.ID);
            }

            await member.Notifications.AddAsync(records);
        }

        [Then("member-notification record was updated")]
        public async Task ThenMemberNotificationRecordWasUpdated()
        {
            ////await Support.FakeDb.Notifications.ReceivedWithAnyArgs().UpdateAsync(default);
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Then("member-notification record was marked \\[(.*)\\]")]
        public async Task ThenMemberNotificationRecordWasUpdatedAndMarkedAsUnread(bool expected)
        {
            var member = await _context.GetRecord<MemberEntity>(Constants.MemberId)
                .ConfigureAwait(false);


            var note = member.Notifications.First();
            note.IsRead.Should().Be(expected);
        }

        [Then("member-notification record was marked as archived")]
        public async Task ThenMemberNotificationRecordWasUpdatedAndMarkedAsArchived()
        {
            var member = await _context.GetRecord<MemberEntity>(Constants.MemberId)
                .ConfigureAwait(false);

            var note = member.Notifications.First();
            note.IsArchived.Should().BeTrue();
        }

    }
}
