using System;
using System.Collections.Generic;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;

namespace QuartzSendExcel
{
    public static class QuartzManager
    {
        private static readonly IScheduler Sched;

        static QuartzManager()
        {
            ISchedulerFactory sf = new StdSchedulerFactory();
            Sched = sf.GetScheduler();
            Sched.Start();
        }

        /// <summary>
        /// 添加Job 并且以定点的形式运行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jobName"></param>
        /// <param name="cronTime"></param>
        /// <param name="jobDataMap"></param>
        /// <returns></returns>
        public static DateTimeOffset AddJob<T>(string jobName, string cronTime,JobDataMap jobDataMap) where T : IJob
        {
            IJobDetail jobCheck = JobBuilder.Create<T>().WithIdentity(jobName, jobName + "_Group").UsingJobData(jobDataMap).Build();
            ICronTrigger cronTrigger = new CronTriggerImpl(jobName + "_CronTrigger", jobName + "_TriggerGroup", cronTime);
            return Sched.ScheduleJob(jobCheck, cronTrigger);
        }

        /// <summary>
        /// 添加Job 并且以定点的形式运行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jobName"></param>
        /// <param name="cronTime"></param>
        /// <returns></returns>
        public static DateTimeOffset AddJob<T>(string jobName,string cronTime) where T : IJob
        {
            IJobDetail jobCheck = JobBuilder.Create<T>().WithIdentity(jobName, jobName + "_Group").Build();
            ICronTrigger cronTrigger = new CronTriggerImpl(jobName + "_CronTrigger", jobName + "_TriggerGroup", cronTime);
            return Sched.ScheduleJob(jobCheck, cronTrigger);
        }

        /// <summary>
        /// 添加Job 并且以周期的形式运行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jobName"></param>
        /// <param name="simpleTime">毫秒数</param>
        /// <returns></returns>
        public static DateTimeOffset AddJob<T>(string jobName, int simpleTime) where T : IJob
        {
            return AddJob<T>(jobName, DateTime.UtcNow.AddMilliseconds(1), TimeSpan.FromMilliseconds(simpleTime));
        }

        /// <summary>
        /// 添加Job 并且以周期的形式运行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jobName"></param>
        /// <param name="startTime"></param>
        /// <param name="simpleTime">毫秒数</param>
        /// <returns></returns>
        public static DateTimeOffset AddJob<T>(string jobName, DateTimeOffset startTime, int simpleTime) where T : IJob
        {
            return AddJob<T>(jobName, startTime, TimeSpan.FromMilliseconds(simpleTime));
        }

        /// <summary>
        /// 添加Job 并且以周期的形式运行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jobName"></param>
        /// <param name="startTime"></param>
        /// <param name="simpleTime"></param>
        /// <returns></returns>
        public static DateTimeOffset AddJob<T>(string jobName,DateTimeOffset startTime, TimeSpan simpleTime) where T : IJob
        {
            return AddJob<T>(jobName, startTime, simpleTime, new Dictionary<string, object>());
        }

        /// <summary>
        /// 添加Job 并且以周期的形式运行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jobName"></param>
        /// <param name="startTime"></param>
        /// <param name="simpleTime">毫秒数</param>
        /// <param name="mapKey"></param>
        /// <param name="mapValue"></param>
        /// <returns></returns>
        public static DateTimeOffset AddJob<T>(string jobName, DateTimeOffset startTime, int simpleTime,string mapKey,object mapValue) where T : IJob
        {
            Dictionary<string, object> map=  new Dictionary<string, object> {{mapKey, mapValue}};
            return AddJob<T>(jobName, startTime, TimeSpan.FromMilliseconds(simpleTime),map);
        }

        /// <summary>
        /// 添加Job 并且以周期的形式运行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jobName"></param>
        /// <param name="startTime"></param>
        /// <param name="simpleTime"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        public static DateTimeOffset AddJob<T>(string jobName, DateTimeOffset startTime, TimeSpan simpleTime, Dictionary<string,object> map) where T : IJob
        {
            IJobDetail jobCheck = JobBuilder.Create<T>().WithIdentity(jobName, jobName + "_Group").Build();
            jobCheck.JobDataMap.PutAll(map);
            ISimpleTrigger triggerCheck = new SimpleTriggerImpl(jobName + "_SimpleTrigger", jobName + "_TriggerGroup",
                                        startTime,
                                        null,
                                        SimpleTriggerImpl.RepeatIndefinitely,
                                        simpleTime);
            return Sched.ScheduleJob(jobCheck, triggerCheck);
        }

        /// <summary>
        /// 修改触发器时间,需要job名,以及修改结果
        /// CronTriggerImpl类型触发器
        /// </summary>
        public static void UpdateTime(string jobName, string cronTime)
        {
            if (cronTime == null) throw new ArgumentNullException("cronTime");
            TriggerKey key = new TriggerKey(jobName + "_CronTrigger", jobName + "_TriggerGroup");
            CronTriggerImpl cti = Sched.GetTrigger(key) as CronTriggerImpl;
            if (cti != null)
            {
                cti.CronExpression = new CronExpression(cronTime);
                Sched.RescheduleJob(key, cti);
            }
        }

        /// <summary>
        /// 修改触发器时间,需要job名,以及修改结果
        /// SimpleTriggerImpl类型触发器
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="simpleTime">分钟数</param>
        public static void UpdateTime(string jobName, int simpleTime)
        {
            UpdateTime(jobName, TimeSpan.FromMinutes(simpleTime));
        }

        /// <summary>
        /// 修改触发器时间,需要job名,以及修改结果
        /// SimpleTriggerImpl类型触发器
        /// </summary>
        public static void UpdateTime(string jobName, TimeSpan simpleTime)
        {
            TriggerKey key = new TriggerKey(jobName + "_SimpleTrigger", jobName + "_TriggerGroup");
            SimpleTriggerImpl sti = Sched.GetTrigger(key) as SimpleTriggerImpl;
            if (sti != null)
            {
                sti.RepeatInterval = simpleTime;
                Sched.RescheduleJob(key, sti);
            }
        }

        /// <summary>
        /// 暂停所有Job
        /// 暂停功能Quartz提供有很多,以后可扩充
        /// </summary>
        public static void PauseAll()
        {
            Sched.PauseAll();
        }

        /// <summary>
        /// 恢复所有Job
        /// 恢复功能Quartz提供有很多,以后可扩充
        /// </summary>
        public static void ResumeAll()
        {
            Sched.ResumeAll();
        }

        /// <summary>
        /// 删除Job
        /// 删除功能Quartz提供有很多,以后可扩充
        /// </summary>
        /// <param name="jobName"></param>
        public static void DeleteJob(string jobName)
        {
            JobKey jk = new JobKey(jobName, jobName + "_Group");
            Sched.DeleteJob(jk);
        }

        /// <summary>
        /// 卸载定时器
        /// </summary>
        /// <param name="waitForJobsToComplete">是否等待job执行完成</param>
        public static void Shutdown(bool waitForJobsToComplete)
        {
            Sched.Shutdown(waitForJobsToComplete);
        }
    }
}
