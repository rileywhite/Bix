using Bix.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Bix.Repositories.Restful.WebApi
{
    public abstract class ItemsControllerBase<TIdentity, TItem, TRepository> : BixControllerBase
        where TItem : class, IHasIdentity<TIdentity>, IAggregateRoot
        where TRepository : IRepository<TIdentity, TItem>
    {
        protected ILogger Logger { get; }
        protected TRepository Repository { get; }

        public ItemsControllerBase(ILogger logger, TRepository repository)
        {
            Contract.Requires(logger != null);
            Contract.Requires(repository != null);
            Contract.Ensures(this.Logger != null);
            Contract.Ensures(this.Repository != null);

            this.Logger = logger;
            this.Repository = repository;
        }

        [HttpGet]
        public async Task<IEnumerable<TItem>> GetAll(CancellationToken cancellationToken)
        {
            return await this.Repository.GetAllAsync(cancellationToken);
        }
    }
}
