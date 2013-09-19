using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Globalization;

namespace Quartz
{
    /// <summary>
    /// An exception that can be thrown by a <see cref="IRetryableJob" />
    /// to indicate to the Quartz <see cref="IScheduler" /> that an error
    /// occurred while executing, and whether or not the <see cref="IJob" /> requests
    /// to be re-fired immediately (using the same <see cref="IJobExecutionContext" />,
    /// or whether it wants to be unscheduled.
    /// </summary>
    /// <remarks>
    /// Note that if the flag for 'refire immediately' is set, the flags for
    /// unscheduling the Job are ignored.
    /// </remarks>
    /// <seealso cref="IJob" />
    /// <seealso cref="IJobExecutionContext" />
    /// <seealso cref="SchedulerException" />
    /// <author>Christian Krumm (.NET)</author>
    [Serializable]
    public class RetryableJobExecutionException : JobExecutionException
    {
        private bool retry = true;

        /// <summary>
        /// Gets or sets a value indicating whether the job shall be retried.
        /// </summary>
        /// <value><c>true</c> if the <see cref="Listener.RetryableJobListener" /> shall retry the job otherwise, <c>false</c>.</value>
        public virtual bool RetryJob
        {
            set { retry = value; }
            get { return retry; }
        }

        /// <summary>
		/// Create a RetryableJobExecutionException, with the 're-fire immediately' flag set
        /// to <see langword="false" /> and the retry flag set to <see langword="true" />.
		/// </summary>
		public RetryableJobExecutionException() : base()
		{
		}

		/// <summary>
        /// Create a RetryableJobExecutionException, with the given cause.
		/// </summary>
		/// <param name="cause">The cause.</param>
		public RetryableJobExecutionException(Exception cause) : base(cause)
		{
		}

		/// <summary>
        /// Create a RetryableJobExecutionException, with the given message.
		/// </summary>
		public RetryableJobExecutionException(string msg) : base(msg)
		{
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="RetryableJobExecutionException"/> class.
		/// </summary>
		/// <param name="msg">The message.</param>
		/// <param name="cause">The original cause.</param>
		public RetryableJobExecutionException(string msg, Exception cause) : base(msg, cause)
		{
		}

		/// <summary>
        /// Create a RetryableJobExecutionException with the 're-fire immediately' flag set
		/// to the given value. Also the 'retry' flag is set to the given value.
		/// </summary>
		public RetryableJobExecutionException(bool refireImmediately, bool retry) : base(refireImmediately)
		{
            this.retry = retry;
        }

		/// <summary>
        /// Create a RetryableJobExecutionException with the given underlying exception, and
        /// the 're-fire immediately' flag and the 'retry' flag
        /// are set to the given values.
		/// </summary>
		public RetryableJobExecutionException(Exception cause, bool refireImmediately, bool retry) : base(cause, refireImmediately)
		{
		}

		/// <summary>
        /// Create a RetryableJobExecutionException with the given message, and underlying
		/// exception, the 're-fire immediately' flag and the 'retry' flag
        /// are set to the given values.
		/// </summary>
		public RetryableJobExecutionException(string msg, Exception cause, bool refireImmediately, bool retry) : base(msg, cause, refireImmediately)
		{
            this.retry = retry;
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryableJobExecutionException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"></see> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"></see> is zero (0). </exception>
        /// <exception cref="T:System.ArgumentNullException">The info parameter is null. </exception>
        protected RetryableJobExecutionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Creates and returns a string representation of the current exception.
        /// </summary>
        /// <returns>
        /// A string representation of the current exception.
        /// </returns>
        /// <PermissionSet><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*"/></PermissionSet>
        public override string ToString()
        {
            return
                string.Format(CultureInfo.InvariantCulture,
                    "Retryable Parameters: retry = {0} \n {1}",
                    RetryJob, base.ToString());
        }

    }
}
