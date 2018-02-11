using System.Drawing;
using JetBrains.Annotations;
using Sequencer.ViewModel.Properties;

namespace Sequencer.ViewModel
{
    public class NoteAction : VisualEnumerableType<NoteAction>
    {
        [UsedImplicitly] [NotNull] public static readonly NoteAction None = new NoteAction(0, "None", null, false);
        [UsedImplicitly] [NotNull] public static readonly NoteAction Select = new NoteAction(1, "Select", Resources.Move, true);
        [UsedImplicitly] [NotNull] public static readonly NoteAction Create = new NoteAction(2, "Create", Resources.Create, true);
        [UsedImplicitly] [NotNull] public static readonly NoteAction Delete = new NoteAction(3, "Delete", Resources.Delete, true);

        private NoteAction(int value, [NotNull] string displayName, [CanBeNull] Bitmap visual, bool canView)
            : base(value, displayName, visual, canView)
        {
        }
    }
}