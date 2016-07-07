﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitVersion.Helpers
{
    internal class RetryOperationExponentialBackoff<T> where T : Exception
    {
        private IThreadSleep ThreadSleep;
        private Action Operation;
        private int MaxRetries;

        public RetryOperationExponentialBackoff(IThreadSleep threadSleep, Action operation, int maxRetries = 5)
        {
            if (threadSleep == null)
                throw new ArgumentNullException("threadSleep");
            if (maxRetries < 0)
                throw new ArgumentOutOfRangeException("maxRetries");

            this.ThreadSleep = threadSleep;
            this.Operation = operation;
            this.MaxRetries = maxRetries;
        }

        public void Execute()
        {
            var exceptions = new List<Exception>();

            int tries = 0;
            int sleepMSec = 500;

            while (tries <= MaxRetries)
            {
                tries++;

                try
                {
                    Operation();
                    break;
                }
                catch (T e)
                {
                    exceptions.Add(e);
                    if (tries > MaxRetries)
                    {
                        throw new AggregateException("Operation failed after maximum number of retries were exceeded.", exceptions);
                    }
                }

                ThreadSleep.Sleep(sleepMSec);
                sleepMSec *= 2;
            }
        }
    }
}
