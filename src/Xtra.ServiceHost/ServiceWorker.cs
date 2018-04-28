using System;
using System.Threading;
using System.Threading.Tasks;


namespace Xtra.ServiceHost
{

    public abstract class ServiceWorker : IServiceWorker
    {

        protected ServiceWorker()
            : this(new CancellationTokenSource())
        { }


        protected ServiceWorker(CancellationTokenSource cancellationTokenSource)
            => CancellationTokenSource = cancellationTokenSource;


        public virtual Task Initialize(params string[] args)
            => Task.CompletedTask;


        public virtual Task Start()
            => Task.CompletedTask;


        public virtual Task Stop()
            => Task.CompletedTask;


        public virtual Task Pause()
            => Task.CompletedTask;


        public virtual Task Resume()
            => Task.CompletedTask;


        protected CancellationTokenSource CancellationTokenSource { get; set; }

        protected CancellationToken CancellationToken => CancellationTokenSource.Token;

    }

}
