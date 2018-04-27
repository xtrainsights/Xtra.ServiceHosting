using System.Threading.Tasks;


namespace Xtra.ServiceHost
{

    public interface IAsyncServiceWorker
    {
        Task OnStart(params string[] args);
        Task Run();
        Task OnStop();
    }

}