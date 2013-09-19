using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quartz
{
    /// <summary> 
    /// The interface to be implemented by classes which represent a 'retryable job' to be
    /// performed.
    /// </summary>
    /// <remarks>
    /// Instances of this interface must have a <see langword="public" />
    /// no-argument constructor. <see cref="JobDataMap" /> provides a mechanism for 'instance member data'
    /// that may be required by some implementations of this interface.
    /// </remarks>
    /// <seealso cref="IJobDetail" />
    /// <seealso cref="JobBuilder" />
    /// <seealso cref="DisallowConcurrentExecutionAttribute" />
    /// <seealso cref="PersistJobDataAfterExecutionAttribute" />
    /// <seealso cref="ITrigger" />
    /// <seealso cref="IScheduler" />
    /// <author>Christian Krumm</author>
    public interface IRetryableJob : IJob
    {
        /// <summary>
        /// The maximum number of tries
        /// </summary>
        int MaxNumberTries { get; }

        /// <summary>
        /// The delay in IntervalUnit before the Job is rescheduled
        /// </summary>
        int Delay { get; }

        /// <summary>
        /// The IntervalUnit of the 
        /// </summary>
        IntervalUnit IntervalUnit { get; }

    }
}
