using Microsoft.WindowsAzure.Storage.Queue;

namespace WorkerRole1
{
    public class WorkerRole : BaseWorkerRole
    {
        protected override void OnMessageReceived(CloudQueueMessage message)
        {
            string content = message.AsString;
            // TODO: Rest of the Implementation
        }
    }
}
