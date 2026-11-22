using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Sequencer.Domain.Drums;
using Sequencer.Domain.Effects;
using Sequencer.Domain.Settings;

namespace Sequencer.Domain.Arrangement
{
    /// <summary>
    /// Saves and loads songs as readable JSON: the tempo, each track's instrument and clips, and which clip
    /// plays in each section. Note positions are in ticks (96 per quarter note).
    /// </summary>
    public static class SongFile
    {
        public const string Extension = "sequencer";
        public const int CurrentVersion = 1;
        private const string FormatName = "sequencer-song";
        private const int TicksPerQuarterNote = 96;

        [NotNull] private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };

        public static void Save([NotNull] Song song, int beatsPerMinute, [NotNull] Stream stream)
        {
            var file = new SongDto
            {
                Format = FormatName,
                Version = CurrentVersion,
                BeatsPerMinute = beatsPerMinute,
                Tracks = song.Tracks.Select(ToDto).ToList()
            };

            JsonSerializer.Serialize(stream, file, Options);
        }

        /// <summary>
        /// Reads a song. Throws <see cref="FormatException"/> if the file isn't a song this version can read.
        /// </summary>
        [NotNull]
        public static LoadedSong Load([NotNull] Stream stream)
        {
            SongDto file;

            try
            {
                file = JsonSerializer.Deserialize<SongDto>(stream, Options);
            }
            catch (JsonException error)
            {
                throw new FormatException("This file isn't a readable song.", error);
            }

            if (file == null || file.Format != FormatName)
            {
                throw new FormatException("This file isn't a Sequencer song.");
            }

            if (file.Version > CurrentVersion)
            {
                throw new FormatException("This song was saved by a newer version of Sequencer.");
            }

            var song = new Song();
            var tracks = new List<Track>();

            foreach (TrackDto dto in file.Tracks ?? new List<TrackDto>())
            {
                tracks.Add(FromDto(dto, song.SectionCount));
            }

            song.ReplaceTracks(tracks);
            return new LoadedSong(song, Math.Clamp(file.BeatsPerMinute, 30, 300));
        }

        [NotNull]
        private static TrackDto ToDto([NotNull] Track track)
        {
            return new TrackDto
            {
                Kind = track.Kind,
                Name = track.Name,
                Colour = track.ColourIndex,
                Muted = track.IsMuted,
                Synth = track.Synth,
                DrumVolume = track.Drums?.Volume,
                Effects = track.Effects.Count == 0 ? null : track.Effects.Select(effect => new EffectDto
                {
                    Kind = effect.Kind,
                    Enabled = effect.IsEnabled,
                    Parameters = effect.Parameters.ToDictionary(parameter => parameter.Key, parameter => effect[parameter.Key])
                }).ToList(),
                Clips = track.Clips.Select(ToDto).ToList(),
                Sections = Enumerable.Range(0, track.SectionCount).Select(section => track.ClipAt(section)?.Name).ToList()
            };
        }

        [NotNull]
        private static ClipDto ToDto([NotNull] Clip clip)
        {
            switch (clip)
            {
                case NoteClip notes:
                    return new ClipDto
                    {
                        Name = clip.Name,
                        Notes = notes.Tones.Select(tone => new NoteDto
                        {
                            Pitch = tone.Pitch.ToString(),
                            Velocity = tone.Velocity.Value,
                            Start = tone.StartPosition.TotalTicks(TimeSignature.FourFour, TicksPerQuarterNote),
                            Length = tone.EndPosition.TotalTicks(TimeSignature.FourFour, TicksPerQuarterNote) -
                                     tone.StartPosition.TotalTicks(TimeSignature.FourFour, TicksPerQuarterNote)
                        }).ToList()
                    };
                case DrumClip drums:
                    return new ClipDto
                    {
                        Name = clip.Name,
                        Steps = DrumPattern.AllSounds.Where(sound => Enumerable.Range(0, DrumPattern.StepCount).Any(step => drums.Pattern.IsHit(sound, step)))
                            .ToDictionary(sound => sound.ToString(), sound => StepsToText(drums.Pattern, sound))
                    };
                default:
                    throw new ArgumentException($"Unknown clip type {clip.GetType().Name}.", nameof(clip));
            }
        }

