using System;

namespace Bix.Repositories.Restful.HttpClient
{
    public interface IClientConfiguration
    {
        string BaseControllerPath { get; }
    }
}
