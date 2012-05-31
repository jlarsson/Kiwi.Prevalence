using System;
using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace Kiwi.Prevalence.Tests
{
    [TestFixture]
    public class SnapshotArchiverGetsCalledFixture
    {
        private string _repoPath;

        [SetUp]
        public void Setup()
        {
            _repoPath = Path.Combine(TestUtil.CreateCleanTestFolder("test"), "testdata");
        }
        public class Model
        {
        }
        public class ModelCommand: AbstractCommand<Model,int>{
            public override Func<int> Prepare(Model model)
            {
                return () => 0;
            }
        }

        [Test]
        public void NothingToArchive()
        {
            using (var mocks = new Mocks())
            {
                var archiver = mocks.Create<ISnapshotArchiver>();
                var configuration = new RepositoryConfiguration()
                                        {
                                            SnapshotArchiver = archiver.Object
                                        };

                using (var repo = new Repository<Model>(configuration, new ModelFactory<Model>(() => new Model())){Path = _repoPath})
                {
                    archiver.Setup(a => a.Archive(It.IsAny<ISnapshotArchiveInfo>()))
                        .Callback(
                            (ISnapshotArchiveInfo info) => Assert.That(info.ArchivedFilePaths.Count(), Is.EqualTo(0)));

                    repo.SaveSnapshot();
                }
            }
        }

        [Test]
        public void ArchiveJournal()
        {
            using (var mocks = new Mocks())
            {
                var archiver = mocks.Create<ISnapshotArchiver>();
                var configuration = new RepositoryConfiguration()
                                        {
                                            SnapshotArchiver = archiver.Object
                                        };

                var expectedArchives = new[] {_repoPath + ".journal.1"};

                using (var repo = new Repository<Model>(configuration, new ModelFactory<Model>(() => new Model())) { Path = _repoPath })
                {
                    archiver.Setup(a => a.Archive(It.IsAny<ISnapshotArchiveInfo>()))
                        .Callback(
                            (ISnapshotArchiveInfo info) => Assert.That(info.ArchivedFilePaths, Is.EqualTo(expectedArchives)));

                    repo.Execute(new ModelCommand());
                    repo.SaveSnapshot();
                }
            }
        }

    }
}