using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AxisAuth.Models.Data.AuthDbContext;

namespace AxisAuth.Repositories.IRepositories
{
    /// <summary>
    /// Repository for Client applications data
    /// </summary>
    public interface IClientRepository : IRepository<Client>
    {
    }
}
