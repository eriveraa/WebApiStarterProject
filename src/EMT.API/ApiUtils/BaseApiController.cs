using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using EMT.Common;

namespace EMT.API.ApiUtils
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        protected readonly IOptionsSnapshot<MyAppConfig> _myAppConfig;
        protected readonly ILogger _logger;

        public BaseApiController(IOptionsSnapshot<MyAppConfig> myAppConfig,
                                 ILogger logger)
        {
            this._myAppConfig = myAppConfig;
            this._logger = logger;
            _logger.LogDebug($"*** CONTROLLER Constructor of {this.GetType()} at {DateTime.Now}");
        }
    }
}
