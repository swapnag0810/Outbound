using Collection.DataIntegrator.Outbound.DAL.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Collection.DataIntegrator.Outbound.DAL.UnitOfWork
{
    public class UnitOfWork<TDbContext> : IUnitOfWork<TDbContext> where TDbContext : DbContext
    {
        protected readonly TDbContext _context;
        public UnitOfWork(TDbContext context)
        {
            _context = context;
        }

        public TDbContext GetContext()
        {
            return _context;
        }
       
        public int Complete()
        {
            return _context.SaveChanges();
        }
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
