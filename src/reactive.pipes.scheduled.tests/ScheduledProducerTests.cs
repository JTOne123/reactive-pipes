﻿// Copyright (c) Daniel Crenna. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using reactive.pipes.scheduled.tests.Fakes;
using reactive.pipes.scheduled.tests.Fixtures;
using reactive.pipes.scheduled.tests.Migrations;
using Xunit;

namespace reactive.pipes.scheduled.tests
{
	public class ScheduledProducerTests
	{
		[Fact]
		public async Task Queues_for_delayed_execution()
		{
			var settings = new ScheduledProducerSettings
			{
				DelayTasks = true,
				Concurrency = 1,
				SleepInterval = TimeSpan.FromMilliseconds(100),
				Store = new InMemoryScheduleStore()
			};

			var scheduler = new ScheduledProducer(settings);
			scheduler.ScheduleAsync<StaticCountingHandler>(
				o => { o.RunAt = DateTimeOffset.UtcNow + TimeSpan.FromMilliseconds(300); },
				h => { h.SomeOption = "SomeValue"; });
			scheduler.Start(); // <-- starts background thread to poll for tasks

			Assert.True(StaticCountingHandler.Count == 0,
				"handler should not have queued immediately since tasks are delayed");
			await Task.Delay(1000); // <-- should poll for tasks about 10 times
			Assert.True(StaticCountingHandler.Count > 0,
				"handler should have executed since we scheduled it in the future");
			Assert.True(StaticCountingHandler.Count == 1,
				"handler should have only executed once since it does not repeat");
		}

		[Fact(Skip = "Runs for over a minute and requires a database")]
		public void Queues_for_delayed_execution_and_continous_repeating_task()
		{
			using (var db = new SqlServerFixture())
			{
				MigrationHelper.MigrateToLatest("sqlserver", db.ConnectionString);

				var settings = new ScheduledProducerSettings
				{
					DelayTasks = true,
					Concurrency = 0,
					SleepInterval = TimeSpan.FromSeconds(1),
					Store = new SqlScheduleStore(db.ConnectionString)
				};

				var scheduler = new ScheduledProducer(settings);
				scheduler.ScheduleAsync<StaticCountingHandler>(o => o.RepeatIndefinitely(CronTemplates.Minutely()));
				scheduler.Start(); // <-- starts background thread to poll for tasks

				Assert.True(StaticCountingHandler.Count == 0,
					"handler should not have queued immediately since tasks are delayed");
				Thread.Sleep(TimeSpan.FromMinutes(1.1)); // <-- enough time for the next occurrence
				Assert.True(StaticCountingHandler.Count > 0,
					"handler should have executed since we scheduled it in the future");
				Assert.Equal(2, StaticCountingHandler.Count);
			}
		}

		[Fact]
		public void Queues_for_immediate_execution()
		{
			var settings = new ScheduledProducerSettings {DelayTasks = false};
			var scheduler = new ScheduledProducer(settings);
			scheduler.ScheduleAsync<StaticCountingHandler>();

			Assert.True(StaticCountingHandler.Count == 1,
				"handler should have queued immediately since tasks are not delayed");
		}

		[Fact]
		public void Starts_and_stops()
		{
			var scheduler = new ScheduledProducer();
			scheduler.Start();
			scheduler.Stop();
		}
	}
}