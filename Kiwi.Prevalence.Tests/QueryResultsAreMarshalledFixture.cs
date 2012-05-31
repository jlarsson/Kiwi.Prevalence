using System.IO;
using Kiwi.Prevalence.Marshalling;
using NUnit.Framework;

namespace Kiwi.Prevalence.Tests
{
    [TestFixture]
    public class QueryResultsAreMarshalledFixture
    {
        public class Model
        {
        }

        public class Entity
        {
            public string Name { get; set; }
        }

        [Test]
        public void MarshallerIsCalled()
        {
            using (var mocks = new Mocks())
            {
                var marshal = mocks.Create<IMarshal>();
                var configuration = new RepositoryConfiguration
                                        {
                                            Marshal = marshal.Object
                                        };
                using (var repo = new Repository<Model>(configuration, new ModelFactory<Model>(() => new Model())))
                {
                    marshal.Setup(m => m.MarshalQueryResult("result")).Returns("marshal result");

                    var marshalledResult = repo.Query(m => "result");

                    Assert.That(marshalledResult, Is.EqualTo("marshal result"));
                }
            }
        }

        [Test]
        public void ResultIsMarshalled()
        {
            using (var repo = new Repository<Model>(new RepositoryConfiguration(),
                                                    new ModelFactory<Model>(() => new Model())))
            {
                var modelResult = new Entity {Name = "A"};
                var marshalledResult = repo.Query(m => modelResult);

                Assert.That(marshalledResult.Name, Is.EqualTo(modelResult.Name), "Inner values should equal");

                Assert.That(ReferenceEquals(modelResult, marshalledResult), Is.Not.True, "Instance should be another");
            }
        }
    }
}