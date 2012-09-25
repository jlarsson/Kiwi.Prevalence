using Kiwi.Prevalence.Journaling.Memory;
using NUnit.Framework;

namespace Kiwi.Prevalence.Tests.HowToTestWithMemoryJournal
{
    [TestFixture]
    public class RecipeForTestingWithMemoryJournal
    {
        public class Model
        {
            public int CommandCount { get; set; }
        }

        public class Command : AbstractCommand<Model>
        {
            public override void Execute(Model model)
            {
                ++model.CommandCount;
            }
        }

        [Test]
        public void HowToVerifySnapshotIsTaken()
        {
            var sharedJournalData = new MemoryJournalData();

            // Define how reposity is setup
            var factory = RepositoryFactory
                .ForModel<Model>()
                .JournalToMemory(sharedJournalData)
                .SynchronizeNone();

            using (var repo = factory.CreateRepository())
            {
                // Assert that repo is completely untouched
                Assert.That(sharedJournalData.JsonLog.Count, Is.EqualTo(0));
                Assert.That(sharedJournalData.JsonSnapshot, Is.EqualTo(null));
                Assert.That(repo.Query(m => m.CommandCount), Is.EqualTo(0));

                // Execute some commands and very that log and model is updated accordingly
                repo.Execute(new Command());
                repo.Execute(new Command());
                Assert.That(sharedJournalData.JsonLog.Count, Is.EqualTo(2));
                Assert.That(sharedJournalData.JsonSnapshot, Is.Null);
                Assert.That(repo.Query(m => m.CommandCount), Is.EqualTo(2));

                // Take a snapshot, ie. purge the log
                repo.SaveSnapshot();
                Assert.That(sharedJournalData.JsonLog.Count, Is.EqualTo(0));
                Assert.That(sharedJournalData.JsonSnapshot, Is.Not.Null);
                Assert.That(repo.Query(m => m.CommandCount), Is.EqualTo(2));

                // Simulate offline time by closing this repo
            }

            using (var repo = factory.CreateRepository())
            {
                // Open repo using the shared memory state
                // It should now have empty log, a snapshot and the model should be as we left it
                Assert.That(sharedJournalData.JsonLog.Count, Is.EqualTo(0), "Log should be empty after snapshot");
                Assert.That(sharedJournalData.JsonSnapshot, Is.Not.Null, "Missing snapshot");
                Assert.That(repo.Query(m => m.CommandCount), Is.EqualTo(2));
            }
        }
    }
}
