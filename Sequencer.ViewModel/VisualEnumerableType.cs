using System.Drawing;
using JetBrains.Annotations;
using Sequencer.Domain;

namespace Sequencer.ViewModel
{
    public abstract class VisualEnumerableType<T> : EnumerableType<T> where T : EnumerableType<T>
    {
        protected VisualEnumerableType(int value, [NotNull] string displayName, [CanBeNull] Bitmap visual, bool canView) : base(value, displayName)
        {
            Visual = visual;
            CanView = canView;
        }

        [CanBeNull]
        public Bitmap Visual { get; }

        public bool CanView { get; }
    }
}