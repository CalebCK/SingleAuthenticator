using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AxisAuth.Models.Data.AuthDbContext;
using AxisAuth.Repositories.IRepositories;

namespace AxisAuth.Repositories
{
    public class ClientRepository : Repository<Client>, IClientRepository
    {
        public ClientRepository(ClientDbContext clientDbContext) : base(clientDbContext)
        {

        }
    }
}
