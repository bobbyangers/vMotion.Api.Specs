using AutoFixture;
using AutoFixture.AutoNSubstitute;
using MongoDB.Entities;
using System;
using System.Threading.Tasks;
using vMotion.Dal;
using vMotion.Dal.MongoDb;
using vMotion.Dal.MongoDb.Entities;

namespace vMotion.Api.Specs
{
#pragma warning disable S101 // Types should be named in PascalCase
    public class _002_SeedOperator1 : IMigration
#pragma warning restore S101 // Types should be named in PascalCase
    {
        private readonly IFixture _fixture = new Fixture();

        public _002_SeedOperator1()
        {
            _fixture.Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });

            _fixture.Customize(PostalCodeStringsGenerator.ToCustomization());
            _fixture.Customize(CountryCodeStringsGenerator.ToCustomization());

            _fixture.Customize(PhoneStringsGenerator.ToCustomization());
            _fixture.Customize(EmailAddressStringsGenerator.ToCustomization());
            _fixture.Customize(LinkSpecimenBuilder.ToCustomization());
            _fixture.Customize(IdInterneSpecimenBuilder.ToCustomization());

            _fixture.Customize<OperatorEntity>(_ => _
                .With(x => x.ID, DbNames.Operator1_ID)
                .With(x => x.Name, DbNames.Operator1_Name)
                .With(x => x.Code, "A000")
                .With(x => x.Version, 1)
                .With(x => x.AdultAge, 21)
                .With(x => x.WorkHourStart, TimeSpan.FromHours(9))
                .With(x => x.WorkHourEnd, TimeSpan.FromHours(17))
                .With(x => x.VideoProvider, VideoProvider.None)
                .With(x => x.MapProvider, MapProvider.Google)
                .With(x => x.SyncProvider, SyncProvider.None)
                .With(x => x.Timezone, "EST")
                .With(x => x.IsDefault, true)
            );
        }

        public async Task UpgradeAsync()
        {
            var platformRole = await DB.Find<RoleEntity>().OneAsync("Super Admin".ToObjectId()).ConfigureAwait(false);

            var op = _fixture.Create<OperatorEntity>();

            await op.SaveAsync().ConfigureAwait(false);

            DbData.ReasonList.Then(_ =>
            {
                _.ID = op.ID;
                _.SaveAsync().Wait();
            });

            DbData.Intentions.Then(_ =>
            {
                _.ID = op.ID;
                _.SaveAsync().Wait();
            });

            var adminRole1 = DbData.GetRole("Administrator Role #1", op.ID, DbSeedRoles.AdminPermissions)
                .Then(_ => _.SaveAsync().Wait());

            DbData.GetRole(DbConstants.DefaultRoleName, op.ID, DbSeedRoles.CallAgentPermissions)
                .Then(_ => _.SaveAsync().Wait());

            DbData.GetRole("Developer Role", op.ID, DbSeedRoles.DeveloperPermissions)
                .Then(_ => _.SaveAsync().Wait());

            ////await op.Roles.AddAsync(new[] {administratorRole1.ID, callAgentRole1.ID, developerRole.ID});

            var staffAdmin = DbData.StaffAdmin;
            await staffAdmin.SaveAsync();
            await staffAdmin.Roles.AddAsync(new[] { adminRole1.ID, platformRole.ID });

            var online = new StaffOnlineEntity().Then(_ =>
            {
                _.ID = staffAdmin.ID;
                _.Name = staffAdmin.Name;
                _.UserType = staffAdmin.UserType;
            });
            await online.SaveAsync();

            var member = DbData.Member1.Then(_ =>
            {
                _.Operator = op.ID;
            });

            await member.SaveAsync();

            await member.LinkedOperators.AddAsync(op.ID);
        }
    }
}