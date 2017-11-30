using Sequencer.Domain;

namespace Sequencer.View
{
    public interface IVisualNoteFactory
    {
        IVisualNote CreateNote(Pitch pitch, IPosition start, IPosition end);
    }
}