using System.Drawing;
using Sequencer.ViewModel.Properties;

namespace Sequencer.ViewModel
{
    public class NoteAction : VisualEnumerableType<NoteAction>
    {
        [UsedImplicitly] public static readonly NoteAction Select = new NoteAction(1, "Select", Resources.Move);
        [UsedImplicitly] public static readonly NoteAction Create = new NoteAction(2, "Create", Resources.Create);
        [UsedImplicitly] public static readonly NoteAction Delete = new NoteAction(3, "Delete", Resources.Delete);

        private NoteAction(int value, string displayName, Bitmap visual) : base(value, displayName, visual)
        {
        }
    }
}