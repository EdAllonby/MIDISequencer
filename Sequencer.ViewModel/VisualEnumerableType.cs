using JetBrains.Annotations;
using Sequencer.Domain;

namespace Sequencer.ViewModel
{
    public abstract class VisualEnumerableType<T> : EnumerableType<T> where T : EnumerableType<T>
    {
        protected VisualEnumerableType(int value, [NotNull] string displayName, [CanBeNull] string iconName, bool canView) : base(value, displayName)
        {
            IconName = iconName;
            CanView = canView;
        }

        /// <summary>
        /// The name of the icon asset the view should show for this item, or null for no icon.
        /// </summary>
        [CanBeNull]
        public string IconName { get; }

        public bool CanView { get; }
    }
}
