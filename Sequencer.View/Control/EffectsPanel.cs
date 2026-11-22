using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using JetBrains.Annotations;
using Sequencer.Domain.Arrangement;
using Sequencer.Domain.Effects;
using Effect = Sequencer.Domain.Effects.Effect;
using Track = Sequencer.Domain.Arrangement.Track;

namespace Sequencer.View.Control
{
    /// <summary>
    /// The selected track's effects, in signal order: add one from a menu, switch it off (bypass), move it up or
    /// down the chain, remove it, and set its parameters. Double-click a slider to reset it. Built from each
    /// effect's parameter list, so a new effect needs no new UI.
    /// </summary>
    public sealed class EffectsPanel : UserControl
    {
        private const double SliderSteps = 1000;

        [NotNull] private readonly StackPanel cards = new StackPanel { Spacing = 8 };
        [NotNull] private readonly TextBlock emptyHint;
        [NotNull] private readonly Button addButton;
        [CanBeNull] private Track track;
        [CanBeNull] private IReadOnlyList<Effect> shownChain;
        private bool syncing;

        public EffectsPanel()
        {
            addButton = new Button { Name = "AddEffectButton", Focusable = false };
            addButton.Classes.Add("quiet");
            ToolTip.SetTip(addButton, "Add an effect to this track");
            addButton.Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                Children =
                {
                    WithStroke(StrokeIcon(Icons.Plus, 10), "TextSecondaryBrush"),
                    new TextBlock { Text = "Add", VerticalAlignment = VerticalAlignment.Center }
                }
            };
            addButton.Flyout = BuildAddMenu();

            emptyHint = new TextBlock
            {
                Text = "None yet. Add compression, EQ, drive, chorus, delay or reverb; the sound passes through them top to bottom.",
                TextWrapping = TextWrapping.Wrap
            };
            emptyHint.Classes.Add("label");
            emptyHint.Foreground = Brush("TextTertiaryBrush");

            var title = new TextBlock { Text = "EFFECTS", VerticalAlignment = VerticalAlignment.Center };
            title.Classes.Add("section");
            var header = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
            header.Children.Add(title);
            Grid.SetColumn(addButton, 1);
            header.Children.Add(addButton);

            Content = new StackPanel { Spacing = 10, Children = { header, emptyHint, cards } };
        }

        /// <summary>
        /// Raised when the user changes the effects, so the song can be marked edited.
        /// </summary>
        public event EventHandler Edited;

        /// <summary>
        /// The track whose effects are shown.
        /// </summary>
        [CanBeNull]
        public Track Track
        {
            get => track;
            set
            {
                if (track == value)
                {
                    return;
                }

                if (track != null)
                {
                    track.EffectsChanged -= OnEffectsChanged;
                }

                track = value;

                if (track != null)
                {
                    track.EffectsChanged += OnEffectsChanged;
                }

                Rebuild();
            }
        }

        /// <summary>
        /// Adds an effect to the end of the chain, as the Add menu does.
        /// </summary>
        public void Add(EffectKind kind)
        {
            if (track == null)
            {
                return;
            }

            track.AddEffect(new Effect(kind));
            Edited?.Invoke(this, EventArgs.Empty);
        }

        private void OnEffectsChanged(object sender, EventArgs e)
        {
            // Effects change on the UI thread, but be safe.
            Dispatcher.UIThread.Post(() =>
            {
                if (track == null || ReferenceEquals(track.Effects, shownChain))
                {
                    SyncValues();
                }
                else
                {
                    Rebuild();
                }
            });
        }

        [NotNull]
        private MenuFlyout BuildAddMenu()
        {
            var menu = new MenuFlyout { Placement = PlacementMode.BottomEdgeAlignedRight };

            foreach (EffectKind kind in EffectCatalogue.All)
            {
                var item = new MenuItem
                {
                    Header = new StackPanel
                    {
                        Spacing = 1,
                        Children =
                        {
                            new TextBlock { Text = EffectCatalogue.NameOf(kind) },
                            new TextBlock { Text = EffectCatalogue.DescriptionOf(kind), FontSize = 11, Foreground = Brush("TextTertiaryBrush") }
                        }
                    }
                };
                item.Click += (sender, args) => Add(kind);
                menu.Items.Add(item);
            }

            return menu;
        }

