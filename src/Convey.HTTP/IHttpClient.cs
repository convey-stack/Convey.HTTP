using System.Threading.Tasks;

namespace Convey.HTTP
{
    public interface IHttpClient
    {
        Task<T> GetAsync<T>(string requestUri);
    }
}