using System.Net.Http.Json;
using FluentAssertions;
using HIS.Models;
using Xunit;

namespace HIS.Tests
{
    public class PatientTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _f;
        public PatientTests(TestFixture f) => _f = f;

        [Fact] public async Task GetList_ReturnsData()
        {
            await _f.LoginAsAdminAsync();
            var r = await _f.Client.GetFromJsonAsync<LayuiTableResult>("/Patient/GetList?pageIndex=1&pageSize=10");
            r!.Code.Should().Be(0);
            r.Total.Should().BeGreaterThan(0);
        }

        [Fact] public async Task Create_Success()
        {
            await _f.LoginAsAdminAsync();
            var r = await _f.PostAsync<ApiResult>("/Patient/Create",
                new { Name = "测试患者", Gender = 1, Phone = "13800138000" });
            r!.Code.Should().Be(0);
        }
    }
}
