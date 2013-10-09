using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using System.Globalization;

namespace Quartz.Listener
{
    /// <summary>
    /// This listener allows jobs to be rescheduled automatically x times with a spectified delay.
    /// The corresponding job has to implement the interface Quartz.IRetryableJob in order to set the maximum number of tries 
    /// and the corresponding delay. Furthermore the attribute PersistJobDataAfterExecution is required for the class.
    /// Implementors shall use the class RetryableJob as a base. 
    /// </summary>
    /// <author>Christian Krumm</author>
    /// <seealso cref="DisallowConcurrentExecutionAttribute" />
    /// <seealso cref="PersistJobDataAfterExecutionAttribute" />
    /// <seealso cref="IRetryableJob" />
	/// <seealso cref="RetryableJob" />
	public class RetryableJobListener : JobListenerSupport
    {

        private readonly IScheduler _scheduler;
        private readonly ILog logger;


        /// <summary>
        /// Key of JobDataMap to hold the current try number 
        /// </summary>
        const string NumberTriesJobDataMapKey = "RetryableJobListener.TryNumber";

        /// <summary>
        /// Key of JobDataMap to hold the maximum number of tries 
        /// </summary>
        const string MaxTriesJobDataMapKey = "RetryableJobListener.MaxTries";

        /// <summary>
        /// Key of JobDataMap to hold the delay between different tries (in Miliseconds) 
        /// </summary>
        const string WaitIntervalJobDataMapKey = "RetryableJobListener.WaitInterval";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scheduler"></param>
        public RetryableJobListener(IScheduler scheduler)
        {
            if (scheduler == null)
                throw new ArgumentNullException("scheduler");

            _scheduler = scheduler;
            logger = LogManager.GetLogger(GetType());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public override void JobToBeExecuted(IJobExecutionContext context)
        {
            var retryableJob = context.JobInstance as IRetryableJob;
            if (retryableJob == null)
                return;

            // Job needs attribute PersistJobDataAfterExecution to be set
            Type jobType = retryableJob.GetType();
            if (!Quartz.Util.ObjectUtils.IsAttributePresent(jobType, typeof(PersistJobDataAfterExecutionAttribute)))
            {
                logger.FatalFormat("The retryable job of type {0} requires the attribute PersistJobDataAfterExecution to be set on the class.", jobType);
                return;
            }

            if (!context.JobDetail.JobDataMap.Contains(NumberTriesJobDataMapKey))
                context.JobDetail.JobDataMap.PutAsString(NumberTriesJobDataMapKey, 0);

            int numberTries = context.JobDetail.JobDataMap.GetIntValueFromString(NumberTriesJobDataMapKey);
            context.JobDetail.JobDataMap.PutAsString(NumberTriesJobDataMapKey, ++numberTries);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="jobException"></param>
        public override void JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException)
        {
            // If there no exception occured there is no need for a retry
            if (jobException == null)
                return;

            // Ignore Jobs which are not retryable
            var retryableJob = context.JobInstance as IRetryableJob;
            if (retryableJob == null)
                return;

            // Job has to throw a RetryableJobExecutionException with RetryJob set to true in order to be rescheduled
            var retryableJobExecutionException = jobException as RetryableJobExecutionException;
            if (retryableJobExecutionException != null && retryableJobExecutionException.RetryJob)
            {
                // Get maximum number of tries
                // Check JobDataMap for the corresponding value
                // Use retryableJob.MaxNumberTries as a default value (= fallback)
                int maxTries;
                object obj = context.JobDetail.JobDataMap.Get(MaxTriesJobDataMapKey);
                if (null == obj || !Int32.TryParse(obj.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out maxTries))
                {
                    maxTries = retryableJob.MaxNumberTries;
                    logger.TraceFormat("Maximum number of tries has NOT been spectified in JobDataMap. Using default {2} for Job {0}.{1}.", context.JobDetail.Key.Group, context.JobDetail.Key.Name, maxTries);
                }
                else
                {
                    logger.TraceFormat("Maximum number of tries has been spectified as {2} in JobDataMap for Job {0}.{1}.", context.JobDetail.Key.Group, context.JobDetail.Key.Name, maxTries);
                }


                int numberTries = context.JobDetail.JobDataMap.GetIntValue(NumberTriesJobDataMapKey);
                logger.TraceFormat("Number of tries for Job {0}.{1} is {2}. Maximum number of tries is {3}.", context.JobDetail.Key.Group, context.JobDetail.Key.Name, numberTries, maxTries);

                if (numberTries >= maxTries)
                {
                    logger.ErrorFormat("Job {0}.{1} has reached the maximum number of tries (= {2}). Giving up...", context.JobDetail.Key.Group, context.JobDetail.Key.Name, maxTries);
                    return; // Max number of tries reached
                }
                else
                {
                    logger.InfoFormat("Job {0}.{1} has not yet reached the maximum number of tries (= {2}). Reschedule...", context.JobDetail.Key.Group, context.JobDetail.Key.Name, maxTries);

                    // Schedule next try
                    ScheduleRetryableJob(context, retryableJob);
                }
            }
            else
            {
                logger.DebugFormat("NOT rescheduling job {0}.{1} as it hasn't thrown a RetryableJobExecutionException or the corresponding RetryJob flag is false.", context.JobDetail.Key.Group, context.JobDetail.Key.Name);
            }
        }


        /// <summary>
        /// Schedules the retryableJob
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="retryableJob">The job</param>
        private void ScheduleRetryableJob(IJobExecutionContext context, IRetryableJob retryableJob)
        {
            var oldTrigger = context.Trigger;
            TriggerKey retryTriggerKey = new TriggerKey(oldTrigger.Key.Name, "RETRY");
                      

            int waitInterval;
            IntervalUnit intervalUnit;

            // Check if WaitInterval has been set in JobDataMap
            // If it was set, the value is in Milliseconds and overwrites value in code
            // Otherwise use value defined in code
            object obj = context.JobDetail.JobDataMap.Get(WaitIntervalJobDataMapKey);
            if (null == obj || !Int32.TryParse(obj.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out waitInterval))
            {
                waitInterval = retryableJob.Delay;
                intervalUnit = retryableJob.IntervalUnit;
                logger.DebugFormat("Wait Interval has NOT been spectified in JobDataMap. Reschedule Job {0}.{1} using defaults (wait interval of {2} {3}(s)).", context.JobDetail.Key.Group, context.JobDetail.Key.Name, waitInterval, intervalUnit.ToString());
            }
            else
            {
                intervalUnit = IntervalUnit.Millisecond; // value in JobDataMap is always in Miliseconds
                logger.TraceFormat("Reschedule Job {0}.{1} with a wait interval of {2} {3}(s).", context.JobDetail.Key.Group, context.JobDetail.Key.Name, waitInterval, intervalUnit.ToString());
            }

            // Create and schedule new trigger
            var retryTrigger = TriggerBuilder.Create().ForJob(context.JobDetail).WithIdentity(retryTriggerKey).WithSimpleSchedule(s => s.WithRepeatCount(0)).StartAt(DateBuilder.FutureDate(waitInterval, intervalUnit)).Build();
            if (!_scheduler.CheckExists(retryTriggerKey))
                _scheduler.ScheduleJob(retryTrigger);
            else
                _scheduler.RescheduleJob(retryTriggerKey, retryTrigger);
        }

        /// <summary>
        /// The name of the JobListener <see cref="IJobListener"/>
        /// </summary>
        public override string Name
        {
            get { return this.GetType().FullName; }
        }        
    }
}
