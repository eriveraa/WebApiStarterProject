using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using EMT.Common;
using EMT.DAL.Interfaces;

namespace EMT.BLL.Services
{
    public abstract class BaseService
    {
        protected readonly IOptionsSnapshot<MyAppConfig> _myAppConfig;
        protected readonly ILogger _logger;
        protected readonly IUnityOfWork_EF _uow;
        protected readonly IDbConnection _connection;

        public BaseService(IOptionsSnapshot<MyAppConfig> myAppConfig, ILogger logger, IUnityOfWork_EF uow)
        {
            this._myAppConfig = myAppConfig;
            this._logger = logger;
            this._uow = uow;
            this._connection = _uow.GetConnection;
            //logger.LogInformation($"* SERVICE Constructor of {this.GetType()} at {DateTime.Now}");
        }
    }
}
