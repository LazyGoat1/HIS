using System.Net.Http.Json;
using FluentAssertions;
using HIS.Models;
using Xunit;

namespace HIS.Tests
{
    public class DoctorTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _f;
        public DoctorTests(TestFixture f) => _f = f;

        [Fact] public async Task GetList_ReturnsData()
        {
            await _f.LoginAsAdminAsync();
            var r = await _f.Client.GetFromJsonAsync<LayuiTableResult>("/Doctor/GetList?pageIndex=1&pageSize=10");
            r!.Code.Should().Be(0);
            r.Total.Should().BeGreaterThanOrEqualTo(8);
        }

        [Fact] public async Task Create_AutoGeneratesAccount()
        {
            await _f.LoginAsAdminAsync();
            var r = await _f.PostAsync<ApiResult>("/Doctor/Create",
                new { Name = "测试医生", Gender = 1, DepartmentId = 1, Title = 3, Specialty = "测试", MaxDailyPatients = 40, ConsultationFee = 15 });
            r!.Code.Should().Be(0);
            r.Msg.Should().Contain("登录账号");
        }
    }
}
