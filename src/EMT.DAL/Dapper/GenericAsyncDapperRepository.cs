using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using EMT.DAL.Interfaces;

namespace EMT.DAL.Dapper
{
    public abstract class GenericAsyncDapperRepository<T> : IGenericAsyncRepository<T> where T : class
    {
        private readonly IConnectionFactory _connectionFactory;

        // Comandos que serán cambiados en los repositorios concretos que heredan de esta clase (en el constructor)
        // Si cambian los nombres aquí, se podrá aplicar el renombrado a las clases derivadas.
        protected string GetAll_Command;
        protected string GetById_Command;
        protected string Insert_Command;
        protected string Update_Command;
        protected string DeleteById_Command;

        /// <summary>
        /// Repositorio genérico del cual deben heredar todos los repositorios de las tablas
        /// </summary>
        /// <param name="connectionFactory">connectionFactory se debe inyectar por DI</param>
        protected GenericAsyncDapperRepository(IConnectionFactory connectionFactory)
        {
            Debug.WriteLine($"* Constructor of {this.GetType()} at {DateTime.Now}");
            _connectionFactory = connectionFactory;
        }

        public IDbConnection GetConnection => _connectionFactory.GetConnection;

        public async Task<IEnumerable<T>> GetAll()
        {
            return await DapperUtils.GetObjectListAsync<T>(GetConnection, GetAll_Command);
        }

        public async Task<T> GetById(object id)
        {
            return await DapperUtils.GetObjectAsync<T>(GetConnection, GetById_Command, id);
        }

        public async Task Add(T entity)
        {
            await DapperUtils.ExecuteAsync(GetConnection, Insert_Command, entity);
            return;
        }

        public async Task Update(T entity)
        {
            await DapperUtils.ExecuteAsync(GetConnection, Update_Command, entity);
            return;
        }

        public async Task DeleteById(object id)
        {
            await DapperUtils.ExecuteAsync(GetConnection, DeleteById_Command, id);
            return;
        }
    }
}
