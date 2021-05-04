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
using EMT.DAL.Interfaces;

namespace EMT.BLL
{
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
            return $@"SELECT {selectFields} FROM {tableName} T0 {userInnerJoins} WHERE ( {notDeleted} ) ORDER BY UpdatedAt DESC;";
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
            //if (timeStamp == 0) timeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();  // Moved to DbContext

            // Establecer las propiedades de la creación de la entidad
            auditableEntity.CreatedBy = userId;
            auditableEntity.UpdatedBy = userId;
            auditableEntity.IsDeleted = false;
            //auditableEntity.CreatedAt = timeStamp;  // Moved to DbContext
            //auditableEntity.UpdatedAt = timeStamp;  // Moved to DbContext

            return timeStamp;
        }

        public static long SetFieldsForEntityUpdate(AuditableEntityBase auditableEntity, uint userId = DefaulUserId, long timeStamp = 0)
        {
            //if (timeStamp == 0) timeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); // Moved to DbContext

            auditableEntity.UpdatedBy = userId;
            //auditableEntity.UpdatedAt = timeStamp; // Moved to DbContext

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

        public static async Task<bool> ServiceUpdateEntityCheckHelper<TEntity, TDto, TIdPropertyType>(TEntity updatedEntity, 
                                        BaseResult<TDto> response, object id, string propertyName, IUnityOfWork_EF uow) where TEntity : AuditableEntityBase
        {
            // Detectar inconsistencia entre el Id y la entidad (deben ser iguales en valores)
            if (!((TIdPropertyType)id).Equals((TIdPropertyType)AppHelpers.GetPropertyValue(updatedEntity, propertyName)))
                throw new Exception(AppMessages.UPDATE_ID_ERROR);

            // Obtener la entidad desde la BD para comparar con la recibida
            var entityFromDb = await uow.GetRepository<TEntity>().GetById(id);

            // Si la entidad ya está eliminada o no existe, no hacer nada y devolver null
            if (entityFromDb == null || entityFromDb.IsDeleted)
            {
                response.Data = default(TDto);
                return false;
            }

            // Comparar con la entidad de la BD y detectar un cambio no permitido
            if (AppHelpers.NotAllowedChanges(entityFromDb, updatedEntity)) throw new Exception(AppMessages.UPDATE_FORBIDDEN_FIELDS);

            // Actualizar propiedades de la entidad
            AppHelpers.SetFieldsForEntityUpdate(updatedEntity);

            return true;
        }

        public static async Task<bool> ServiceEntityExists<TEntity>(object id, IUnityOfWork_EF uow)
                            where TEntity : AuditableEntityBase
        {
            // Try to get the entity from DB
            var entityFromDb = await uow.GetRepository<TEntity>().GetById(id);
            if (entityFromDb == null) return false;
            return true;
        }

        public static async Task<BaseResult<TDto>> ServiceGetByIdHelper<TDto>(
            IDbConnection cn, string GetById_Command, object id)
        {
            var response = new BaseResult<TDto>
            {
                Data = await cn.QuerySingleOrDefaultAsync<TDto>(GetById_Command, new { id1 = id })
            };

            #region Other samples
            //EF query samples:
            //var note = await _uow.GetRepository<MyNote>().GetById(id);
            //var allUsers = (from p in _uow.GetContext.AppUser select p).ToList();
            #endregion

            return response;
        }

        public static async Task<ListResult<TDto>> ServiceGetAllHelper<TDto>(
            IDbConnection cn, string GetAll_Command)
        {
            var response = new ListResult<TDto>
            {
                Data = await cn.QueryAsync<TDto>(GetAll_Command)
            };

            return response;
        }

        public static async Task<ListResult<TDto>> ServiceGetSearchAndPaginated<TDto>(
            IDbConnection cn, string GetSearchAndPaginated_Command, object queryParams)
        {
            var response = new ListResult<TDto>();

            // Obtener los datos
            var queryMultiple = await cn.QueryMultipleAsync(
                                    GetSearchAndPaginated_Command,
                                    queryParams);
            var totalCount = await queryMultiple.ReadSingleAsync<long>();
            var data = await queryMultiple.ReadAsync<TDto>();

            // Datos de retorno (esto llena el response.data)
            //AppHelpers.SetListResponse<TDto>(response, data, page, pageSize, totalCount);

            return response;
        }

        public static async Task<BaseResult<TDto>> ServiceCreateEntityHelper<TEntity, TDto, TIdPropertyType>(
                    TEntity newEntity, string propertyName, IUnityOfWork_EF uow,
                    string GetById_Command) where TEntity : AuditableEntityBase
        {
            var response = new BaseResult<TDto>();

            // Actualizar propiedades de la entidad
            AppHelpers.SetFieldsForEntityUpdate(newEntity);

            // Guardar la nueva entidad
            await uow.GetRepository<TEntity>().Add(newEntity);
            await uow.SaveAsync();

            // Devolver la entidad recien guardada desde la BD
            response.Data = await uow.GetConnection.QuerySingleOrDefaultAsync<TDto>(GetById_Command, 
                new { id1 = AppHelpers.GetPropertyValue(newEntity, propertyName) });

            return response;
        }

        public static async Task<BaseResult<TDto>> ServiceUpdateEntityHelper<TEntity, TDto, TIdPropertyType>(
                            TEntity updatedEntity, TIdPropertyType id, string propertyName, IUnityOfWork_EF uow, 
                            string GetById_Command) where TEntity : AuditableEntityBase
        {
            var response = new BaseResult<TDto>();

            // Detectar inconsistencia entre el Id y la entidad (deben ser iguales en valores)
            if (!((TIdPropertyType)id).Equals((TIdPropertyType)AppHelpers.GetPropertyValue(updatedEntity, propertyName)))
                throw new Exception(AppMessages.UPDATE_ID_ERROR);

            // Obtener la entidad desde la BD para comparar con la recibida
            var entityFromDb = await uow.GetRepository<TEntity>().GetById(id);

            // Si la entidad ya está eliminada o no existe, no hacer nada y devolver null
            if (entityFromDb == null || entityFromDb.IsDeleted)
            {
                response.Data = default(TDto);
                return response;
            }

            // Comparar con la entidad de la BD y detectar un cambio no permitido
            if (AppHelpers.NotAllowedChanges(entityFromDb, updatedEntity)) throw new Exception(AppMessages.UPDATE_FORBIDDEN_FIELDS);

            // Actualizar propiedades de la entidad
            AppHelpers.SetFieldsForEntityUpdate(updatedEntity);

            // Guardar la entidad actualizada
            await uow.GetRepository<TEntity>().Update(updatedEntity);
            await uow.SaveAsync();
            
            // Devolver la entidad recien guardada desde la BD
            response.Data = await uow.GetConnection.QuerySingleOrDefaultAsync<TDto>(GetById_Command, new { id1 = id });

            return response;
        }

        public static async Task<BaseResult<object>> ServiceDeleteEntityHelper<TEntity>(
                                object id, IUnityOfWork_EF uow) where TEntity : AuditableEntityBase
        {
            var response = new BaseResult<object>();

            // Obtener la entidad desde la BD para eliminarla (lógicamente)
            var entityFromDb = await uow.GetRepository<TEntity>().GetById(id);

            // Si la entidad ya está eliminada o no existe, no hacer nada y devolver null
            if (entityFromDb == null || entityFromDb.IsDeleted)
            {
                response.Data = null;
                return response;
            }

            // Actualizar propiedades de la entidad
            AppHelpers.SetFieldsForEntityDeletion(entityFromDb);

            // Guardar la entidad eliminada lógicamente
            await uow.GetRepository<TEntity>().Update(entityFromDb);
            await uow.SaveAsync();

            // Solo devolver el ID de la entidad eliminada lógicamente
            response.Data = id;

            return response;
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
    }
}
