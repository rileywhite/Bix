using Bix.Core;
using Bix.Http;
using Newtonsoft.Json;
using Serilog;
using System;
using System.IO;
using snh=System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Bix.Repositories.Restful.HttpClient
{
    public abstract class ValueTypeClientRepositoryBase<TIdentity, TNaturalKey, TItem>
        : ClientRepositoryBase<TIdentity, TItem>, IValueTypeRepository<TIdentity, TNaturalKey, TItem>
        where TItem : class, IAggregateRoot, IHasIdentity<TIdentity>, IHasNaturalKey<TNaturalKey>
    {
        public ValueTypeClientRepositoryBase(
            IClientConfiguration config,
            IAuthenticationHeaderGenerator authenticationHeaderGenerator,
            ILogger logger)
            : base(config, authenticationHeaderGenerator, logger) { }

        public async Task<TItem> FindOrAddAsync(TItem item, CancellationToken cancellationToken = default(CancellationToken))
        {
            var key = item.NaturalKey;
            try
            {
                using (var client = new snh.HttpClient())
                {
                    var response = await client.PatchWithAuthenticationAsync(
                        $"{this.Config.BaseControllerPath}{this.ItemControllerPath}",
                        item.ToJson(),
                        this.AuthenticationHeaderGenerator,
                        cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new RestfulRepositoryHttpClientException("Unsuccessful service call response") { ErrorResponse = response };
                    }
                    using (var streamReader = new StreamReader(await response.Content.ReadAsStreamAsync()))
                    using (var jsonTextReader = new JsonTextReader(streamReader))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        return new JsonSerializer().Deserialize<TItem>(jsonTextReader);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, "Failure to FindOrAddAsync. Item {Item}", item.ToJson());
                throw;
            }
        }
    }

    public abstract class ValueTypeClientRepositoryBase<TNaturalKey, TItem>
        : ValueTypeClientRepositoryBase<TNaturalKey, TNaturalKey, TItem>, IValueTypeRepository<TNaturalKey, TItem>
        where TItem : class, IAggregateRoot, IHasIdentity<TNaturalKey>, IHasNaturalKey<TNaturalKey>
    {
        public ValueTypeClientRepositoryBase(
            IClientConfiguration config,
            IAuthenticationHeaderGenerator authenticationHeaderGenerator,
            ILogger logger)
            : base(config, authenticationHeaderGenerator, logger) { }
    }
}
