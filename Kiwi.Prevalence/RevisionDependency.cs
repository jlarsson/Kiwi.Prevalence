namespace Kiwi.Prevalence
{
    internal class RevisionDependency<TModel> : IRevisionDependency
    {
        private readonly IRepository<TModel> _owner;

        public RevisionDependency(IRepository<TModel> owner)
        {
            IsValid = true;
            _owner = owner;
        }

        public bool IsValid { get; set; }

        public IRevisionDependency Renew()
        {
            return IsValid ? this : _owner.RevisionDependency;
        }
    }
}