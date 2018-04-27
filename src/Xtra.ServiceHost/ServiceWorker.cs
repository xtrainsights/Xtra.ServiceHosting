using System;
using System.Threading;
using System.Threading.Tasks;


namespace Xtra.ServiceHost
{

    public abstract class ServiceWorker : IAsyncServiceWorker
    {

        protected ServiceWorker()
            : this(new CancellationTokenSource())
        { }


        protected ServiceWorker(CancellationTokenSource cancellationTokenSource)
            => CancellationTokenSource = cancellationTokenSource;


        public virtual Task OnStart(params string[] args)
            => Task.CompletedTask;


        public virtual Task Run()
            => Task.CompletedTask;


        public virtual Task OnStop()
            => Task.CompletedTask;


        protected CancellationTokenSource CancellationTokenSource { get; set; }

        protected CancellationToken CancellationToken => CancellationTokenSource.Token;

    }

}
