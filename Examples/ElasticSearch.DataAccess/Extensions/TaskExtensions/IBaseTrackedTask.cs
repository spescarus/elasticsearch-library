using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ElasticSearch.DataAccess.Extensions.TaskExtensions
{
    public interface IBaseTrackedTask<TEntity>
    {
        Task<TEntity> AsNoTracking();
        TaskAwaiter<TEntity> GetAwaiter();
    }
}
