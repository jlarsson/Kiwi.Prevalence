namespace Kiwi.Prevalence
{
    public interface IRevisionDependency
    {
        bool IsValid { get; }
        IRevisionDependency Renew();
    }
}