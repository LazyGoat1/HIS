using System.Net.Http.Json;
using FluentAssertions;
using HIS.Models;
using Xunit;

namespace HIS.Tests
{
    public class AuthTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _f;
        public AuthTests(TestFixture f) => _f = f;

        [Fact] public async Task Login_Valid_ReturnsSuccess()
        {
            var r = await _f.Client.PostAsJsonAsync("/Account/Login",
                new { UserName = "admin", Password = "123456", RememberMe = false });
            r.IsSuccessStatusCode.Should().BeTrue();
        }

        [Fact] public async Task Login_Invalid_ReturnsFail()
        {
            var r = await _f.Client.PostAsJsonAsync("/Account/Login",
                new { UserName = "admin", Password = "wrong", RememberMe = false });
            var b = await r.Content.ReadFromJsonAsync<ApiResult>();
            b!.Code.Should().Be(1);
        }

        [Fact] public async Task Unauthorized_Redirects()
        {
            var r = await _f.Client.GetAsync("/Patient/Index");
            r.StatusCode.Should().Be(System.Net.HttpStatusCode.Redirect);
        }

        [Fact] public async Task Profile_Update_Works()
        {
            await _f.LoginAsAdminAsync();
            var r = await _f.PostAsync<ApiResult>("/Account/Profile",
                new { RealName = "管理员", Phone = "13900000000" });
            r!.Code.Should().Be(0);
        }

        [Fact] public async Task ChangePassword_Works()
        {
            await _f.LoginAsAdminAsync();
            var r = await _f.PostAsync<ApiResult>("/Account/ChangePassword",
                new { OldPassword = "123456", NewPassword = "123456", ConfirmPassword = "123456" });
            r!.Code.Should().Be(0);
        }

        [Fact] public async Task GetMenus_ReturnsData()
        {
            await _f.LoginAsAdminAsync();
            var r = await _f.Client.GetFromJsonAsync<ApiResult>("/Account/GetMenus");
            r!.Code.Should().Be(0);
        }
    }
}
