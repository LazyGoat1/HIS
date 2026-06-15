using FluentAssertions;
using HIS.Models;
using Xunit;

namespace HIS.Tests
{
    public class InpatientTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _f;
        public InpatientTests(TestFixture f) => _f = f;

        [Fact] public async Task Admit_Works()
        {
            await _f.LoginAsAdminAsync();
            var r = await _f.PostAsync<ApiResult>("/InpatientRecord/Create",
                new { PatientId = 1, DepartmentId = 1, AdmissionTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), AdmissionDiagnosis = "肺炎", DepositAmount = 2000 });
            r!.Code.Should().Be(0);
        }
    }
}
