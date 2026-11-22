using System;
using System.Diagnostics;
using System.Threading;
using JetBrains.Annotations;

namespace Sequencer.Midi
{
    /// <summary>
    /// A tempo clock that raises <see cref="Tick"/> once per tick on a dedicated background thread,
    /// timed with <see cref="Stopwatch"/>. Replaces Sanford's Windows-only multimedia-timer clock.
    /// </summary>
    /// <remarks>
    /// Behaviour matches the Sanford clock that <see cref="SequencerClock"/> was written against:
    /// <see cref="Tick"/> is raised before the tick count advances, <see cref="Start"/> resets the count to
    /// zero but does nothing if the clock is already running, and <see cref="Continue"/> resumes from the
    /// current count. <see cref="Started"/> is raised whenever the clock begins running, from either method.
    /// </remarks>
    public sealed class InternalClock : IInternalClock
    {
        /// <summary>
        /// Microseconds per quarter note at 120 BPM, Sanford's default.
        /// </summary>
        private const int DefaultTempo = 500000;

        // Sleep until this close to the next tick, then spin, because Thread.Sleep can overshoot by a millisecond or more.
        private const double SpinThresholdSeconds = 0.002;

        [NotNull] private readonly Func<long> timestamp;
        private readonly long timestampFrequency;
        [NotNull] private readonly object stateLock = new object();
        [CanBeNull] private readonly Thread thread;

        private volatile bool disposed;
        private volatile bool running;
        private volatile int tempo = DefaultTempo;
        private int ticks;
        private int ppqn = 24;
        private long nextTickTimestamp;

        public InternalClock() : this(Stopwatch.GetTimestamp, Stopwatch.Frequency, true)
        {
        }

        /// <summary>
        /// Creates a clock with its own time source. Without a background thread, call
        /// <see cref="ProcessDueTicks"/> to raise ticks; tests use this to control time.
        /// </summary>
        internal InternalClock([NotNull] Func<long> timestamp, long timestampFrequency, bool useBackgroundThread)
        {
            this.timestamp = timestamp;
            this.timestampFrequency = timestampFrequency;

            if (useBackgroundThread)
            {
                thread = new Thread(Run) { IsBackground = true, Name = "Sequencer clock", Priority = ThreadPriority.Highest };
                thread.Start();
            }
        }

        public event EventHandler Tick;

        public event EventHandler Started;

        public event EventHandler Stopped;

        /// <summary>
        /// Microseconds per quarter note. Can change while running; the next tick uses the new length.
        /// </summary>
        public int Tempo
        {
            get => tempo;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Tempo must be positive.");
                }

                tempo = value;
            }
        }

        public bool IsRunning => running;

        public int Ticks
        {
            get => Volatile.Read(ref ticks);
            set => Volatile.Write(ref ticks, value);
        }

        public int Ppqn
        {
            get => ppqn;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Ticks per quarter note must be positive.");
                }

                ppqn = value;
            }
        }

        private long TimestampsPerTick => (long) ((double) Tempo / 1000000 * timestampFrequency / Ppqn);

        public void Start()
        {
            lock (stateLock)
            {
                if (running)
                {
                    return;
                }

                Ticks = 0;
                BeginRunning();
            }

            Started?.Invoke(this, EventArgs.Empty);
        }

        public void Continue()
        {
            lock (stateLock)
            {
                if (running)
                {
                    return;
                }

                BeginRunning();
            }

            Started?.Invoke(this, EventArgs.Empty);
        }

        public void Stop()
        {
            lock (stateLock)
            {
                if (!running)
                {
                    return;
                }

                running = false;
            }

            Stopped?.Invoke(this, EventArgs.Empty);
        }

        public void SetTicks(int value)
        {
            Ticks = value;
        }

        public void Dispose()
        {
            disposed = true;
            running = false;
            thread?.Join(TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// Raises every tick whose time has arrived. Returns the time of the next tick, or null if stopped.
        /// </summary>
        internal long? ProcessDueTicks()
        {
            while (running)
            {
                long now = timestamp();

                if (now < nextTickTimestamp)
                {
                    return nextTickTimestamp;
                }

                // Raise first, then advance, as Sanford's clock did. A handler may set Ticks (to loop).
                Tick?.Invoke(this, EventArgs.Empty);
                Interlocked.Increment(ref ticks);

                // Schedule from the previous deadline, not from now, so timing errors do not accumulate.
                nextTickTimestamp += TimestampsPerTick;
            }

            return null;
        }

        private void BeginRunning()
        {
            nextTickTimestamp = timestamp();
            running = true;
        }

        private void Run()
        {
            while (!disposed)
            {
                long? next = ProcessDueTicks();

                if (next == null)
                {
                    Thread.Sleep(1);
                    continue;
                }

                double secondsUntilNext = (double) (next.Value - timestamp()) / timestampFrequency;

                if (secondsUntilNext > SpinThresholdSeconds)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(secondsUntilNext - SpinThresholdSeconds));
                }
                else
                {
                    Thread.SpinWait(50);
                }
            }
        }
    }
}
