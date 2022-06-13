using System;
using System.Threading.Tasks;
using MongoDB.Entities;
using vMotion.Dal;
using vMotion.Dal.MongoDb;
using vMotion.Dal.MongoDb.Entities;

namespace vMotion.Api.Specs
{
    public class _003_SeedCalls ////: IMigration
    {
        public async Task UpgradeAsync()
        {
            var op = await DB.Find<OperatorEntity>().OneAsync(DbNames.Operator1_ID).ConfigureAwait(false);

            var member1 = await DB.Find<MemberEntity>().OneAsync(DbNames.Member1.ToObjectId()).ConfigureAwait(false);

            var staff1 = await DB.Find<StaffEntity>().OneAsync(DbNames.Staff1.ToObjectId()).ConfigureAwait(false);

            // Recent call by member2
           var call2 = new CallEntity().Then(_ =>
            {
                var tm = DateTime.UtcNow.AddDays(-2);

                _.ID = DB.Entity<CallEntity>().GenerateNewID();
                _.InQueueTime = tm;
                _.BeginCallTime = tm.AddMinutes(5);
                _.EndCallTime = tm.AddMinutes(12); ;
                _.Status = CallStatus.Completed;
                _.InVideoCallStatus = VideoCallStatus.Done;
                _.Operator = op;
                _.ByMember = member1;
            });
           await call2.SaveAsync();

            var call1 = new CallEntity().Then(_ =>
            {
                _.ID = DB.Entity<CallEntity>().GenerateNewID();
                _.InVideoCallStatus = VideoCallStatus.Waiting;
                _.InQueueTime = DateTime.UtcNow;
                _.Status = CallStatus.NewCase;
                _.InVideoCallStatus = VideoCallStatus.Waiting;
                _.Operator = op;

                _.ByMember = member1;
            });

            await call1.SaveAsync();


            var case1 = new CaseEntity().Then(_ =>
            {
                _.ID = DB.Entity<CaseEntity>().GenerateNewID();

                var caseNbr = DB.NextSequentialNumberAsync<CaseEntity>();
                caseNbr.Wait();
                _.CaseNumber = caseNbr.Result;
                _.Operator = op;

                _.ByMember = member1;
            }); 
            await case1.SaveAsync();
            await case1.Calls.AddAsync(call1);

            call1.RelatedCase = case1;
            call1.OngoingCallBy = staff1;

            await call1.SaveOnlyAsync(x => new { x.RelatedCase });

            staff1.CurrentCall = call1;
            await staff1.SaveOnlyAsync(x => new { x.CurrentCall });


            member1.CurrentCall = call1;
            await member1.SaveOnlyAsync(x => new {x.CurrentCall });
        }
    }
}