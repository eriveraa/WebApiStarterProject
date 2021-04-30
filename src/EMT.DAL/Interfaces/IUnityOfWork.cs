using System;
using System.Data;
using System.Threading.Tasks;
using EMT.Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace EMT.DAL.Interfaces
{
    public interface IUnityOfWork : IDisposable
    {
        IDbConnection GetConnection { get; }
        Task StartTransaction();
        Task SaveAsync();
        IGenericAsyncRepository<T> GetRepository<T>() where T : AuditableEntityBase;

        // ---------------------------------------------------------------------
        // Agregar aquí todos los repositorios requeridos (YA NO SERÍA NECESARIO PORQUE SE MANEJARÍA CON EL GetRepository<T>)
        //IGenericAsyncRepository<note> NoteRepository { get; }
        //IGenericAsyncRepository<AppParameter> AppParameterRepository { get; }
    }
}
