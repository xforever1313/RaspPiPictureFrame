//
// PiPictureFrame - Digital Picture Frame built for the Raspberry Pi.
// Copyright (C) 2022 Seth Hendrick
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

using Quartz;
using Quartz.Impl;
using Serilog;

namespace PiPictureFrame.Api
{
    internal sealed class TaskScheduler : IDisposable
    {
        // ---------------- Fields ----------------

        private readonly IPiPictureFrameApi api;

        private readonly ILogger log;

        private readonly StdSchedulerFactory factory;

        private readonly IScheduler scheduler;

        private IJobDetail? screenOnJob;
        private IJobDetail? screenOffJob;
        private IJobDetail? nextPictureJob;

        private PiPictureFrameConfig? lastConfig;

        // ---------------- Constructor ----------------

        public TaskScheduler( IPiPictureFrameApi piFrameApi, ILogger log )
        {
            this.api = piFrameApi;

            this.log = log;
            this.factory = new StdSchedulerFactory();

            this.scheduler = this.factory.GetScheduler().Result;

            ScreenOnJob.EventTriggered += ScreenOnJob_EventTriggered;
            ScreenOffJob.EventTriggered += ScreenOffJob_EventTriggered;
            NextPictureJob.EventTriggered += NextPictureJob_EventTriggered;
        }

        // ---------------- Functions ----------------

        public void Start()
        {
            if( this.scheduler.IsStarted )
            {
                throw new InvalidOperationException( "Scheduler already started" );
            }

            this.scheduler.Start();
        }

        public void UpdateTasks( PiPictureFrameConfig config )
        {
            ArgumentNullException.ThrowIfNull( config );

            TimeOnly? ReadTime( Func<PiPictureFrameConfig, TimeOnly?> getter )
            {
                if( this.lastConfig is null )
                {
                    return null;
                }
                else
                {
                    return getter( this.lastConfig );
                }
            }

            TimeSpan? ReadSpan( Func<PiPictureFrameConfig, TimeSpan> getter )
            {
                if( this.lastConfig is null )
                {
                    return null;
                }
                else
                {
                    TimeSpan span = getter( this.lastConfig );

                    // If our timespan is less than or equal to zero,
                    // return null to show that we don't want a job scheduled.
                    if( span <= TimeSpan.Zero )
                    {
                        return null;
                    }

                    return span;
                }
            }

            this.screenOnJob = UpdateCronTask<ScreenOnJob>(
                ReadTime( c => c.AwakeTime ),
                config.AwakeTime,
                this.screenOnJob
            );

            this.screenOffJob = UpdateCronTask<ScreenOffJob>(
                ReadTime( c => c.SleepTime ),
                config.SleepTime,
                this.screenOffJob
            );

            this.nextPictureJob = UpdateTimeSpanTask<NextPictureJob>(
                ReadSpan( c => c.PhotoChangeInterval ),
                config.PhotoChangeInterval,
                this.nextPictureJob
            );

            this.lastConfig = config;
        }

        private IJobDetail? UpdateCronTask<TJob>(
            TimeOnly? cachedValue,
            TimeOnly? newValue,
            IJobDetail? currentJob
        ) where TJob : IJob
        {
            string jobName = typeof( TJob ).Name;

            // If our cached value is the same as our new value,
            // nothing needs to change, return the current job
            // as-is.
            if( cachedValue == newValue )
            {
                return currentJob;
            }

            // If our current job is not null,
            // we'll have to delete it since we are about
            // to create a new one.
            if( currentJob is not null )
            {
                this.scheduler.DeleteJob( currentJob.Key );
                this.log.Information( $"{jobName} deleted" );
            }

            // If our new value is null,
            // return as it means we do not want to
            // create a new job of this type.
            if( newValue is null )
            {
                return null;
            }

            IJobDetail job = JobBuilder.Create<TJob>()
                .WithIdentity( $"{jobName}_job" )
                .Build();

            TimeOnly time = newValue.Value;
            string cronString = $"{time.Second} {time.Minute} {time.Hour} * * ?";

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity( $"{jobName}_trigger" )
                .WithCronSchedule( cronString ).Build();

            this.scheduler.ScheduleJob( job, trigger );
            this.log.Information( $"{jobName} started with cron string: {cronString}" );

            return job;
        }

        private IJobDetail? UpdateTimeSpanTask<TJob>(
            TimeSpan? cachedValue,
            TimeSpan? newValue,
            IJobDetail? currentJob
        ) where TJob : IJob
        {
            string jobName = typeof( TJob ).Name;

            // If our cached value is the same as our new value,
            // nothing needs to change, return the current job
            // as-is.
            if( cachedValue == newValue )
            {
                return currentJob;
            }

            // If our current job is not null,
            // we'll have to delete it since we are about
            // to create a new one.
            if( currentJob is not null )
            {
                this.scheduler.DeleteJob( currentJob.Key );
                this.log.Information( $"{jobName} deleted" );
            }

            // If our new value is null,
            // return as it means we do not want to
            // create a new job of this type.
            if( newValue is null )
            {
                return null;
            }

            IJobDetail job = JobBuilder.Create<TJob>()
                .WithIdentity( $"{jobName}_job" )
                .Build();

            TimeSpan span = newValue.Value;

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity( $"{jobName}_trigger" )
                .WithSimpleSchedule(
                    x => x.WithInterval( span ).RepeatForever()
                )
                .Build();

            this.scheduler.ScheduleJob( job, trigger );
            this.log.Information( $"{jobName} started with interval: {span.TotalMinutes} minutes." );

            return job;
        }

        public void Dispose()
        {
            if( this.scheduler.IsStarted )
            {
                this.scheduler.Shutdown();
            }

            NextPictureJob.EventTriggered -= NextPictureJob_EventTriggered;
            ScreenOffJob.EventTriggered -= ScreenOffJob_EventTriggered;
            ScreenOnJob.EventTriggered -= ScreenOnJob_EventTriggered;
        }

        private void ScreenOnJob_EventTriggered()
        {
            this.api.Screen.SetOn( true );
        }

        private void ScreenOffJob_EventTriggered()
        {
            this.api.Screen.SetOn( false );
        }

        private void NextPictureJob_EventTriggered()
        {
            this.api.Renderer.GoToNextPicture();
        }

        private class ScreenOnJob : IJob
        {
            public static event Action? EventTriggered;

            public Task Execute( IJobExecutionContext context )
            {
                return Task.Run( () => EventTriggered?.Invoke() );
            }
        }

        private class ScreenOffJob : IJob
        {
            public static event Action? EventTriggered;

            public Task Execute( IJobExecutionContext context )
            {
                return Task.Run( () => EventTriggered?.Invoke() );
            }
        }

        private class NextPictureJob : IJob
        {
            public static event Action? EventTriggered;

            public Task Execute( IJobExecutionContext context )
            {
                return Task.Run( () => EventTriggered?.Invoke() );
            }
        }
    }
}
