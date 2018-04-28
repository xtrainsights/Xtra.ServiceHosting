using System.Threading.Tasks;


namespace Xtra.ServiceHost
{

    public interface IServiceWorker
    {
        Task Initialize(params string[] args);
        Task Start();
        Task Stop();
        Task Pause();
        Task Resume();
    }

}