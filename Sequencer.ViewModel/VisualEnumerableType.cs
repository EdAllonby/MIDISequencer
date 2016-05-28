using System.Drawing;
using Sequencer.Domain;

namespace Sequencer.ViewModel
{
    public abstract class VisualEnumerableType<T> : EnumerableType<T> where T : EnumerableType<T>
    {
        protected VisualEnumerableType(int value, string displayName, Bitmap visual) : base(value, displayName)
        {
            Visual = visual;
        }

        public Bitmap Visual { get; }
    }
}