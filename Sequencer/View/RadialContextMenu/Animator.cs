using System;
using System.Windows.Media.Animation;

namespace Sequencer.View.RadialContextMenu
{
    /// <summary>
    /// Helper for WPF animations.
    /// </summary>
    public static class Animator
    {
        /// <summary>
        /// Create a fade out animation.
        /// </summary>
        /// <param name="fadeOutDuration">How long the fade out animation should last, in milliseconds.</param>
        /// <returns>A fade out animation.</returns>
        public static DoubleAnimation CreateFadeOutAnimation(int fadeOutDuration)
        {
            return new DoubleAnimation
            {
                To = 0,
                BeginTime = TimeSpan.FromSeconds(0),
                Duration = TimeSpan.FromMilliseconds(fadeOutDuration),
                FillBehavior = FillBehavior.Stop
            };
        }
    }
}