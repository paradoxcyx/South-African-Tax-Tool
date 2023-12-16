namespace southafricantaxtool.Interface;

public interface IStore<T>
{
    /// <summary>
    /// Retrieve data from store
    /// </summary>
    /// <param name="filter">filter to narrow results</param>
    /// <returns>The data</returns>
    Task<List<T>?> GetAsync(Func<T, bool>? filter = null);
    
    /// <summary>
    /// Update data in store
    /// </summary>
    /// <param name="data">The data to update</param>
    /// <returns></returns>
    Task SetAsync(List<T> data);
}