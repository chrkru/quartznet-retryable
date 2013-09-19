using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quartz
{
    /// <summary>
    /// A helpful abstract base class for implementors of <see cref="IRetryableJob" />.
    /// </summary>
    [DisallowConcurrentExecution]
    [PersistJobDataAfterExecution]
    public abstract class RetryableJob : IRetryableJob
    {
        /// <summary>
        /// <see cref="IRetryableJob"/>
        /// </summary>
        public abstract int MaxNumberTries
        {
            get;
        }

        
        
        /// <summary>
        /// <see cref="IRetryableJob"/>
        /// </summary>
        public abstract int Delay
        {
            get;
        }


        /// <summary>
        /// <see cref="IRetryableJob"/>
        /// </summary>
        public abstract IntervalUnit IntervalUnit
        {
            get;
        }

        /// <summary>
        /// <see cref="IRetryableJob"/>
        /// </summary>
        public abstract void Execute(IJobExecutionContext context);
    }
}
