using System.Diagnostics;
using System.Threading;
using Microsoft.WindowsAzure.Storage.Queue;
using WorkerRoleLib;

namespace WorkerRole1
{
    public class WorkerRole : BaseWorkerRole
    {
        protected override void OnMessageReceived(CloudQueueMessage message)
        {
            Trace.TraceInformation(@"Let's pretend we're working");
            string content = message.AsString;
            // TODO: Rest of the Implementation

            Thread.Sleep(2000);
        }

        protected override void OnPostMessageReceived(CloudQueueMessage message)
        {
            base.OnPostMessageReceived(message);
            Trace.TraceInformation(@"Message Processed");
        }
    }
}
