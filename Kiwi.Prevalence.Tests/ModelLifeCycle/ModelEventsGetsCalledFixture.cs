using NUnit.Framework;

namespace Kiwi.Prevalence.Tests.ModelLifeCycle
{
    [TestFixture]
    public class ModelEventsGetsCalled
    {
        public class Model
        {
        }

        [Test]
        public void Test()
        {
            using (var mocks = new Mocks())
            {
                var events = mocks.Create<IModelEvents>();

                using (var repo = RepositoryFactory
                    .ForModel<Model>()
                    .WithEvents(m => events.Object)
                    .JournalToMemory().CreateRepository())
                {
                    // Setup expected call sequence
                    var eventId = 0;
                    events.Setup(e => e.BeginRestore()).Callback(() => Assert.AreEqual(0, eventId++));
                    events.Setup(e => e.BeginRestoreSnapshot()).Callback(() => Assert.AreEqual(1, eventId++));
                    events.Setup(e => e.EndRestoreSnapshot()).Callback(() => Assert.AreEqual(2, eventId++));
                    events.Setup(e => e.BeginRestoreJournal()).Callback(() => Assert.AreEqual(3, eventId++));
                    events.Setup(e => e.EndRestoreJournal()).Callback(() => Assert.AreEqual(4, eventId++));
                    events.Setup(e => e.EndRestore()).Callback(() => Assert.AreEqual(5, eventId++));

                    // Trigger initialization by querying
                    repo.Query(m => 0);
                }
            }
        }
    }
}