        [NotNull]
        private static Track FromDto([NotNull] TrackDto dto, int sectionCount)
        {
            var track = new Track(dto.Kind, dto.Name ?? dto.Kind.ToString(), dto.Colour, sectionCount) { IsMuted = dto.Muted };

            if (track.Synth != null && dto.Synth != null)
            {
                track.Synth.CopyFrom(dto.Synth);
            }

            if (track.Drums != null && dto.DrumVolume != null)
            {
                track.Drums.Volume = dto.DrumVolume.Value;
            }

            foreach (EffectDto effectDto in dto.Effects ?? new List<EffectDto>())
            {
                var effect = new Effect(effectDto.Kind) { IsEnabled = effectDto.Enabled };

                foreach (KeyValuePair<string, double> parameter in effectDto.Parameters ?? new Dictionary<string, double>())
                {
                    if (effect.Has(parameter.Key))
                    {
                        effect[parameter.Key] = parameter.Value;
                    }
                }

                track.AddEffect(effect);
            }

            var clips = new Dictionary<string, Clip>();

            foreach (ClipDto clipDto in dto.Clips ?? new List<ClipDto>())
            {
                Clip clip = track.Kind == TrackKind.Synth ? (Clip) ReadNoteClip(track, clipDto) : ReadDrumClip(track, clipDto);
                track.AddClip(clip);
                clips[clip.Name] = clip;
            }

            List<string> sections = dto.Sections ?? new List<string>();

            for (var section = 0; section < Math.Min(sectionCount, sections.Count); section++)
            {
                if (sections[section] != null && clips.TryGetValue(sections[section], out Clip clip))
                {
                    track.Place(section, clip);
                }
            }

            return track;
        }

        [NotNull]
        private static NoteClip ReadNoteClip([NotNull] Track track, [NotNull] ClipDto dto)
        {
            var clip = new NoteClip(track, dto.Name ?? "A");
            clip.SetTones((dto.Notes ?? new List<NoteDto>()).Select(note => new Tone(
                ParsePitch(note.Pitch),
                new Velocity(note.Velocity),
                Position.PositionFromTick(note.Start, TimeSignature.FourFour, TicksPerQuarterNote),
                Position.PositionFromTick(note.Start + Math.Max(1, note.Length), TimeSignature.FourFour, TicksPerQuarterNote))));
            return clip;
        }

        [NotNull]
        private static DrumClip ReadDrumClip([NotNull] Track track, [NotNull] ClipDto dto)
        {
            var clip = new DrumClip(track, dto.Name ?? "A");

            foreach (KeyValuePair<string, string> lane in dto.Steps ?? new Dictionary<string, string>())
            {
                if (!Enum.TryParse(lane.Key, out DrumSound sound))
                {
                    continue;
                }

                for (var step = 0; step < Math.Min(DrumPattern.StepCount, lane.Value.Length); step++)
                {
                    char cell = lane.Value[step];
                    byte velocity = cell == 'X' ? DrumPattern.AccentVelocity : cell == 'x' ? DrumPattern.NormalVelocity : (byte) 0;

                    if (velocity > 0)
                    {
                        clip.Pattern.SetVelocity(sound, step, velocity);
                    }
                }
            }

            return clip;
        }

        [NotNull]
        private static string StepsToText([NotNull] DrumPattern pattern, DrumSound sound)
        {
            return new string(Enumerable.Range(0, DrumPattern.StepCount)
                .Select(step => pattern.VelocityAt(sound, step) is var velocity && velocity >= DrumPattern.AccentVelocity ? 'X' : velocity > 0 ? 'x' : '.')
                .ToArray());
        }

        [NotNull]
        private static Pitch ParsePitch([NotNull] string text)
        {
            int split = text.IndexOfAny("-0123456789".ToCharArray());

            if (split <= 0 || !int.TryParse(text.Substring(split), out int octave))
            {
                throw new FormatException($"\"{text}\" isn't a pitch.");
            }

            return new Pitch(Note.FromDisplayName(text.Substring(0, split)), octave);
        }

        private sealed class SongDto
        {
            public string Format { get; set; }

            public int Version { get; set; }

            public int BeatsPerMinute { get; set; } = 120;

            public List<TrackDto> Tracks { get; set; }
        }

        private sealed class TrackDto
        {
            public TrackKind Kind { get; set; }

            public string Name { get; set; }

            public int Colour { get; set; }

            public bool Muted { get; set; }

            public SynthSettings Synth { get; set; }

            public double? DrumVolume { get; set; }

            public List<EffectDto> Effects { get; set; }

            public List<ClipDto> Clips { get; set; }

            public List<string> Sections { get; set; }
        }

        private sealed class EffectDto
        {
            public EffectKind Kind { get; set; }

            public bool Enabled { get; set; } = true;

            public Dictionary<string, double> Parameters { get; set; }
        }

        private sealed class ClipDto
        {
            public string Name { get; set; }

            public List<NoteDto> Notes { get; set; }

            public Dictionary<string, string> Steps { get; set; }
        }

        private sealed class NoteDto
        {
            public string Pitch { get; set; }

            public int Velocity { get; set; }

            public int Start { get; set; }

            public int Length { get; set; }
        }
    }

    /// <summary>
    /// A song read from a file, with its tempo.
    /// </summary>
    public sealed class LoadedSong
    {
        public LoadedSong([NotNull] Song song, int beatsPerMinute)
        {
            Song = song;
            BeatsPerMinute = beatsPerMinute;
        }

        [NotNull]
        public Song Song { get; }

        public int BeatsPerMinute { get; }
    }
}
