using HotChocolate.DataLoader;
using MGTModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MGTModel.AuthService;
using IdentityServer.Data;

namespace MGTServices.AuthService
{
public class UserAppByIdDataLoader
        : BatchDataLoader<string, UserApp>
    {
        private readonly ApplicationDbContext _contextMVC;

        public UserAppByIdDataLoader(ApplicationDbContext contextMVC)
        {
            _contextMVC = contextMVC;
        }

        protected override async Task<IReadOnlyDictionary<string, UserApp>> LoadBatchAsync(
            IReadOnlyList<string> keys, CancellationToken cancellationToken)
        {
            return await _contextMVC.UserApp.Where(q => keys.Contains(q.Id)).ToDictionaryAsync(mc => mc.Id, mc => mc);
        }  
    }
}
