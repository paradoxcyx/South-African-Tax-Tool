using Microsoft.Extensions.Logging;

namespace southafricantaxtool.Interface.Services;

public abstract class Store
{
    protected abstract string RedisKey { get; }
}