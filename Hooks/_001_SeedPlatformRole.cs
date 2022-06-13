using System.Threading.Tasks;
using MongoDB.Entities;
using vMotion.Dal;
using vMotion.Dal.MongoDb;

namespace vMotion.Api.Specs
{
    public class _001_SeedPlatformRole : IMigration
    {
        public async Task UpgradeAsync()
        {
            var data = DbData.GetRole("Super Admin", null, DbSeedRoles.SuperAdminPermissions)
                .Then(_ =>
                {
                    _.ID = _.Name.ToObjectId();
                });
            await data.SaveAsync().ConfigureAwait(false);
        }
    }
}