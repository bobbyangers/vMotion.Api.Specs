using AutoFixture;
using FluentAssertions;
using MongoDB.Driver.Linq;
using MongoDB.Entities;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using vMotion.Dal;
using vMotion.Dal.MongoDb.Entities;

namespace vMotion.Api.Specs.Steps
{
    [Binding]
    public class MemberOperatorsSteps
    {
        private readonly ScenarioContext _context;
        private readonly WebHostSupport _webHost;
        private readonly IFixture _fixture;

        public MemberOperatorsSteps(ScenarioContext context, WebHostSupport webHost, IFixture fixture)
        {
            _context = context;
            _webHost = webHost;
            _fixture = fixture;
        }

        [Given("clear linked operators in member")]
        public async Task GivenClearLinkedOperatorsInMember()
        {
            var @member = await _context.GetRecord<MemberEntity>(Constants.MemberId).ConfigureAwait(false);

            await member.LinkedOperators.RemoveAsync(member.LinkedOperators.Select(x => x.ID)).ConfigureAwait(false);
        }

        [Given(@"member is linked to \[(.*)\]")]
        public async Task GivenMemberIsLinkedTo(string code)
        {
            code = code.ToUpper();

            var op = await DB.Find<OperatorEntity>().Match(x => code == x.Code).ExecuteFirstAsync().ConfigureAwait(false);
            if (null == op)
            {
                throw new DataNotFoundException($"OperatorEntity[{code}] was not found");
            }

            var @member = await _context.GetRecord<MemberEntity>(Constants.MemberId).ConfigureAwait(false);

            await member.LinkedOperators.AddAsync(op.ID).ConfigureAwait(false);
        }

        [Then(@"member is linked to operator with code \[(.*)\]")]
        public async Task ThenMemberIsLinkedToOperatorWithCode(string code)
        {
            var op = await DB.Find<OperatorEntity>().Match(x => code.Equals(x.Code)).ExecuteFirstAsync().ConfigureAwait(false);

            var @member = await _context.GetRecord<MemberEntity>(Constants.MemberId).ConfigureAwait(false);

            var item = member.LinkedOperators.ChildrenQueryable()
                .Where(_ => op.ID.Equals(_.ID))
                .ToList();

            item.Should().NotBeNull();
        }

        [Then(@"member is not linked to operator with code \[(.*)\]")]
        public async Task ThenMemberIsNotLinkedToOperatorWithCode(string code)
        {
            var @member = await _context.GetRecord<MemberEntity>(Constants.MemberId).ConfigureAwait(false);

            var item = member.LinkedOperators
                .ChildrenQueryable()
                .Where(_ => code.Equals(_.ID))
                .ToList();

            item.Should().BeEmpty();
       }

    }
}