using EMT.Common.Entities;
using EMT.Common.Interfaces;

namespace EMT.BLL.Services
{
    public class MyNoteDto : MyNote, IUserJoins
    {
        // IUserJoins
        public string CreatedBy_FN { get; set; }
        public string UpdatedBy_FN { get; set; }
    }

}