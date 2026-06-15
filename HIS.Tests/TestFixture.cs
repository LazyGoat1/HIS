using System.Net.Http.Json;
using HIS.Repository.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HIS.Tests
{
    public class TestFixture : IDisposable
    {
        public HttpClient Client { get; }

        private class CustomWebAppFactory : WebApplicationFactory<HIS.Web.Program>
        {
            private readonly string _dbName = "HIS_Test_" + Guid.NewGuid().ToString("N")[..8];

            protected override void ConfigureWebHost(IWebHostBuilder builder)
            {
                builder.UseEnvironment("Development");
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<HisDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(HisDbContext));
                    if (descriptor != null) services.Remove(descriptor);

                    services.AddDbContext<HisDbContext>(options =>
                        options.UseInMemoryDatabase(_dbName));

                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<HisDbContext>();
                    db.Database.EnsureCreated();
                    DbInitializer.Seed(db);
                });
            }
        }

        private readonly CustomWebAppFactory _factory;

        public TestFixture()
        {
            _factory = new CustomWebAppFactory();
            Client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            { AllowAutoRedirect = false });
        }

        public async Task LoginAsAdminAsync()
        {
            var r = await Client.PostAsJsonAsync("/Account/Login",
                new { UserName = "admin", Password = "123456", RememberMe = false });
            r.EnsureSuccessStatusCode();
        }

        public async Task LoginAsDoctorAsync()
        {
            var r = await Client.PostAsJsonAsync("/Account/Login",
                new { UserName = "doctor01", Password = "123456", RememberMe = false });
            r.EnsureSuccessStatusCode();
        }

        public async Task<T?> PostAsync<T>(string url, object body)
        {
            var r = await Client.PostAsJsonAsync(url, body);
            r.EnsureSuccessStatusCode();
            return await r.Content.ReadFromJsonAsync<T>();
        }

        public void Dispose() => _factory.Dispose();
    }
}
