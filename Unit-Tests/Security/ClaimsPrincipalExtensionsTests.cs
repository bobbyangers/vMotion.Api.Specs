using FluentAssertions;
using IdentityModel;
using System;
using System.Linq;
using System.Security.Claims;
using vMotion.Dal;
using Xunit;
using Xunit.Abstractions;

namespace vMotion.Api.Specs.Unit_Tests
{
    internal static class GuidHelper
    {
        internal static Guid MakeFrom(int i)
        {
            return new Guid($"00000000-0000-0000-0000-{i:D12}");
        }
    }
    [Trait(Constants.Category, Constants.CI)]
    public class GivenClaimsPrincipalExtensionsTests : AutoFixtureTests
    {
        private Guid OperatorId { get; }

        public GivenClaimsPrincipalExtensionsTests(ITestOutputHelper output) : base(output)
        {
            OperatorId = GuidHelper.MakeFrom(99900);
        }

        private ClaimsPrincipal GetUser(PermissionValue userLevel)
        {
            var claims = new[]
            {
                new Claim(JwtClaimTypes.Id, GuidHelper.MakeFrom(1).ToString("D")),
                new Claim("perm", $"{OperatorId:D}:{PermissionKind.ManageStaffs}:{userLevel}"),
            };

            return new ClaimsPrincipal(new[] { new ClaimsIdentity(claims) });
        }

        [Theory]
        [InlineData(PermissionValue.View)]
        [InlineData(PermissionValue.Update)]
        [InlineData(PermissionValue.Insert)]
        [InlineData(PermissionValue.Delete)]
        [InlineData(PermissionValue.True)]
        public void WhenCheckingPermissionsAtLeastViewForLevelThenOk(PermissionValue level)
        {
            var user = GetUser(level);

            var result = user.HasAtLeast(new[] { OperatorId }, PermissionKind.ManageStaffs, PermissionValue.View);

            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(PermissionValue.Revoked)]
        [InlineData(PermissionValue.False)]
        public void WhenCheckingPermissionsAtLeastViewForLevelThenError(PermissionValue level)
        {
            var user = GetUser(level);

            var result = user.HasAtLeast(new[] { OperatorId }, PermissionKind.ManageStaffs, PermissionValue.View);

            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(UserTypeEnum.Agent)]
        [InlineData(UserTypeEnum.Unknown)]
        public void WhenGetUserTypeFromRolesThenDoctor(UserTypeEnum ut)
        {
            var data = new[] { new Claim(JwtClaimTypes.Role, ut.ToString()) };

            var user = new ClaimsIdentity(data);

            var result = user.Claims.Where(c => c.Type == JwtClaimTypes.Role).ToList().GetUserTypeFromRoles();

            result.Should().Be(ut);
        }

        [Fact]
        public void WhenGetOperatorsThenOpFound()
        {
            var op1 = GuidHelper.MakeFrom(99);

            var user = GetUserPrincipal(new[]
            {
                new Claim("perm", $"{op1:D}:{PermissionKind.ManageCalls}:{PermissionValue.View}"),
            });

            var result = user.GetOperators();

            ShowResult(result);

            result.Should().Contain(op1);
        }

        [Fact]
        public void GivenGetOperatorsWhenTwoOperatorsThenOpFound()
        {
            var op1 = GuidHelper.MakeFrom(98);
            var op2 = GuidHelper.MakeFrom(99);

            var user = GetUserPrincipal(new[]
            {
                new Claim("perm", $"{op1:D}:{PermissionKind.ManageCalls}:{PermissionValue.View}"),
                new Claim("perm", $"{op2:D}:{PermissionKind.ManageCalls}:{PermissionValue.View}")
            });

            var result = user.GetOperators();

            ShowResult(result);

            result.Should().Contain(op1);
            result.Should().Contain(op2);
        }

        [Fact]
        public void GivenGetOperatorPermissionsFromRolesWhenTwoOperatorsTheOk()
        {
            var op1 = GuidHelper.MakeFrom(98);
            var op2 = GuidHelper.MakeFrom(99);

            var user = GetUserPrincipal(new[]
            {
                new Claim("perm", $"{op1:D}:{PermissionKind.ManageCalls}:{PermissionValue.View}"),
                new Claim("perm", $"{op2:D}:{PermissionKind.ManageCalls}:{PermissionValue.View}")
            });

            var result = user.Claims.ToList().GetOperatorPermissionsFromRoles();

            ShowResult(result);

            result.Should().HaveCount(2);
        }

        [Fact]
        public void GivenGetPlatformPermissionsFromRolesThenOk()
        {
            var user = GetUserPrincipal(new[]
            {
                new Claim("perm", $"{Guid.Empty:D}:{PermissionKind.ManageCalls}:{PermissionValue.View}"),
            });

            var result = user.Claims.ToList().GetPlatformPermissionsFromRoles();

            ShowResult(result);

            result.Should().NotBeEmpty();
        }

        private static ClaimsPrincipal GetUserPrincipal(params Claim[] claims)
        {
            var identity = new ClaimsIdentity(claims);

            return new ClaimsPrincipal(new[] { identity });
        }
    }
}
