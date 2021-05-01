using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Dapper;
using EMT.Common;
using EMT.Common.Entities;
using EMT.Common.ResponseWrappers;
using EMT.DAL.Interfaces;
using System.Linq;

namespace EMT.BLL.Services
{
    /// <summary>
    /// Service class for business logic, interacting with data, etc.
    /// </summary>
    public class MyNoteService : BaseService, IMyNoteService
    {
        private const string _tableName = "MyNote";
        private const string _idPropertyName = "NoteId";
        private const string _selectFields = AppHelpers.QueryPart_SelectFields;
        private const string _userInnerJoins = AppHelpers.QueryPart_UserInnerJoins;
        private const string _notDeleted = AppHelpers.QueryPart_NotDeleted;

        public MyNoteService(IOptionsSnapshot<MyAppConfig> myAppConfig, ILogger<MyNoteService> logger, IUnityOfWork_EF uow) 
            : base(myAppConfig, logger, uow)
        {
        }

        private string GetById_Command => AppHelpers.QueryGetById_Command(_tableName, _selectFields, _idPropertyName,_userInnerJoins, _notDeleted);
        private string GetAll_Command => AppHelpers.QueryGetAll_Command(_tableName, _selectFields, _idPropertyName, _userInnerJoins, _notDeleted);
        //private string GetExists_Command => AppHelpers.QueryGetExists_Command(_tableName, _idPropertyName);

        public async Task<BaseResult<MyNoteDto>> GetById(object id)
        {
            var response = new BaseResult<MyNoteDto>();

            response.Data = await _connection.QuerySingleOrDefaultAsync<MyNoteDto>(GetById_Command, new { id1 = id });

            //EF query samples:
            //var note = await _uow.GetRepository<MyNote>().GetById(id);
            //var allUsers = (from p in _uow.GetContext.AppUser select p).ToList();

            return response;
        }

        public async Task<ListResult<MyNoteDto>> GetAll()
        {
            var response = new ListResult<MyNoteDto>();

            response.Data = await _connection.QueryAsync<MyNoteDto>(GetAll_Command, null);

            return response;
        }

        public async Task<ListResult<MyNoteDto>> GetSearchAndPaginated(string search = null, int page = 1, int pageSize = 5)
        {
            var response = new ListResult<MyNoteDto>();

            search = $"%{search}%";
            string sqlWHERE = $@"( {_notDeleted} AND ( T0.Title LIKE @search OR T0.NoteBody LIKE @search ) )";
            string sqlPaged = $@"SELECT COUNT(*) FROM {_tableName} T0 WHERE {sqlWHERE};

                                     SELECT {_selectFields} FROM {_tableName} T0 {_userInnerJoins} WHERE {sqlWHERE} 
                                     ORDER BY T0.UpdatedAt DESC LIMIT @pageSize OFFSET @offset;";

            // Obtener los datos
            var queryMultiple = await _connection.QueryMultipleAsync(
                                    sqlPaged,
                                    new { search, pageSize, offset = pageSize * (page - 1) });
            var totalCount = await queryMultiple.ReadSingleAsync<long>();
            var data = await queryMultiple.ReadAsync<MyNoteDto>();

            // Datos de retorno (esto llena el response.data)
            AppHelpers.SetListResponse<MyNoteDto>(response, data, page, pageSize, totalCount);

            return response;
        }


        public async Task<BaseResult<MyNoteDto>> Create(MyNote newEntity)
        {
            var response = new BaseResult<MyNoteDto>();

            // Set initial properties
            AppHelpers.SetFieldsForEntityCreation(newEntity);

            // Guardar la entidad
            await _uow.GetRepository<MyNote>().Add(newEntity);
            await _uow.SaveAsync();

            // Devolver la entidad recien guardada desde la BD
            response.Data = await _connection.QuerySingleOrDefaultAsync<MyNoteDto>(GetById_Command, new { id1 = newEntity.NoteId });

            return response;
        }

