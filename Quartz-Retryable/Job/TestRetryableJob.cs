using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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



        public override int MaxNumberTries
        {
            get { return 10; }
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
            int currentTry = context.JobDetail.JobDataMap.GetIntValue(NumberTriesJobDataMapKey);
            if (currentTry <= 3)
            {
                throw new RetryableJobExecutionException(string.Format("Current Try: {0}", currentTry));
            }
            else
            {
                return;
            }
        }

    }
}
