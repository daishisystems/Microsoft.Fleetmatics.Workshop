using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Fleetmatics.Workshop.Microservice.Microservice.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Fleetmatics.Workshop.Service.Service
{
    /// <summary>
    ///     An instance of this class is created for each service replica by the
    ///     Service Fabric runtime.
    /// </summary>
    internal sealed class Service : StatefulService
    {
        public Service(StatefulServiceContext context)
            : base(context)
        {
        }

        /// <summary>
        ///     Optional override to create listeners (e.g., HTTP, Service Remoting, WCF,
        ///     etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        ///     For more information on service communication, see
        ///     https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new ServiceReplicaListener[0];
        }

        /// <summary>
        ///     This is the main entry point for your service replica.
        ///     This method executes when this replica of your service becomes primary and
        ///     has write status.
        /// </summary>
        /// <param name="cancellationToken">
        ///     Canceled when Service Fabric needs to shut down
        ///     this service replica.
        /// </param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var actorId = ActorId.CreateRandom();

                var microservice = ActorProxy.Create<IMicroservice>(
                    actorId,
                    new Uri("fabric:/Fleetmatics.Workshop.Microservice/MicroserviceActorService"));

                var random = new Random();
                await microservice.SetCountAsync(random.Next(), cancellationToken);

                var count = await microservice.GetCountAsync(cancellationToken);

                ServiceEventSource
                    .Current
                    .ServiceMessage(Context, "Current Counter Value: {0}", count);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}