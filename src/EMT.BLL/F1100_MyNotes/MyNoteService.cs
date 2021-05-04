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
    /// Use this service class as a template for new Services.
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

        public async Task<BaseResult<MyNoteDto>> GetById(uint id)
        {
            _logger.LogInformation("*** {Method} {id}", "GetById", id);
            return await AppHelpers.ServiceGetByIdHelper<MyNoteDto>(_connection, GetById_Command, id);
        }

        public async Task<ListResult<MyNoteDto>> GetAll()
        {
            _logger.LogInformation("*** {Method}", "GetAll");
            return await AppHelpers.ServiceGetAllHelper<MyNoteDto>(_connection, GetAll_Command);
        }

        public async Task<ListResult<MyNoteDto>> GetSearchAndPaginated(string search = null, int page = 1, int pageSize = 5)
        {
            _logger.LogInformation("*** {Method}", "GetSearchAndPaginated");

            search = $"%{search}%";
            string sqlWHERE = $@"( {_notDeleted} AND ( T0.Title LIKE @search OR T0.NoteBody LIKE @search ) )";
            string sqlPaged = $@"SELECT COUNT(*) FROM {_tableName} T0 WHERE {sqlWHERE};

                                SELECT {_selectFields} FROM {_tableName} T0 {_userInnerJoins} WHERE {sqlWHERE} 
                                ORDER BY T0.CreatedAt ASC LIMIT @pageSize OFFSET @offset;";

            // Add here all the parameters you need (besides @pageSize and @offset)
            DynamicParameters dapperParameters = new();
            dapperParameters.AddDynamicParams(new { search });

            return await AppHelpers.ServiceGetSearchAndPaginated<MyNoteDto>(_connection, sqlPaged, dapperParameters, page, pageSize);
        }

        public async Task<BaseResult<MyNoteDto>> Create(MyNote newEntity)
        {
            _logger.LogInformation("*** {Method} {@Entity}", "Create", newEntity);
            // Agregar verificación de la existencia de la entidad con el Id, solo si el usuario lo provee. (utilizar ServiceEntityExists)
            return await AppHelpers.ServiceCreateEntityHelper<MyNote, MyNoteDto, uint>(
                            newEntity, _idPropertyName, _uow, GetById_Command);
        }

        public async Task<BaseResult<MyNoteDto>> Update(uint id, MyNote updatedEntity)
        {
            _logger.LogInformation("*** {Method} {@Entity}", "Update", updatedEntity);
            return await AppHelpers.ServiceUpdateEntityHelper<MyNote, MyNoteDto, uint>(
                            updatedEntity, id, _idPropertyName, _uow, GetById_Command);
        }
        public async Task<BaseResult<object>> DeleteById(uint id)
        {
            _logger.LogInformation("*** {Method} {id}", "DeleteById", id);
            return await AppHelpers.ServiceDeleteEntityHelper<MyNote>(id, _uow);
        }

        #region Old Code for reference

        // Use this method when full customization is required.
        public async Task<BaseResult<MyNoteDto>> Create_OLD(MyNote newEntity)
        {
            var response = new BaseResult<MyNoteDto>();

            // Set initial properties
            AppHelpers.SetFieldsForEntityCreation(newEntity);

            // Guardar la entidad
            await _uow.GetRepository<MyNote>().Add(newEntity);
            await _uow.SaveAsync();

            // Devolver la entidad recien guardada desde la BD
            response.Data = await _connection.QuerySingleOrDefaultAsync<MyNoteDto>(GetById_Command, new { id1 = newEntity.NoteId });

            _logger.LogInformation("*** {Method} {@Entity}", "Create", newEntity);
            return response;
        }

        // Use this method when full customization is required.
        public async Task<BaseResult<MyNoteDto>> Update_OLD2(uint id, MyNote updatedEntity)
        {
            var response = new BaseResult<MyNoteDto>();

            if (!await AppHelpers.ServiceUpdateEntityCheckHelper<MyNote, MyNoteDto, uint>(updatedEntity, response, id, _idPropertyName, _uow)) return response;

            // Guardar la entidad actualizada
            await _uow.GetRepository<MyNote>().Update(updatedEntity);
            await _uow.SaveAsync();

            // Devolver la entidad recien guardada desde la BD
            response.Data = await _connection.QuerySingleOrDefaultAsync<MyNoteDto>(GetById_Command, new { id1 = id });

            _logger.LogInformation("*** {Method} {@Entity}", "Update", updatedEntity);
            return response;
        }

        // This method is just for reference (no helpers, no global exception handler). Don´t use it.
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
        #endregion
    }
}
