using Kiwi.Prevalence.Marshalling;
using NUnit.Framework;

namespace Kiwi.Prevalence.Tests.Marshalling
{
    [TestFixture]
    public class ResultsAreMarshalled
    {
        public class Model
        {
        }

        public class Command : AbstractCommand<Model, string>
        {
            public string ReturnValue { get; set; }

            public override string Execute(Model model)
            {
                return ReturnValue;
            }
        }

        [Test]
        public void MarshalCommandResult()
        {
            using (var mocks = new Mocks())
            {
                var marshal = mocks.Create<IMarshal>();
                var repoFactory = RepositoryFactory.ForModel<Model>().JournalToMemory().MarshalUsing(marshal.Object);
                using (var repo = repoFactory.CreateRepository())
                {
                    marshal.Setup(m => m.MarshalCommandResult("command result")).Returns("marshal command result");

                    var marshalledResult = repo.Execute(new Command {ReturnValue = "command result"});

                    Assert.That(marshalledResult, Is.EqualTo("marshal command result"));
                }
            }
        }

        [Test]
        public void MarshalQueryResult()
        {
            using (var mocks = new Mocks())
            {
                var marshal = mocks.Create<IMarshal>();
                var repoFactory = RepositoryFactory.ForModel<Model>().JournalToMemory().MarshalUsing(marshal.Object);
                using (var repo = repoFactory.CreateRepository())
                {
                    marshal.Setup(m => m.MarshalQueryResult("result")).Returns("marshal result");

                    var marshalledResult = repo.Query(m => "result");

                    Assert.That(marshalledResult, Is.EqualTo("marshal result"));
                }
            }
        }
    }
}