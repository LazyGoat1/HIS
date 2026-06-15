using FluentAssertions;
using HIS.Models;
using Xunit;

namespace HIS.Tests
{
    public class PrescriptionTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _f;
        public PrescriptionTests(TestFixture f) => _f = f;

        [Fact] public async Task Create_InsufficientStock_Fails()
        {
            await _f.LoginAsDoctorAsync();
            var r = await _f.PostAsync<ApiResult>("/Prescription/Create",
                new { OutpatientRecordId = 1, PatientId = 1, PrescriptionType = 1,
                    Details = new[] { new { ItemType = 1, ItemId = 1, ItemName = "阿莫西林", Quantity = 99999, UnitPrice = 12, Amount = 99999 } } });
            r!.Code.Should().Be(1);
            r.Msg.Should().Contain("库存不足");
        }
    }
}
