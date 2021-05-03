using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EMT.Common.Entities;
using EMT.Common.Interfaces;
using EMT.DAL.Interfaces;

namespace EMT.DAL.EF
{
    public class UnityOfWork_EF : IUnityOfWork, IUnityOfWork_EF
    {
        private ApplicationDbContext _context = null;
        private bool disposedValue;

        public UnityOfWork_EF(ApplicationDbContext _context)
        {
            //Debug.WriteLine($"* Constructor of {this.GetType()} at {DateTime.Now}");
            this._context = _context;
        }

        public IDbConnection GetConnection => _context.Database.GetDbConnection();

        public ApplicationDbContext GetContext => _context;

        public async Task StartTransaction()
        {
            return;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
            return;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _context.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~UOW_EntityFramework()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private Dictionary<Type, object> ReposDict = new Dictionary<Type, object>();

        public IGenericAsyncRepository<T> GetRepository<T>() where T : AuditableEntityBase
        {
            //Si no existe el Repositorio lo crea
            if (!ReposDict.ContainsKey(typeof(T)))
                ReposDict.Add(typeof(T), new GenericAsyncEFRepository<T>(_context));

            // Devolver el Repo
            return ReposDict[typeof(T)] as IGenericAsyncRepository<T>;
        }

        // ----------------------------------------------------------------------
        // AGREGAR/IMPLEMENTAR EL ACCESO A LOS REPOSITORIOS AQUÍ

        //private readonly IGenericAsyncRepository<note> _noteRepository;
        //public IGenericAsyncRepository<note> NoteRepository => GetRepo(_noteRepository);

        //private readonly IGenericAsyncRepository<AppParameter> _appParameterRepository;
        //public IGenericAsyncRepository<AppParameter> AppParameterRepository => GetRepo(_appParameterRepository);

    }
}