        private void Rebuild()
        {
            cards.Children.Clear();
            shownChain = track?.Effects;
            IReadOnlyList<Effect> chain = shownChain ?? Array.Empty<Effect>();
            emptyHint.IsVisible = chain.Count == 0;
            addButton.IsEnabled = track != null;

            for (var i = 0; i < chain.Count; i++)
            {
                cards.Children.Add(BuildCard(chain[i], i, chain.Count));
            }
        }

        [NotNull]
        private Avalonia.Controls.Control BuildCard([NotNull] Effect effect, int index, int count)
        {
            var power = new ToggleButton
            {
                Name = $"Power{index}",
                IsChecked = effect.IsEnabled,
                Focusable = false,
                Width = 26,
                Height = 26,
                Padding = new Thickness(0),
                CornerRadius = new CornerRadius(13),
                Content = StrokeIcon(Icons.Power, 13)
            };
            power.Classes.Add("effect-power");
            ToolTip.SetTip(power, "On or bypassed");
            power.IsCheckedChanged += (sender, args) =>
            {
                if (!syncing)
                {
                    effect.IsEnabled = power.IsChecked == true;
                    Edited?.Invoke(this, EventArgs.Empty);
                }
            };

            var name = new TextBlock { Text = effect.Name, FontWeight = FontWeight.SemiBold, VerticalAlignment = VerticalAlignment.Center };
            Button up = SmallButton(Icons.ChevronUp, "Earlier in the chain", index > 0, () => Move(effect, -1));
            Button down = SmallButton(Icons.ChevronDown, "Later in the chain", index < count - 1, () => Move(effect, 1));
            Button remove = SmallButton(Icons.Close, $"Remove {effect.Name}", true, () => Remove(effect));
            remove.Name = $"RemoveEffect{index}";

            var header = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto,Auto,Auto"), ColumnSpacing = 4 };
            header.Children.Add(power);
            Grid.SetColumn(name, 1);
            name.Margin = new Thickness(4, 0, 0, 0);
            header.Children.Add(name);
            Grid.SetColumn(up, 2);
            header.Children.Add(up);
            Grid.SetColumn(down, 3);
            header.Children.Add(down);
            Grid.SetColumn(remove, 4);
            header.Children.Add(remove);

            var parameters = new StackPanel { Spacing = 6, Margin = new Thickness(0, 6, 0, 0) };

            foreach (EffectParameter parameter in effect.Parameters)
            {
                parameters.Children.Add(parameter.Unit == ParameterUnit.Choice ? ChoiceRow(effect, parameter) : SliderRow(effect, parameter));
            }

            parameters.Opacity = effect.IsEnabled ? 1 : 0.45;
            power.IsCheckedChanged += (sender, args) => parameters.Opacity = power.IsChecked == true ? 1 : 0.45;

            return new Border
            {
                Name = $"Effect{index}",
                Tag = effect,
                Background = Brush("FieldBrush"),
                BorderBrush = Brush("HairlineBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(8, 6, 10, 10),
                Child = new StackPanel { Children = { header, parameters } }
            };
        }

        [NotNull]
        private Avalonia.Controls.Control SliderRow([NotNull] Effect effect, [NotNull] EffectParameter parameter)
        {
            var label = new TextBlock { Text = parameter.Name };
            label.Classes.Add("label");
            var value = new TextBlock { Text = parameter.Format(effect[parameter.Key]) };
            value.Classes.Add("value");
            var labels = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
            labels.Children.Add(label);
            Grid.SetColumn(value, 1);
            labels.Children.Add(value);

            var slider = new Slider
            {
                Name = $"{effect.Kind}{parameter.Name.Replace(" ", string.Empty)}Slider",
                Minimum = 0,
                Maximum = SliderSteps,
                Value = parameter.ToPosition(effect[parameter.Key]) * SliderSteps,
                Focusable = false,
                Margin = new Thickness(0, -6, 0, -4),
                Tag = parameter
            };
            ToolTip.SetTip(slider, "Double-click to reset");

            slider.ValueChanged += (sender, args) =>
            {
                if (syncing)
                {
                    return;
                }

                effect[parameter.Key] = parameter.FromPosition(slider.Value / SliderSteps);
                value.Text = parameter.Format(effect[parameter.Key]);
                Edited?.Invoke(this, EventArgs.Empty);
            };
            slider.DoubleTapped += (sender, args) => slider.Value = parameter.ToPosition(parameter.Default) * SliderSteps;

            return new StackPanel { Spacing = 2, Tag = (Action) Sync, Children = { labels, slider } };

            void Sync()
            {
                slider.Value = parameter.ToPosition(effect[parameter.Key]) * SliderSteps;
                value.Text = parameter.Format(effect[parameter.Key]);
            }
        }

        [NotNull]
        private Avalonia.Controls.Control ChoiceRow([NotNull] Effect effect, [NotNull] EffectParameter parameter)
        {
            var label = new TextBlock { Text = parameter.Name, VerticalAlignment = VerticalAlignment.Center };
            label.Classes.Add("label");
            var choices = new ComboBox
            {
                Name = $"{effect.Kind}{parameter.Name.Replace(" ", string.Empty)}Choice",
                ItemsSource = parameter.Choices,
                SelectedIndex = (int) effect[parameter.Key],
                Focusable = false,
                FontSize = 12,
                MinHeight = 26,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            ToolTip.SetTip(choices, "In time with the tempo");
            choices.SelectionChanged += (sender, args) =>
            {
                if (!syncing && choices.SelectedIndex >= 0)
                {
                    effect[parameter.Key] = choices.SelectedIndex;
                    Edited?.Invoke(this, EventArgs.Empty);
                }
            };

            var row = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), Tag = (Action) (() => choices.SelectedIndex = (int) effect[parameter.Key]) };
            row.Children.Add(label);
            Grid.SetColumn(choices, 1);
            row.Children.Add(choices);
            return row;
        }

