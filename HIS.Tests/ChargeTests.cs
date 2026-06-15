using FluentAssertions;
using HIS.Models;
using Xunit;

namespace HIS.Tests
{
    public class ChargeTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _f;
        public ChargeTests(TestFixture f) => _f = f;

        [Fact] public async Task Create_Success()
        {
            await _f.LoginAsAdminAsync();
            var r = await _f.PostAsync<ApiResult>("/Charge/Create",
                new { PatientId = 1, ChargeType = 2, TotalAmount = 100, PaidAmount = 100, PaymentMethod = 1 });
            r!.Code.Should().Be(0);
        }
    }
}
