using System;
using Moq;

namespace Kiwi.Prevalence.Tests
{
    public class Mocks: MockRepository, IDisposable
    {
        public Mocks() : base(MockBehavior.Strict)
        {
        }
        public Mocks(MockBehavior defaultBehavior) : base(defaultBehavior)
        {
        }

        public void Dispose()
        {
            base.VerifyAll();
        }
    }
}