        /// <summary>
        /// Shows values changed elsewhere (by undo, say) without rebuilding the cards.
        /// </summary>
        private void SyncValues()
        {
            syncing = true;

            try
            {
                foreach (Border card in cards.Children.OfType<Border>())
                {
                    var effect = (Effect) card.Tag;
                    var body = (StackPanel) card.Child;
                    var header = (Grid) body.Children[0];
                    ((ToggleButton) header.Children[0]).IsChecked = effect?.IsEnabled;

                    foreach (Avalonia.Controls.Control row in ((StackPanel) body.Children[1]).Children)
                    {
                        (row.Tag as Action)?.Invoke();
                    }
                }
            }
            finally
            {
                syncing = false;
            }
        }

        private void Move([NotNull] Effect effect, int offset)
        {
            track?.MoveEffect(effect, offset);
            Edited?.Invoke(this, EventArgs.Empty);
        }

        private void Remove([NotNull] Effect effect)
        {
            track?.RemoveEffect(effect);
            Edited?.Invoke(this, EventArgs.Empty);
        }

        [NotNull]
        private static Button SmallButton([NotNull] Geometry icon, [NotNull] string tip, bool enabled, [NotNull] Action action)
        {
            Avalonia.Controls.Shapes.Path path = StrokeIcon(icon, 10);
            path.Stroke = Brush("TextSecondaryBrush");
            var button = new Button
            {
                Width = 24,
                Height = 24,
                Padding = new Thickness(0),
                Focusable = false,
                IsEnabled = enabled,
                Content = path
            };
            button.Classes.Add("icon");
            ToolTip.SetTip(button, tip);
            button.Click += (sender, args) => action();
            return button;
        }

        [NotNull]
        private static Avalonia.Controls.Shapes.Path StrokeIcon([NotNull] Geometry data, double size)
        {
            var path = new Avalonia.Controls.Shapes.Path { Data = data, Width = size, Height = size, Stretch = Stretch.Uniform };
            path.Classes.Add("stroke-icon");
            return path;
        }

        [NotNull]
        private static Avalonia.Controls.Shapes.Path WithStroke([NotNull] Avalonia.Controls.Shapes.Path path, [NotNull] string brush)
        {
            path.Stroke = Brush(brush);
            return path;
        }

        [CanBeNull]
        private static IBrush Brush([NotNull] string key) =>
            Application.Current?.TryFindResource(key, out object brush) == true ? brush as IBrush : null;
    }
}
