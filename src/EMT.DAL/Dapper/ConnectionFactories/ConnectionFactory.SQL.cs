using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using EMT.DAL.Interfaces;

namespace EMT.DAL.Dapper.ConnectionFactories
{
    public class SQLConnectionFactory : IConnectionFactory
    {
        private IDbConnection _connection;
        public readonly string _connectionString;

        public SQLConnectionFactory(string connectionString)
        {
            Debug.WriteLine($"* Constructor of {this.GetType()} at {DateTime.Now}");

            _connectionString = connectionString;
        }

        public string ConnectionString => _connectionString;

        public IDbConnection GetConnection
        {
            get
            {
                if (_connection == null)
                {
                    _connection = new SqlConnection(_connectionString);
                    _connection.Open();
                }
                else if (_connection.State != ConnectionState.Open)
                {
                    _connection.ConnectionString = _connectionString;
                    _connection.Open();
                }
                return _connection;
            }
        }

        public void CloseConnection()
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }
        }
    }
}
