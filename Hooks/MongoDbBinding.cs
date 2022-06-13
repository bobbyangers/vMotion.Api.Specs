using AutoFixture;
using MongoDB.Entities;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using vMotion.Dal.MongoDb;
using vMotion.Dal.MongoDb.Entities;

namespace vMotion.Api.Specs
{
    [Binding]
    public class MongoDbPrepBinding
    {
        [BeforeTestRun(Order = 1)]
        public static async Task BeforeTestRunHook()
        {
            await DB.InitAsync(TestConfiguration.GetDbName()).ConfigureAwait(false);

            await DB.MigrateAsync<MongoDbPrepBinding>().ConfigureAwait(false);
        }

        [AfterTestRun]
        public static async Task AfterTestRunHook()
        {
            var client = DB.Database(TestConfiguration.GetDbName()).Client;

            var dbNames = (await DB.AllDatabaseNamesAsync()).Where(x => x.StartsWith("TestRun_")).OrderByDescending(x => x);

            foreach (var db in dbNames.Skip(3))
            {
                await client.DropDatabaseAsync(db).ConfigureAwait(false);
            }
        }
    }

    [Binding]
    [Scope(Tag = "mongoDb")]
    public class OperatorData
    {
        [BeforeScenario(Order = 1)]
        public void BeforeScenario(ScenarioContext context, IFixture fixture)
        {
            context[Constants.OperatorId] = DbNames.Operator1_ID.ObjectIdToGuidString();

            context[Constants.RoleAgent] = DbConstants.DefaultRoleName.ToObjectId().ObjectIdToGuidString();
        }
    }

    [Binding]
    [Scope(Tag = "mongoDb")]
    public class MemberData
    {
        [BeforeScenario(Order = 2)]
        public async Task BeforeScenario(ScenarioContext context, IFixture fixture)
        {
            var op = context.Get<string>(Constants.OperatorId);
            var record = fixture.Create<MemberEntity>();
            record.Operator = op.ToObjectId();

            await record.SaveAsync().ConfigureAwait(false);

            context[Constants.MemberId] = record.ID.ObjectIdToGuidString();
        }
    }

    [Binding]
    [Scope(Tag = "mongoDb")]
    public class StaffData
    {
        [BeforeScenario(Order = 2)]
        public async Task BeforeScenario(ScenarioContext context, IFixture fixture)
        {
            var op = context.Get<string>(Constants.OperatorId);

            var record = fixture.Create<StaffEntity>();
            record.Operator = op.ToObjectId();

            await record.SaveAsync().ConfigureAwait(false);

            context[Constants.StaffId] = record.ID.ObjectIdToGuidString();
        }
    }
}