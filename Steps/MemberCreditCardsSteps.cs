using System.Threading.Tasks;

using AutoFixture;

using MongoDB.Entities;

using TechTalk.SpecFlow;

using vMotion.Api.Specs.Steps;
using vMotion.Dal.MongoDb.Entities;

namespace vMotion.Api.Specs
{
    [Binding]
    [Scope(Feature = "MemberCreditCards")]
    public class MemberCreditCardsSteps
    {
        private readonly ScenarioContext _context;
        private readonly IFixture _fixture;

        public MemberCreditCardsSteps(ScenarioContext context, IFixture fixture)
        {
            _context = context;
            _fixture = fixture;

            CustomizeDomain(_fixture);
        }

        private void CustomizeDomain(IFixture f)
        {
            f.Customize<MemberCreditCardAddressElement>(x => x
                .With(_ => _.Line1)
                .With(_ => _.City, "Montreal")
                .With(_ => _.State, "QC")
                .With(_ => _.Country, "CA")
                .With(_ => _.Zip, "H1H1H1")
            );

            f.Customize<MemberCreditCardEntity>(x => x
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
    }
}
