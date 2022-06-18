/***************************************************************************/
// Copyright 2013-2022 Riley White
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
/***************************************************************************/

using Bix.Core;
using Bix.WebApi.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bix.Repositories.Restful.WebApi
{
    public abstract class AuthenticatedItemsControllerBase<TIdentity, TItem, TRepository> : AuthenticatedBixControllerBase
        where TItem : class, IModel<TIdentity>, IAggregateRoot
        where TRepository : IRepository<TIdentity, TItem>
    {
        protected ILogger Logger { get; }
        protected TRepository Repository { get; }

        public AuthenticatedItemsControllerBase(ILogger logger, TRepository repository)
        {
            Contract.Requires(logger != null);
            Contract.Requires(repository != null);
            Contract.Ensures(this.Logger != null);
            Contract.Ensures(this.Repository != null);

            this.Logger = logger;
            this.Repository = repository;
        }

        [HttpGet]
        public async Task<IQueryable<TItem>> GetAll(CancellationToken cancellationToken)
        {
            this.Logger?.LogDebug("Processing {Verb} request at {Uri}", this.Request.Method, this.Request.Path);
            return await this.Repository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
