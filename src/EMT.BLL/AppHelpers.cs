using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dapper;
using EMT.Common.Entities;
using EMT.Common.ResponseWrappers;

namespace EMT.BLL
{
    public enum IdGenerationModeEnum
    {
        GENAPP_GUIDSTRING,
        GENAPP_GUID,
        GENUSER,
        GENDB_IDENTITY
    }

    public class AppHelpers
    {
        public const uint DefaulUserId = 1;
        public const string QueryPart_SelectFields = @"T0.*, U1.FN AS 'CreatedBy_FN', U2.FN AS 'UpdatedBy_FN'";
        public const string QueryPart_UserInnerJoins = @"INNER JOIN AppUser U1 ON U1.AppUserId = T0.CreatedBy  
                                                         INNER JOIN AppUser U2 ON U2.AppUserId = T0.UpdatedBy";
        public const string QueryPart_NotDeleted = @"T0.IsDeleted = 0";

        public static string QueryGetById_Command(string tableName, string selectFields, string idPropertyName, string userInnerJoins, string notDeleted)
        {
            return $@"SELECT {selectFields} FROM {tableName} T0 {userInnerJoins} WHERE ( T0.{idPropertyName} = @id1 AND {notDeleted} );";
        }

        public static string QueryGetAll_Command(string tableName, string selectFields, string idPropertyName, string userInnerJoins, string notDeleted)
        {
            return $@"SELECT {selectFields} FROM {tableName} T0 {userInnerJoins} WHERE ( {notDeleted} ) ORDER BY UpdatedAt;";
        }

        public static string QueryGetExists_Command(string tableName, string idPropertyName)
        {
            return $@"SELECT T0.{idPropertyName} FROM {tableName} T0 WHERE ( T0.{idPropertyName} = @id1 );";
        }

        public static void SetListResponse<T>(ListResult<T> response, IEnumerable<T> data,
                                            int page, int pageSize, long totalRecordCount)
        {
            response.Data = data;
            response.Page = page;
            response.PageSize = pageSize;
            response.TotalRecordCount = totalRecordCount;
            response.TotalPages = (int)Math.Ceiling((double)totalRecordCount / pageSize);
        }

        public static long SetFieldsForEntityCreation<T>(T auditableEntity, uint userId = DefaulUserId, long timeStamp = 0) where T : AuditableEntityBase
        {
            if (timeStamp == 0) timeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Establecer las propiedades de la creación de la entidad
            auditableEntity.CreatedBy = userId;
            auditableEntity.UpdatedBy = userId;
            auditableEntity.IsDeleted = false;
            auditableEntity.CreatedAt = timeStamp;
            auditableEntity.UpdatedAt = timeStamp;

            return timeStamp;
        }

        public static long SetFieldsForEntityUpdate(AuditableEntityBase auditableEntity, uint userId = DefaulUserId, long timeStamp = 0)
        {
            if (timeStamp == 0) timeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            auditableEntity.UpdatedBy = userId;
            auditableEntity.UpdatedAt = timeStamp;

            return timeStamp;
        }

        public static long SetFieldsForEntityDeletion(AuditableEntityBase auditableEntity, uint userId = DefaulUserId, long timeStamp = 0)
        {
            auditableEntity.IsDeleted = true;            
            return SetFieldsForEntityUpdate(auditableEntity, userId, timeStamp);
        }

        public static bool NotAllowedChanges(AuditableEntityBase originalEntity, AuditableEntityBase updatedEntity)
        {
            // Ninguno de estos cambios puede ser cambiado en el cliente (UI, Móvil, etc.)
            // Solo pueden ser cambiados por la lógica de negocio explícitamente.
            if (   (originalEntity.CreatedBy != updatedEntity.CreatedBy)
                || (originalEntity.UpdatedBy != updatedEntity.UpdatedBy)
                || (originalEntity.CreatedAt != updatedEntity.CreatedAt)
                || (originalEntity.UpdatedAt != updatedEntity.UpdatedAt)
                || (originalEntity.IsDeleted != updatedEntity.IsDeleted)
            )
                return true;

            return false;
        }

        public static object GetPropertyValue(object src, string propName) 
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        public static void SetPropertyValue(object src, string propName, object newValue) 
        {
            src.GetType().GetProperty(propName).SetValue(src, newValue);
        }

