namespace southafricantaxtool.Interface.Services;

public interface IStore<T>
{
    Task<List<T>> GetAsync(Func<T, bool>? filter = null);
    Task SetAsync(List<T> data);
}