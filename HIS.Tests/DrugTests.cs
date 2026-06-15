using System.Net.Http.Json;
using FluentAssertions;
using HIS.Models;
using Xunit;

namespace HIS.Tests
{
    public class DrugTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _f;
        public DrugTests(TestFixture f) => _f = f;

        [Fact] public async Task GetList_ReturnsData()
        {
            await _f.LoginAsAdminAsync();
            var r = await _f.Client.GetFromJsonAsync<LayuiTableResult>("/Drug/GetList?pageIndex=1&pageSize=50");
            r!.Code.Should().Be(0);
            r.Total.Should().BeGreaterThanOrEqualTo(10);
        }

        [Fact] public async Task Export_ReturnsExcelFile()
        {
            await _f.LoginAsAdminAsync();
            var r = await _f.Client.GetAsync("/Drug/Export");
            r.IsSuccessStatusCode.Should().BeTrue();
        }
    }
}
