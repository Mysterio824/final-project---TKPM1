using System.Threading.Tasks;

public interface IMockDao
{
    Task<T?> GetAsync<T>(string endpoint);
    Task<T?> PostAsync<T>(string endpoint, object? data = null);
}
