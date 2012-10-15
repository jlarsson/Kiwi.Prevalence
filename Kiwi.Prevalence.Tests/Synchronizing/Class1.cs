using System;
using Kiwi.Prevalence.Concurrency;
using NUnit.Framework;

namespace Kiwi.Prevalence.Tests.Synchronizing
{
    [TestFixture]
    public class SynchronizerGetsCalled
    {
        public class Model
        {
        }

        private class SynchronizeForTest : ISynchronize
        {
            public int ReadLocks { get; set; }
            public int WriteLocks { get; set; }

            #region ISynchronize Members

            public T Read<T>(Func<T> action)
            {
                ++ReadLocks;
                var result = action();
                --ReadLocks;
                return result;
            }

            public T Write<T>(Func<T> action)
            {
                ++WriteLocks;
                var result = action();
                --WriteLocks;
                return result;
            }

            #endregion
        }
        public class Command: AbstractCommand<Model>
        {
            public Action ExecuteAction { get; set; }
            public override void Execute(Model model)
            {
                ExecuteAction();
            }
        }

        [Test]
        public void CommandsAreWriteLocked()
        {
            var synchronize = new SynchronizeForTest();

            var repoFactory = RepositoryFactory.ForModel<Model>()
                .JournalToMemory()
                .SynchronizeUsing(synchronize);
            using (var repo = repoFactory.CreateRepository())
            {
                Assert.That(synchronize.ReadLocks, Is.EqualTo(0));
                Assert.That(synchronize.WriteLocks, Is.EqualTo(0));

                var command = new Command()
                                  {
                                      ExecuteAction = () =>
                                                          {
                                                              Assert.That(synchronize.ReadLocks, Is.EqualTo(0));
                                                              Assert.That(synchronize.WriteLocks, Is.EqualTo(1));
                                                          }
                                  };

                repo.Execute(command);

                Assert.That(synchronize.ReadLocks, Is.EqualTo(0));
                Assert.That(synchronize.WriteLocks, Is.EqualTo(0));
            }
        }

        [Test]
        public void QueriesAreReadLocked()
        {
            var synchronize = new SynchronizeForTest();

            var repoFactory = RepositoryFactory.ForModel<Model>()
                .JournalToMemory()
                .SynchronizeUsing(synchronize);
            using (var repo = repoFactory.CreateRepository())
            {
                Assert.That(synchronize.ReadLocks, Is.EqualTo(0));
                Assert.That(synchronize.WriteLocks, Is.EqualTo(0));
                repo.Query(m =>
                               {
                                   Assert.That(synchronize.ReadLocks, Is.EqualTo(1));
                                   Assert.That(synchronize.WriteLocks, Is.EqualTo(0));
                                   return 0;
                               });
                Assert.That(synchronize.ReadLocks, Is.EqualTo(0));
                Assert.That(synchronize.WriteLocks, Is.EqualTo(0));
            }
        }
    }
}