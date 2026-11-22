using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace Sequencer.Midi.Tests
{
    [TestFixture]
    public class InternalClockTests
    {
        // Timestamps in microseconds. At 120 BPM and 96 ticks per quarter note, one tick is 5208 µs.
        private const long MicrosecondsPerSecond = 1000000;
        private const long TickLength = 5208;

        private long now;

        private InternalClock CreateClock()
        {
            now = 0;
            return new InternalClock(() => now, MicrosecondsPerSecond, false) { Ppqn = 96 };
        }

        [Test]
        public void ProcessDueTicks_RaisesOneTickPerTickLength()
        {
            InternalClock clock = CreateClock();
            var tickCount = 0;
            clock.Tick += (sender, args) => tickCount++;

            clock.Start();
            clock.ProcessDueTicks();
            Assert.That(tickCount, Is.EqualTo(1), "The first tick is due immediately.");

            now = TickLength * 10;
            clock.ProcessDueTicks();

            Assert.That(tickCount, Is.EqualTo(11));
            Assert.That(clock.Ticks, Is.EqualTo(11));
        }

        [Test]
        public void ProcessDueTicks_RaisesNoTickBeforeItIsDue()
        {
            InternalClock clock = CreateClock();
            var tickCount = 0;
            clock.Tick += (sender, args) => tickCount++;

            clock.Start();
            clock.ProcessDueTicks();
            now = TickLength - 1;
            long? next = clock.ProcessDueTicks();

            Assert.That(tickCount, Is.EqualTo(1));
            Assert.That(next, Is.EqualTo(TickLength));
        }

        [Test]
        public void Tick_IsRaisedBeforeTheCountAdvances()
        {
            InternalClock clock = CreateClock();
            var ticksSeen = new List<int>();
            clock.Tick += (sender, args) => ticksSeen.Add(clock.Ticks);

            clock.Start();
            now = TickLength * 2;
            clock.ProcessDueTicks();

            Assert.That(ticksSeen, Is.EqualTo(new[] { 0, 1, 2 }));
        }

        [Test]
        public void SettingTicksInATickHandler_ContinuesFromTheNewValue()
        {
            InternalClock clock = CreateClock();
            clock.Tick += (sender, args) =>
            {
                if (clock.Ticks == 5)
                {
                    clock.Ticks = 0;
                }
            };

            clock.Start();
            now = TickLength * 5;
            clock.ProcessDueTicks();

            Assert.That(clock.Ticks, Is.EqualTo(1));
        }

        [Test]
        public void Start_ResetsTicks()
        {
            InternalClock clock = CreateClock();
            clock.Ticks = 50;

            clock.Start();

            Assert.That(clock.Ticks, Is.EqualTo(0));
            Assert.That(clock.IsRunning, Is.True);
        }

        [Test]
        public void Start_WhileRunning_DoesNothing()
        {
            InternalClock clock = CreateClock();
            var startedCount = 0;
            clock.Started += (sender, args) => startedCount++;

            clock.Start();
            clock.Ticks = 50;
            clock.Start();

            Assert.That(clock.Ticks, Is.EqualTo(50));
            Assert.That(startedCount, Is.EqualTo(1));
        }

        [Test]
        public void Continue_KeepsTicksAndRaisesStarted()
        {
            InternalClock clock = CreateClock();
            var startedCount = 0;
            clock.Started += (sender, args) => startedCount++;
            clock.Ticks = 50;

            clock.Continue();

            Assert.That(clock.Ticks, Is.EqualTo(50));
            Assert.That(startedCount, Is.EqualTo(1));
        }

        [Test]
        public void Stop_RaisesStoppedAndStopsTicking()
        {
            InternalClock clock = CreateClock();
            var stoppedCount = 0;
            var tickCount = 0;
            clock.Stopped += (sender, args) => stoppedCount++;
            clock.Tick += (sender, args) => tickCount++;

            clock.Start();
            clock.ProcessDueTicks();
            clock.Stop();
            now = TickLength * 10;
            long? next = clock.ProcessDueTicks();

            Assert.That(stoppedCount, Is.EqualTo(1));
            Assert.That(tickCount, Is.EqualTo(1));
            Assert.That(next, Is.Null);
        }

        [Test]
        public void Stop_WhenNotRunning_DoesNotRaiseStopped()
        {
            InternalClock clock = CreateClock();
            var stoppedCount = 0;
            clock.Stopped += (sender, args) => stoppedCount++;

            clock.Stop();

            Assert.That(stoppedCount, Is.EqualTo(0));
        }

        [Test]
        public void Tempo_Is120BeatsPerMinute()
        {
            InternalClock clock = CreateClock();

            Assert.That(clock.Tempo, Is.EqualTo(500000));
        }

        [Test]
        public void RealClock_TicksAtTheExpectedRate()
        {
            var tickTimes = new List<long>();

            using (var clock = new InternalClock { Ppqn = 96 })
            {
                clock.Tick += (sender, args) =>
                {
                    lock (tickTimes)
                    {
                        tickTimes.Add(Stopwatch.GetTimestamp());
                    }
                };

                clock.Start();
                Thread.Sleep(500);
                clock.Stop();
            }

            // 500 ms / 5.208 ms per tick is about 96 ticks. Allow for a busy machine.
            Assert.That(tickTimes.Count, Is.InRange(80, 100));

            double[] intervalsMs = tickTimes.Zip(tickTimes.Skip(1), (a, b) => (b - a) * 1000.0 / Stopwatch.Frequency).ToArray();
            TestContext.Out.WriteLine($"ticks={tickTimes.Count} mean={intervalsMs.Average():F3}ms " +
                                      $"min={intervalsMs.Min():F3}ms max={intervalsMs.Max():F3}ms (ideal 5.208ms)");
        }
    }
}
