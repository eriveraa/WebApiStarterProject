using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace EMT.DAL.Dapper
{
    /// <summary>
    /// Versión asíncrona y sin dependencias con un provider de base de datos específico.
    /// Se puede utilizar con cualquier base de datos.
    /// </summary>
    public class DapperUtils
    {
        /* 

         * USAR ESTO EN LA CONFIGURACIÓN (App.config or Web.config)
        <configuration>	
           <system.data>
             <DbProviderFactories>
               <remove invariant="System.Data.SQLite" />
               <add name="SQLite Data Provider" invariant="System.Data.SQLite" description=".Net Framework Data Provider for SQLite" type="System.Data.SQLite.SQLiteFactory, System.Data.SQLite" />
             </DbProviderFactories>
           </system.data>

           <connectionStrings>
             <add name="sqlitedb" connectionString="Data Source=D:\ContactManager.db" />
           </connectionStrings>			
        </configuration>

        * OBTENER UNA connectionString desde configuración
        connectionString = ConfigurationManager.ConnectionStrings["sqlitedb"].ConnectionString;
        return new SQLiteConnection(connectionString);

        */

        //public static void CompactDB(string connectionString = null)
        //{
        //    Execute("VACUUM;", null, connectionString);
        //}

        public static async Task<int> ExecuteAsync(IDbConnection cn, string sqlCommand, object paramObject = null)
        {
            int recordsAfected = 0;
            recordsAfected = await cn.ExecuteAsync(sqlCommand, paramObject);
            return recordsAfected;
        }

        public static async Task<IEnumerable<T>> GetObjectListAsync<T>(IDbConnection cn, string sqlCommand, object paramObject = null)
        {
            IEnumerable<T> retData;
            retData = await cn.QueryAsync<T>(sqlCommand, paramObject);
            return retData;
        }

        public static async Task<T> GetObjectAsync<T>(IDbConnection cn, string sqlCommand, object paramObject = null)
        {
            T retData;
            retData = await cn.QuerySingleOrDefaultAsync<T>(sqlCommand, paramObject);
            return retData;
        }

    }
}
