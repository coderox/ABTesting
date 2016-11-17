using System.Threading.Tasks;

namespace Client.Utilities
{
    public interface IExperimentsService
    {
        Task Initialize();
        T Get<T>(string key, T defaultValue);
        void LogView<T>(string key, T value);
        void LogConversion<T>(string key, T value);
    }
}
