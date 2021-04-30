using System;
using EMT.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EMT.API.ApiUtils
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        private readonly IOptionsSnapshot<MyAppConfig> _myAppConfig;
        private readonly ILogger _logger;

        public BaseApiController(IOptionsSnapshot<MyAppConfig> myAppConfig,
                                 ILogger logger)
        {
            this._myAppConfig = myAppConfig;
            this._logger = logger;
            logger.LogInformation($"* CONTROLLER Constructor of {this.GetType()} at {DateTime.Now}");
        }
    }
}
