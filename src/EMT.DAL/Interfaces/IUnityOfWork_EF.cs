using System;
using System.Data;
using System.Threading.Tasks;
using EMT.DAL.EF;
using EMT.DAL.Interfaces;

namespace EMT.DAL.Interfaces
{
    public interface IUnityOfWork_EF : IUnityOfWork
    {
        ApplicationDbContext GetContext { get; }

        // ---------------------------------------------------------------------
        // Agregar aquí todos los repositorios requeridos (YA NO SERÍA NECESARIO PORQUE SE MANEJARÍA CON EL GetRepository<T>)
        //IGenericAsyncRepository<note> NoteRepository { get; }
        //IGenericAsyncRepository<AppParameter> AppParameterRepository { get; }
    }
}
