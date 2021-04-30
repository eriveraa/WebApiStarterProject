﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using EMT.DAL.Interfaces;

namespace EMT.DAL.EF
{
    public class GenericAsyncEFRepository<T> : IGenericAsyncRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context = null;
        private readonly DbSet<T> _table = null;

        public GenericAsyncEFRepository(ApplicationDbContext _context)
        {
            Debug.WriteLine($"* Constructor of {this.GetType()} at {DateTime.Now}");
            this._context = _context;
            _table = _context.Set<T>();
        }

        public IDbConnection GetConnection => _context.Database.GetDbConnection();

        public async Task<IEnumerable<T>> GetAll()
        {
            return await _table.ToListAsync();
        }

        public async Task<T> GetById(object id)
        {
            var entity = await _table.FindAsync(id);
            if (entity != null)
                _context.Entry(entity).State = EntityState.Detached;
            return entity;
        }

        public async Task Add(T entity)
        {            
            await _table.AddAsync(entity);
            return;
        }

        public async Task Update(T entity)
        {
            _table.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            return;
        }

        public async Task DeleteById(object id)
        {
            T existing = _table.Find(id);
            _table.Remove(existing);
            return;
        }
    }
}
