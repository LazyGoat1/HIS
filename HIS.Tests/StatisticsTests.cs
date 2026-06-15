using System.Net.Http.Json;
using FluentAssertions;
using HIS.Models;
using Xunit;

namespace HIS.Tests
{
    public class StatisticsTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _f;
        public StatisticsTests(TestFixture f) => _f = f;

        [Fact] public async Task Summary_Works()
        {
            await _f.LoginAsAdminAsync();
            var r = await _f.Client.GetFromJsonAsync<ApiResult>("/Statistics/Summary");
            r!.Code.Should().Be(0);
        }

        [Fact] public async Task DailySettlement_Works()
        {
            await _f.LoginAsAdminAsync();
            var r = await _f.Client.GetFromJsonAsync<ApiResult>("/Statistics/DailySettlement");
            r!.Code.Should().Be(0);
        }
    }
}
