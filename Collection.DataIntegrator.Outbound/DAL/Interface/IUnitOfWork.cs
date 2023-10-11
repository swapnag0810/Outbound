using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Collection.DataIntegrator.Outbound.DAL.Interface
{
    public interface IUnitOfWork<TDbContext> : IDisposable where TDbContext : DbContext
    {
        TDbContext GetContext();
        int Complete();
    }
}
