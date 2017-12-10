using System;
using System.Drawing;
using JetBrains.Annotations;
using Sequencer.Domain;

namespace Sequencer.ViewModel
{
    public abstract class VisualEnumerableType<T> : EnumerableType<T> where T : EnumerableType<T>
    {
        protected VisualEnumerableType(int value, [NotNull] string displayName, [CanBeNull] Bitmap visual) : base(value, displayName)
        {
            Visual = visual ?? throw new ArgumentNullException(nameof(visual));
        }

        [NotNull]
        public Bitmap Visual { get; }
    }
}