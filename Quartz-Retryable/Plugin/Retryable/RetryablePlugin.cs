using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz.Listener;

namespace Quartz.Plugin.Retryable
{
    /// <summary>
    /// A plugin which can be used to add the RetryableJobListener to the scheduler 
    /// </summary>
    /// <author>Christian Krumm</author>
    public class RetryablePlugin : Quartz.Spi.ISchedulerPlugin
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pluginName"></param>
        /// <param name="sched"></param>
        public void Initialize(string pluginName, IScheduler sched)
        {
            IMatcher<JobKey> matcher = Quartz.Impl.Matchers.EverythingMatcher<JobKey>.AllJobs();
            sched.ListenerManager.AddJobListener(new RetryableJobListener(sched), matcher);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            // Do nothing
        }

        /// <summary>
        /// 
        /// </summary>
        public void Shutdown()
        {
            // Do nothing
        }

    }
}
