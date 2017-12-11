using System.Drawing;
using JetBrains.Annotations;
using Sequencer.ViewModel.Properties;

namespace Sequencer.ViewModel
{
    public class NoteAction : VisualEnumerableType<NoteAction>
    {
        [UsedImplicitly] [NotNull] public static readonly NoteAction Select = new NoteAction(1, "Select", Resources.Move);
        [UsedImplicitly] [NotNull] public static readonly NoteAction Create = new NoteAction(2, "Create", Resources.Create);
        [UsedImplicitly] [NotNull] public static readonly NoteAction Delete = new NoteAction(3, "Delete", Resources.Delete);

        private NoteAction(int value, [NotNull] string displayName, [CanBeNull] Bitmap visual) : base(value, displayName, visual)
        {
        }
    }
}