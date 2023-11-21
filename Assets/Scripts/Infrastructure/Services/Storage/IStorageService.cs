namespace Infrastructure.Services.Storage
{
    public interface IStorageService : IService
    {
        void Save(string key, object data);
        T Load<T>(string key) where T : new();
    }
}