using System.Collections.Generic;

namespace EMT.Common.ResponseWrappers
{
    public class BaseResult<T>
    {
        //public bool Success { get; set; }  // 0 : Error, 1: Success
        //public long DataOpMs { get; set; }
        //public string ExceptionMessage { get; set; }

        public T Data { get; set; }  // Acaba se devuelven objetos simples o listas IEnumerable

        public BaseResult()
        {
        }
    }
}
