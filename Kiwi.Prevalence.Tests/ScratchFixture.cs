using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Kiwi.Prevalence.Tests
{
    [TestFixture]
    public class ScratchFixture
    {
        public class Model
        {
            public HashSet<string> Users = new HashSet<string>();
        }

        public class AddUserCommand : AbstractCommand<Model, bool>
        {
            public AddUserCommand()
            {
            }

            public AddUserCommand(string user)
            {
                User = user;
            }

            public string User { get; set; }

            public override Func<bool> Prepare(Model model)
            {
                return () => model.Users.Add(User);
            }
        }

        [Test]
        public void Test()
        {
            using (
                var repo =
                    new Repository<Model>(
                        new RepositoryConfiguration
                            {CommandSerializer = new CommandSerializer().WithTypeAlias<AddUserCommand>("addUser")},
                        new ModelFactory<Model>(() => new Model()))
                        {
                            Path = "c:\\temp\\a"
                        }
                )
            {
                for (var i = 0; i < 500001; ++i)
                {
                    repo.Execute(new AddUserCommand("joe" + i));

                    if (repo.Revision - repo.SnapshotRevision > 100005)
                    {
                        repo.SaveSnapshot();
                    }
                }
                Console.Out.WriteLine("Revision is now {0}", repo.Revision);
                Console.Out.WriteLine("Snapshot revision is now {0}", repo.SnapshotRevision);
                Console.Out.WriteLine("Model size is {0}", repo.Query(m => m.Users.Count));
            }
        }
    }
}