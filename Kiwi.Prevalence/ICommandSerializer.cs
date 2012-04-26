namespace Kiwi.Prevalence
{
    public interface ICommandSerializer
    {
        JournalCommand Serialize(ICommand command);
        ICommand Deserialize(JournalCommand command);
    }
}