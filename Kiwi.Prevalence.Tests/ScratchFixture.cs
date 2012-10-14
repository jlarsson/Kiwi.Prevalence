using System;
using System.Collections.Generic;
using Kiwi.Prevalence.Journaling;
using NUnit.Framework;

namespace Kiwi.Prevalence.Tests
{
    [TestFixture,Explicit]
    public class ScratchFixture
    {
        public class Model
        {
            public HashSet<string> Users = new HashSet<string>();
        }

        public class AddUserCommand : AbstractCommand<Model>
        {
            public AddUserCommand()
            {
            }

            public AddUserCommand(string user)
            {
                User = user;
            }

            public string User { get; set; }

            public override void Execute(Model model)
            {
                model.Users.Add(User);
            }
        }

        [Test]
        public void Test()
        {
            using (
                var repo =
                    new Repository<Model>(
                        new RepositoryConfiguration(@"c:\temp\a")
                            {CommandSerializer = new CommandSerializer().WithTypeAliasForAllCommandsInAssembly(typeof(AddUserCommand).Assembly)},
                        new ModelFactory<Model>(() => new Model()))
                )
            {
                // make a dummy query to boot up repo
                repo.Query(m => 0);

                var journal = repo.Journal as Journal;
                for (var i = 0; i < 500001; ++i)
                {
                    repo.Execute(new AddUserCommand("joe" + i));


                    if (journal.Revision - journal.SnapshotRevision > 100005)
                    {
                        repo.SaveSnapshot();
                    }
                }

                Console.Out.WriteLine("Revision is now {0}", journal.Revision);
                Console.Out.WriteLine("Snapshot revision is now {0}", journal.SnapshotRevision);
                Console.Out.WriteLine("Model size is {0}", repo.Query(m => m.Users.Count));
            }
        }
    }
}