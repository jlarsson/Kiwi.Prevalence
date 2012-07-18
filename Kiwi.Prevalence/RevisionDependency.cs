namespace Kiwi.Prevalence
{
    internal class RevisionDependency<TModel> : IRevisionDependency
    {
        private readonly IRepository<TModel> _owner;
        private readonly long _revision;

        public RevisionDependency(IRepository<TModel> owner)
        {
            _owner = owner;
            _revision = _owner.Revision;
        }

        public bool IsValid { get; private set; }

        public IRevisionDependency Renew()
        {
            if (_revision != _owner.Revision)
            {
                return _owner.RevisionDependency;
            }
            return this;
        }
    }
}