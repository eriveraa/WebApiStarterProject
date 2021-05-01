using System.Collections.Generic;

namespace EMT.Common.ResponseWrappers
{
    public class ListResult<T> : BaseResult<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public long TotalRecordCount { get; set; }
        public int TotalPages { get; set; }
        public new IEnumerable<T> Data { get; set; }

        public ListResult() : base()
        {
        }
    }

}
