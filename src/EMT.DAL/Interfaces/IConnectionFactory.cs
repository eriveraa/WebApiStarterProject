using System.Data;

namespace EMT.DAL.Interfaces
{
    public interface IConnectionFactory
    {
        string ConnectionString { get; }
        IDbConnection GetConnection { get; }
        void CloseConnection();
    }
}
