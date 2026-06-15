using FluentAssertions;
using HIS.Models;
using Xunit;

namespace HIS.Tests
{
    public class ScheduleTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _f;
        public ScheduleTests(TestFixture f) => _f = f;

        [Fact] public async Task DuplicatePrevention()
        {
            await _f.LoginAsAdminAsync();
            var s = new { DoctorId = 2, DepartmentId = 1, DayOfWeek = 1, TimeSlot = "全天", MaxPatients = 30, Status = 1 };
            await _f.PostAsync<ApiResult>("/Schedule/Create", s);
            var r = await _f.PostAsync<ApiResult>("/Schedule/Create", s);
            r!.Code.Should().Be(1);
        }
    }
}
