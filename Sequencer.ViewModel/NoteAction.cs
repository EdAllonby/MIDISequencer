using System;
using System.Drawing;
using JetBrains.Annotations;
using Sequencer.ViewModel.Properties;

namespace Sequencer.ViewModel
{
    public class NoteAction : VisualEnumerableType<NoteAction>
    {
        [UsedImplicitly] [NotNull] public static readonly NoteAction Select = new NoteAction(1, "Select", Resources.Move ?? throw new InvalidOperationException());
        [UsedImplicitly] [NotNull] public static readonly NoteAction Create = new NoteAction(2, "Create", Resources.Create ?? throw new InvalidOperationException());
        [UsedImplicitly] [NotNull] public static readonly NoteAction Delete = new NoteAction(3, "Delete", Resources.Delete ?? throw new InvalidOperationException());

        private NoteAction(int value, [NotNull] string displayName, [NotNull] Bitmap visual) : base(value, displayName, visual)
        {
        }
    }
}