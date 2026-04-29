using JfYu.WebApi.Template.Constants;
using JfYu.WebApi.Template.Controllers;
using JfYu.WebApi.Template.Entity;
using JfYu.WebApi.Template.Model;
using JfYu.WebApi.Template.Model.LoginLog;
using JfYu.WebApi.Template.Services.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace JfYu.WebApi.Template.UnitTests.Controllers
{
    public class LoginLogControllerTests
    {
        [Fact]
        public async Task GetAllAsync_ReturnsOk()
        {
            TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
            TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(true);

            var svcMock = new Mock<ILoginLogService>();
            svcMock.Setup(s => s.GetPagedAsync(It.IsAny<QueryLoginLogRequest>()))
                .ReturnsAsync(new JfYu.Data.Model.PagedData<LoginLog>
                {
                    Data = [new LoginLog { UserName = "u" }],
                    TotalCount = 1,
                });

            var ctrl = new LoginLogController(svcMock.Object);

            var result = await ctrl.GetAllAsync(new QueryLoginLogRequest());

            var ok = result.Should().BeOfType<OkObjectResult>().Subject.Value as BaseResponse<object>;
            ok!.Code.Should().Be(ResponseCode.Success);
        }
    }
}
