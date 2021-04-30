using System;
using System.Collections.Generic;
using System.Text;

namespace EMT.Common.Interfaces
{
    /// <summary>
    /// Interfaz que es utilizada por las clases que hacen JOINS a tablas de usuario.
    /// Es útil porque un cambio de nombre en estos campos, permite renombrar todas las clases que la implementan
    /// </summary>
    public interface IUserJoins
    {
        string CreatedBy_FN { get; set; }  // Join CreatedBy (FullName)
        string UpdatedBy_FN { get; set; } // Join UpdatedBy (FullName)
    }

}
