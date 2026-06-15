using FluentAssertions;
using HIS.Models;
using Xunit;

namespace HIS.Tests
{
    public class RegistrationTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _f;
        public RegistrationTests(TestFixture f) => _f = f;

        [Fact] public async Task Create_AutoGeneratesCharge()
        {
            await _f.LoginAsAdminAsync();
            var r = await _f.PostAsync<ApiResult>("/Registration/Create",
                new { PatientId = 1, DepartmentId = 1, DoctorId = 1, RegistrationType = 1, VisitDate = System.DateTime.Today.ToString("yyyy-MM-dd") });
            r!.Code.Should().Be(0);
            r.Msg.Should().Contain("挂号单号");
        }
    }
}
