using System.Collections.Generic;
using System.Threading.Tasks;
using EMT.Common.Entities;
using EMT.Common.ResponseWrappers;

namespace EMT.BLL.Services
{
    public interface IMyNoteService
    {
        Task<BaseResult<MyNoteDto>> GetById(object id);
        Task<ListResult<MyNoteDto>> GetAll();
        Task<BaseResult<MyNoteDto>> Create(MyNote newEntity);
        Task<BaseResult<MyNoteDto>> Update(object id, MyNote updatedEntity);
        Task<BaseResult<object>> DeleteById(object id);
        Task<ListResult<MyNoteDto>> GetSearchAndPaginated(string search = null, int pageIndex = 1, int pageSize = 10);
    }
}
