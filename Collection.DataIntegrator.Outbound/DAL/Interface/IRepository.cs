using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Collection.DataIntegrator.Outbound.DAL.Interface
{
    public interface IRepository 
    {
        T GetById<T>(int id) where T : class;
        IEnumerable<T> GetAll<T>() where T : class;
        IEnumerable<T> Find<T>(Expression<Func<T, bool>> expression) where T : class;
        void Add<T>(T entity) where T : class;
        void AddRange<T>(IEnumerable<T> entities) where T : class;
        void Remove<T>(T entity) where T : class;
        void RemoveRange<T>(IEnumerable<T> entities) where T : class;
    }
}