        public static string SanitizeString(string rawData, bool toUpper = false, bool nullAsEmpty = false)
        {
            string ret = null;
            if (rawData != null)
            {
                Regex regex = new Regex(@"[,?¿{}<>=!:.;/\\""]+"); // Aquí se definen los caracteres NO-válidos
                // Remover caracteres inválidos
                ret = regex.Replace(rawData, "").Replace("-", "");
                // Remover espacios adicionales
                ret = String.Join(" ", ret.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
                //ret = ret.Replace("-", "").Replace("\"", "").Replace("?", "").Replace("¿", "");
                if (toUpper) ret = ret.ToUpper();
            }
            else
            {
                if (nullAsEmpty) ret = String.Empty;
            }

            return ret;
        }

        public static long GetEllapsedMsAndStop(Stopwatch sw)
        {
            sw.Stop();
            return sw.ElapsedMilliseconds;
        }

        public static string GenerateHashMd5(string dataToHash)
        {
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                UnicodeEncoding parser = new UnicodeEncoding();
                byte[] originalBytes = parser.GetBytes(dataToHash);
                byte[] encryptedBytes = md5.ComputeHash(originalBytes);
                return Convert.ToBase64String(encryptedBytes);
            }
        }

        //public static string GetDefaultAppUserId()
        //{
        //    return ConfigurationManager.AppSettings["DefaultAppUserId"];
        //}

        //public static string GetUploadFolder()
        //{
        //    return ConfigurationManager.AppSettings["UploadFolder"];
        //}

        //public static string GetTemporalFolder()
        //{
        //    //return HttpContext.Current.Server.MapPath("~/temporalfiles/");
        //    return ConfigurationManager.AppSettings["TempFolder"];
        //}

        //public static int GetDelay(IOptionsSnapshot<MyAppConfig> myAppConfig)
        //{
        //    return int.Parse(ConfigurationManager.AppSettings["RuntimeThreadAffinity"]);
        //}

        public static async Task<BaseResult<T>> GetObject<T>(IDbConnection cn,
                                                             string sqlCommand,
                                                             object commandParameters) where T : class
        {
            BaseResult<T> ret = new BaseResult<T>(true);  // Objeto para retornar datos

            // Obtener la data
            T retObject = await cn.QuerySingleOrDefaultAsync<T>(sqlCommand, commandParameters);
            
            // Datos de retorno
            ret.Data = retObject;

            // Devolver los datos
            return ret;
        }

        public static async Task<ListResult<T>> GetList<T>(IDbConnection cn,
                                                               string sqlCommand,
                                                               object commandParameters) where T : class
        {
            ListResult<T> ret = new ListResult<T>(true);  // Objeto para retornar datos

            // Obtener la data
            IEnumerable<T> dataList = await cn.QueryAsync<T>(sqlCommand, commandParameters);

            // Datos de retorno
            ret.Data = dataList;
            //ret.PageRecordCount = dataList.Count();
            ret.Page = 1;
            ret.PageSize = dataList.Count();
            ret.TotalRecordCount = ret.PageSize;
            ret.TotalPages = 1;

            // Devolver los datos
            return ret;
        }

        public static async Task<ListResult<T>> GetSPListData<T>(IDbConnection cn,
                                                                     string sqlCommand,
                                                                     object commandParameters = null) where T : class
        {
            ListResult<T> ret = new ListResult<T>(true);  // Objeto para retornar datos

            // Obtener la data
            IEnumerable<T> dataList = await cn.QueryAsync<T>(sqlCommand, commandParameters, commandType: System.Data.CommandType.StoredProcedure);

            // Datos de retorno
            ret.Data = dataList;
            //ret.PageRecordCount = dataList.Count();
            ret.Page = 1;
            ret.PageSize = dataList.Count();
            ret.TotalRecordCount = ret.PageSize;
            ret.TotalPages = 1;

            // Insertar un pequeño delay (para pruebas)
            //await Task.Delay(myAppConfig.Value.TransactionMs);
            //System.Threading.Thread.Sleep(EMT.BLL.Utils.GetDelay());

            // Devolver los datos
            return ret;
        }

    }
}
