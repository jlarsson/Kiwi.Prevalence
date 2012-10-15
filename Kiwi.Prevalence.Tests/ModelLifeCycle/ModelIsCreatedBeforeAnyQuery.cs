using NUnit.Framework;

namespace Kiwi.Prevalence.Tests.ModelLifeCycle
{
    [TestFixture]
    public class ModelCreationFixture
    {
        private class Model
        {
        }

        private class Command : AbstractCommand<Model>
        {
            public override void Execute(Model model)
            {
            }
        }

        [Test]
        public void ModelIsCreatedBeforeFirstCommand()
        {
            var modelIsCreated = false;
            using (var repo = RepositoryFactory
                .ForModel(() =>
                              {
                                  modelIsCreated = true;
                                  return new Model();
                              })
                .JournalToMemory()
                .CreateRepository())
            {
                Assert.That(modelIsCreated, Is.False);

                repo.Execute(new Command());

                Assert.That(modelIsCreated, Is.True);
            }
        }

        [Test]
        public void ModelIsCreatedBeforeFirstQuery()
        {
            var modelIsCreated = false;
            using (var repo = RepositoryFactory
                .ForModel(() =>
                              {
                                  modelIsCreated = true;
                                  return new Model();
                              })
                .JournalToMemory()
                .CreateRepository())
            {
                Assert.That(modelIsCreated, Is.False);

                repo.Query(model =>
                               {
                                   Assert.That(model, Is.Not.Null);
                                   Assert.That(modelIsCreated, Is.True);
                                   return 0;
                               });

                Assert.That(modelIsCreated, Is.True);
            }
        }

        [Test]
        public void ModelIsRecreatedOnDemandAfterPurge()
        {
            var modelIsCreated = false;
            using (var repo = RepositoryFactory
                .ForModel(() =>
                {
                    modelIsCreated = true;
                    return new Model();
                })
                .JournalToMemory()
                .CreateRepository())
            {
                // Trigger model creation with dummy query
                repo.Query(m => 0);
                // Purge to drop everything
                repo.Purge();

                modelIsCreated = false;
                // Trigger model creation with dummy query
                repo.Query(m => 0);
                Assert.That(modelIsCreated, Is.True);
            }
        }
    }
}