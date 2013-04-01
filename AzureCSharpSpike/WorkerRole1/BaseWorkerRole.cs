namespace WorkerRole1
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;

    using Microsoft.WindowsAzure.ServiceRuntime;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;

    /// <summary>
    /// The base worker role.
    /// </summary>
    public abstract class BaseWorkerRole : RoleEntryPoint
    {
        public CloudStorageAccount StorageAccount { get; private set; }
        public CloudQueueClient QueueClient { get; private set; }
        public CloudQueue Queue { get; private set; }
        
        private volatile bool gracefulShutdown;

        /// <summary>
        /// The run.
        /// </summary>
        public override void Run()
        {
            while (true)
            {
                if (this.gracefulShutdown)
                {
                    break;
                }

                CloudQueueMessage message = Queue.GetMessage();

                if (message != null)
                {
                    try
                    {
                        this.OnPreMessageReceived(message);
                        this.OnMessageReceived(message);
                        this.OnPostMessageReceived(message);
                    }
                    catch (Exception e)
                    {
                        this.OnException(message, e);
                    }
                }
                else
                {
                    Thread.Sleep(5000);
                }
            }
        }

        /// <summary>
        /// The shutdown.
        /// </summary>
        public virtual void Shutdown()
        {
            this.gracefulShutdown = true;
        }

        /// <summary>
        /// The on start.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            gracefulShutdown = false;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.
            
            try
            {
                StorageAccount = CloudStorageAccount.DevelopmentStorageAccount;
                QueueClient = StorageAccount.CreateCloudQueueClient();
                Queue = QueueClient.GetQueueReference("batatinhas");
                Queue.CreateIfNotExists();
            }
            catch (Exception e)
            {
                Trace.TraceError("Exception Ocurred when starting worker role: {0}", e);
                return false;
            }

            return base.OnStart();
        }

        /// <summary>
        /// The on message received.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        protected abstract void OnMessageReceived(CloudQueueMessage message);

        /// <summary>
        /// The on pre message received.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        protected virtual void OnPreMessageReceived(CloudQueueMessage message)
        {
            Queue.UpdateMessage(message, TimeSpan.FromMinutes(5), MessageUpdateFields.Visibility);
        }

        /// <summary>
        /// The on post message received.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        protected virtual void OnPostMessageReceived(CloudQueueMessage message)
        {
        }

        /// <summary>
        /// The on stop.
        /// </summary>
        public override void OnStop()
        {
        }

        /// <summary>
        /// The on exception.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected virtual void OnException(CloudQueueMessage message, Exception e)
        {
            Trace.TraceError("Exception Occurred: {0}", e);
            Queue.UpdateMessage(message, TimeSpan.MinValue, MessageUpdateFields.Visibility);
        }
    }
}