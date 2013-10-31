using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;

namespace Quartz.Job
{
    /// <summary>
    /// A job which can be used to test the Quartz-Retryable assembly 
    /// </summary>
    public class TestRetryableJob : RetryableJob
    {
        /// <summary>
        /// Key of JobDataMap to hold the current try number 
        /// </summary>
        const string NumberTriesJobDataMapKey = "RetryableJobListener.TryNumber";

        private readonly ILog logger = LogManager.GetLogger(typeof(TestRetryableJob));

        public override int MaxNumberTries
        {
            get { return 5; }
        }

        public override int Delay
        {
            get { return 5; }
        }

        public override IntervalUnit IntervalUnit
        {
            get { return Quartz.IntervalUnit.Second; }
        }

        public override void Execute(IJobExecutionContext context)
        {
            TriggerKey key = context.Trigger.Key;

            int currentTry = context.JobDetail.JobDataMap.GetIntValue(NumberTriesJobDataMapKey);
            if (currentTry == 3 || currentTry == 5 || currentTry == 10 || currentTry == 12)
            {
                throw new RetryableJobExecutionException(string.Format("{0} --> ERROR - Current Try: {1}. RESCHEDULE....", key, currentTry));
            }
            else
            {
                logger.DebugFormat("{0} --> OK - Current Try: {1}", key, currentTry);
            }
        }

    }
}
