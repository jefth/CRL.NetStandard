﻿using CRL.EventBus.Queue;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CRL.EventBus
{
    public interface IPublisher
    {
        void Publish<T>(string name, params T[] msgs);
        Task PublishAsync<T>(string name, params T[] msgs);
    }
    public class Publisher : IPublisher, IDisposable
    {
        AbsQueue queue;
#if NETSTANDARD
        public Publisher(Microsoft.Extensions.Options.IOptions<QueueConfig> options)
        {
            queue = QueueFactory.CreateClient(options.Value, false);
        }     
#endif
        public Publisher(QueueConfig _queueConfig)
        {
            queue = QueueFactory.CreateClient(_queueConfig, false);
        }
        public void Dispose()
        {
            queue.Dispose();
        }

        public void Publish<T>(string name, params T[] msgs)
        {
            queue.Publish(name, msgs);
        }
        public Task PublishAsync<T>(string name, params T[] msgs)
        {
            Publish(name, msgs);
            return Task.FromResult(true);
        }
        public long CleanQueue(string name)
        {
            return queue.CleanQueue(name);
        }
        public long GetQueueLength(string name)
        {
            return queue.GetQueueLength(name);
        }
    }
}
