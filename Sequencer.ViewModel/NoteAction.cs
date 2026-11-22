using JetBrains.Annotations;

namespace Sequencer.ViewModel
{
    public class NoteAction : VisualEnumerableType<NoteAction>
    {
        [UsedImplicitly] [NotNull] public static readonly NoteAction None = new NoteAction(0, "None", null, false);
        [UsedImplicitly] [NotNull] public static readonly NoteAction Select = new NoteAction(1, "Select", "Select", true);
        [UsedImplicitly] [NotNull] public static readonly NoteAction Create = new NoteAction(2, "Create", "Draw", true);
        [UsedImplicitly] [NotNull] public static readonly NoteAction Delete = new NoteAction(3, "Delete", "Erase", true);

        private NoteAction(int value, [NotNull] string displayName, [CanBeNull] string iconName, bool canView)
            : base(value, displayName, iconName, canView)
        {
        }
    }
}
