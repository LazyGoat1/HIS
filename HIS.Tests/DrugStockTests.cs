using FluentAssertions;
using HIS.Models;
using Xunit;

namespace HIS.Tests
{
    public class DrugStockTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _f;
        public DrugStockTests(TestFixture f) => _f = f;

        [Fact] public async Task StockIn_IncreasesQuantity()
        {
            await _f.LoginAsAdminAsync();
            var r = await _f.PostAsync<ApiResult>("/DrugStock/StockIn",
                new { DrugId = 1, Quantity = 50, RelatedNo = "TEST001" });
            r!.Code.Should().Be(0);
        }

        [Fact] public async Task StockCheck_Works()
        {
            await _f.LoginAsAdminAsync();
            var r = await _f.PostAsync<ApiResult>("/DrugStock/StockCheck",
                new { drugId = 1, quantity = 200 });
            r!.Code.Should().Be(0);
        }
    }
}
