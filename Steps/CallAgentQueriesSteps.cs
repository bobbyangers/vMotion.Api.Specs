using AutoMapper;
using MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using vMotion.Dal;
using vMotion.Dal.MongoDb.Entities;

namespace vMotion.Api.Specs.Steps
{
    [Binding]
    public class CallAgentQueriesSteps
    {
        private readonly ScenarioContext _context;
        private IMapper Mapper { get; }

        public CallAgentQueriesSteps(ScenarioContext context)
        {
            _context = context;

            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<CallTableData, CallEntity>()
                    .ForMember(_ => _.ID, _ => _.MapFrom(s => (string)null))
                    .ForMember(_ => _.Summary, _ => _.MapFrom(s => s.Summary))
                    .ForMember(_ => _.OngoingCallBy, _ => _.MapFrom<UserRefResolver, string>(s => s.OngoingCallBy))
                    .ForMember(_ => _.InVideoCallStatus, _ => _.MapFrom<StringToEnumResolver, string>(s => s.InVideoCallStatus))
                    .ForMember(_ => _.AssignedToUserType, _ => _.MapFrom<StringToEnumResolver, string>(s => s.AssignedToUserType))
                    .ForMember(_ => _.InQueueTime, _ => _.MapFrom<TimeSpanToDateTimeResolver, TimeSpan>(s => s.ScheduledIn))
                    .ForMember(_ => _.AssignedStaff, _ => _.MapFrom<UserRefResolver, string>(s => s.AssignedTo))
                ;
            });

            Mapper = config.CreateMapper();
        }

        [Given("purge all calls")]
        public async Task GivenPurgeAllCalls()
        {
            var rows = await DB.Find<CallEntity>().Match(_ => true).ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

            await DB.DeleteAsync<CallEntity>(rows.Select(_ => _.ID)).ConfigureAwait(false);
        }

        [Given("with these calls in database")]
        public async Task GivenWithTheseCallsInDatabase(IEnumerable<CallTableData> table)
        {
            var op = await _context.GetRecord<OperatorEntity>(Constants.OperatorId);

            var records = Mapper.Map<IEnumerable<CallEntity>>(table).ToList();
            records.ForEach(x => x.Operator = op.ID);

            await DB.SaveAsync(records);
        }
    }

    public class StringToEnumResolver :
        IMemberValueResolver<object, object, string, VideoCallStatus>,
        IMemberValueResolver<object, object, string, UserTypeEnum>
    {
        public VideoCallStatus Resolve(
            object source, object destination,
            string sourceMember, VideoCallStatus destMember,
            ResolutionContext context)
        {
            if (Enum.TryParse(sourceMember, out VideoCallStatus data))
            {
                return data;
            }

            return destMember;
        }

        public UserTypeEnum Resolve(object source, object destination, string sourceMember, UserTypeEnum destMember,
            ResolutionContext context)
        {
            if (Enum.TryParse(sourceMember, out UserTypeEnum data))
            {
                return data;
            }

            return destMember;
        }
    }

    public class UserRefResolver :
        IMemberValueResolver<object, object, string, One<StaffEntity>>
    {
        public One<StaffEntity> Resolve(object source, object destination, string sourceMember, One<StaffEntity> destMember,
            ResolutionContext context)
        {
            return sourceMember switch
            {
                "$null$" => null,
                "$random$" => Guid.NewGuid().ToObjectId(),
                //"$me$" => CurrentUser,
                "" => destMember,
                _ => sourceMember.ToObjectId()
            };
        }
    }

    public class TimeSpanToDateTimeResolver :
        IMemberValueResolver<object, object, TimeSpan, DateTime>
    {
        public DateTime Resolve(object source, object destination, TimeSpan sourceMember, DateTime destMember,
            ResolutionContext context)
        {
            return TimeSpan.MinValue == sourceMember
                ? destMember
                : DateTime.UtcNow.Add(sourceMember);
        }
    }
}
