using NUnit.Framework;

namespace Kiwi.Prevalence.Tests
{
    [TestFixture]
    public class ModelIsCreatedBeforeAnyQuery
    {
        private class Model
        {
        }
        [Test]
        public void Test()
        {
            var modelIsCreated = false;
            using (var repo = new Repository<Model>(() => { modelIsCreated = true; return new Model(); }))
            {
                repo.Query(model =>
                               {
                                   Assert.That(model, Is.Not.Null);
                                   Assert.That(modelIsCreated, Is.True);
                                   return 0;
                               });
            }
        }
    }
}