using Sequencer.ViewModel;

namespace Sequencer.View.RadialContextMenu
{
    /// <summary>
    /// Defines a note action radial context menu.
    /// </summary>
    /// <remarks>
    /// Because WPF doesn't support XAML2009 or greater, we cannot define generic controls.
    /// This class is simply to get around the problem of defining a <see cref="RadialContextMenu{TMenuItem}" />
    /// of type <see cref="NoteAction" />.
    /// </remarks>
    public class NoteActionRadialContextMenuWrapper : RadialContextMenu<NoteAction>
    {
    }
}