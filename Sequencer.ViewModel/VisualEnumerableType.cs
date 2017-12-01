using System.Drawing;
using JetBrains.Annotations;
using Sequencer.Domain;

namespace Sequencer.ViewModel
{
    public abstract class VisualEnumerableType<T> : EnumerableType<T> where T : EnumerableType<T>
    {
        protected VisualEnumerableType(int value, [NotNull] string displayName, [NotNull] Bitmap visual) : base(value, displayName)
        {
            Visual = visual;
        }

        [NotNull]
        public Bitmap Visual { get; }
    }
}