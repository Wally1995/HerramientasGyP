namespace HerramientasGyP.UI.HttpServices;

public interface IClient
{
    // Generic CRUD
    Task<T?> GetAsync<T>(string uri);
    Task<IEnumerable<T>?> GetAllAsync<T>(string uri);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string uri, TRequest data);
    Task<TResponse?> PutAsync<TRequest, TResponse>(string uri, TRequest data);
    Task<bool> DeleteAsync(string uri);

    // Space for specific methods
    Task<bool> Login(string username, string password);
    Task Logout();
}