        public async Task<BaseResult<MyNoteDto>> Update(object id, MyNote updatedEntity)
        {
            var response = new BaseResult<MyNoteDto>();

            // Detectar inconsistencia entre el Id y la entidad (deben ser iguales en valores)
            if ((uint)id != updatedEntity.NoteId) throw new Exception(AppMessages.UPDATE_ID_ERROR);

            // Obtener la entidad desde la BD para actualizarla
            var entityFromDb = await _uow.GetRepository<MyNote>().GetById(id);

            // Si la entidad ya está eliminada o no existe, no hacer nada y devolver null
            if (entityFromDb == null || entityFromDb.IsDeleted)
            {
                response.Data = null;
                return response;
            }

            // Comparar con la entidad de la BD y detectar un cambio no permitido
            if (AppHelpers.NotAllowedChanges(entityFromDb, updatedEntity)) throw new Exception(AppMessages.UPDATE_FORBIDDEN_FIELDS);

            // Actualizar propiedades de la entidad
            AppHelpers.SetFieldsForEntityUpdate(updatedEntity);

            // Guardar la entidad
            await _uow.GetRepository<MyNote>().Update(updatedEntity);
            await _uow.SaveAsync();

            // Devolver la entidad recien guardada desde la BD
            response.Data = await _connection.QuerySingleOrDefaultAsync<MyNoteDto>(GetById_Command, new { id1 = id });

            return response;
        }

        public async Task<BaseResult<MyNoteDto>> Update_OLD(object id, MyNote updatedEntity)
        {
            var response = new BaseResult<MyNoteDto>();

            try
            {
                // Business logic

                // Detectar inconsistencia entre el Id y la entidad (deben ser iguales en valores)
                if ((uint)id != updatedEntity.NoteId) throw new Exception(AppMessages.UPDATE_ID_ERROR);

                // Obtener la entidad desde la BD para actualizarla
                var entityFromDb = await _uow.GetRepository<MyNote>().GetById(id);

                // Si la entidad ya está eliminada o no existe, no hacer nada y devolver null
                if (entityFromDb == null || entityFromDb.IsDeleted)
                {
                    response.Data = null;
                    return response;
                }

                // Comparar con la entidad de la BD y detectar un cambio no permitido
                if (AppHelpers.NotAllowedChanges(entityFromDb, updatedEntity)) throw new Exception(AppMessages.UPDATE_FORBIDDEN_FIELDS);

                // Actualizar propiedades de la entidad
                AppHelpers.SetFieldsForEntityUpdate(updatedEntity);

                // Guardar la entidad
                await _uow.GetRepository<MyNote>().Update(updatedEntity);
                await _uow.SaveAsync();

                // Devolver la entidad recien guardada desde la BD
                response.Data = await _connection.QuerySingleOrDefaultAsync<MyNoteDto>(GetById_Command, new { id1 = id });
            }
            catch (Exception)
            {
                //response.Success = false;
                //response.ExceptionMessage = ex.ToString();
            }
            return response;
        }

        public async Task<BaseResult<object>> DeleteById(object id)
        {
            var response = new BaseResult<object>();

            // Obtener la entidad desde la BD para actualizarla
            var entityFromDb = await _uow.GetRepository<MyNote>().GetById(id);

            // Si la entidad ya está eliminada o no existe, no hacer nada y devolver null
            if (entityFromDb == null || entityFromDb.IsDeleted)
            {
                response.Data = null;
                return response;
            }

            // Actualizar propiedades de la entidad
            AppHelpers.SetFieldsForEntityDeletion(entityFromDb);

            // Guardar la entidad
            await _uow.GetRepository<MyNote>().Update(entityFromDb);
            await _uow.SaveAsync();

            // Solo devolver el ID de la entidad eliminada lógicamente
            response.Data = id;

            return response;
        }

    }
}
