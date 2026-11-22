using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Sequencer.Domain.Arrangement;
using Sequencer.Domain.Effects;

namespace Sequencer.Audio.Effects
{
    /// <summary>
    /// A track's channel: its voices are mixed here, then pass through the track's effects in order. Adding,
    /// removing or switching an effect crossfades it in or out over 20 ms, so the change never clicks, and tails
    /// (echoes, reverb) carry on after the notes stop.
    /// </summary>
    public sealed class TrackBus : ISampleProvider
    {
        private const double CrossfadeSeconds = 0.02;

        [NotNull] private readonly Track track;
        [NotNull] private readonly MixingSampleProvider voices;
        [NotNull] private readonly Func<double> beatsPerMinute;
        [NotNull] private readonly List<Slot> slots = new List<Slot>();
        private readonly float fadeStep;
        [NotNull] private float[] dry = new float[0];
        private volatile bool retiring;
        [CanBeNull] private IReadOnlyList<Effect> lastChain;
        private long lastEnabledMask = -1;
        private float level = 1f;

        public TrackBus([NotNull] Track track, int sampleRate, [NotNull] Func<double> beatsPerMinute)
        {
            this.track = track;
            this.beatsPerMinute = beatsPerMinute;
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);
            voices = new MixingSampleProvider(WaveFormat) { ReadFully = true };
            fadeStep = (float) (1 / (CrossfadeSeconds * sampleRate));
        }

        public WaveFormat WaveFormat { get; }

        [NotNull]
        public Track Track => track;

        public int VoiceCount => voices.MixerInputs.Count();

        public void AddVoice([NotNull] ISampleProvider voice)
        {
            voices.AddMixerInput(voice);
        }

        /// <summary>
        /// Fades the bus out, after which it finishes and leaves the mixer: for a track no longer in the song.
        /// </summary>
        public void Retire()
        {
            retiring = true;
        }

        public int Read(Span<float> buffer)
        {
            voices.Read(buffer);
            UpdateSlots();

            if (dry.Length < buffer.Length)
            {
                dry = new float[buffer.Length];
            }

            foreach (Slot slot in slots)
            {
                Span<float> before = dry.AsSpan(0, buffer.Length);
                bool fading = slot.Fade < 1 || !slot.Wanted;

                if (fading)
                {
                    buffer.CopyTo(before);
                }

                slot.Processor.Process(buffer);

                if (fading)
                {
                    // Blend between the untouched and the processed sound as the effect fades in or out.
                    float direction = slot.Wanted ? fadeStep : -fadeStep;

                    for (var n = 0; n < buffer.Length; n++)
                    {
                        slot.Fade = Math.Clamp(slot.Fade + direction, 0f, 1f);
                        buffer[n] = before[n] + (buffer[n] - before[n]) * slot.Fade;
                    }
                }
            }

            slots.RemoveAll(slot => !slot.Wanted && slot.Fade <= 0);

            if (retiring)
            {
                for (var n = 0; n < buffer.Length; n++)
                {
                    level = Math.Max(0, level - fadeStep);
                    buffer[n] *= level;
                }

                if (level <= 0)
                {
                    return 0;
                }
            }

            return buffer.Length;
        }

        /// <summary>
        /// Lines the slots up with the track's chain: new or re-enabled effects start fading in, removed or
        /// bypassed ones fade out, and the order follows the track's.
        /// </summary>
        private void UpdateSlots()
        {
            IReadOnlyList<Effect> chain = track.Effects;
            long enabledMask = 0;

            for (var i = 0; i < chain.Count; i++)
            {
                enabledMask |= chain[i].IsEnabled ? 1L << (i % 64) : 0;
            }

            // Most buffers nothing has changed: skip the work (and its allocations) on the audio thread.
            if (ReferenceEquals(chain, lastChain) && enabledMask == lastEnabledMask)
            {
                return;
            }

            lastChain = chain;
            lastEnabledMask = enabledMask;

            foreach (Slot slot in slots)
            {
                slot.Wanted = slot.Processor.Effect.IsEnabled && chain.Contains(slot.Processor.Effect);
            }

            foreach (Effect effect in chain)
            {
                if (effect.IsEnabled && slots.All(slot => slot.Processor.Effect != effect || !slot.Wanted))
                {
                    // A fresh processor: one being bypassed may still hold an old tail.
                    slots.Add(new Slot(EffectProcessor.Create(effect, WaveFormat.SampleRate, beatsPerMinute)));
                }
            }

            // Wanted effects in the chain's order; ones fading out stay where they were.
            List<Slot> ordered = slots.Where(slot => slot.Wanted).OrderBy(slot => IndexIn(chain, slot.Processor.Effect)).ToList();
            var next = 0;

            for (var i = 0; i < slots.Count; i++)
            {
                if (slots[i].Wanted)
                {
                    slots[i] = ordered[next++];
                }
            }
        }

        private static int IndexIn([NotNull] IReadOnlyList<Effect> chain, [NotNull] Effect effect)
        {
            for (var i = 0; i < chain.Count; i++)
            {
                if (chain[i] == effect)
                {
                    return i;
                }
            }

            return int.MaxValue;
        }

        private sealed class Slot
        {
            public Slot([NotNull] EffectProcessor processor)
            {
                Processor = processor;
            }

            [NotNull]
            public EffectProcessor Processor { get; }

            public float Fade { get; set; }

            public bool Wanted { get; set; } = true;
        }
    }
}
