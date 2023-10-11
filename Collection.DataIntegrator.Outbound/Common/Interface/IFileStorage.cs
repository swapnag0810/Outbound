using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collection.DataIntegrator.Outbound.Common.Interface
{
    public interface IFileStorage
    {
        bool CanConnect();
        IEnumerable<string> List(string path);
        bool Upload(string from, string to);
        bool Download(string from, string to);
        bool Delete(string path);
    }
}

