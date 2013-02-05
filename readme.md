# Kiwi.Prevalence ##
Kiwi.Prevalence is a .NET [System Prevalence Layer](http://en.wikipedia.org/wiki/System_Prevalence).
As such, it maintains an application model in memory, providing gated access to ensure consistency and journalling to provide persistence and durability.

## License 

Quite simple, use this without restrictions, but don't blame us. 
For a formal specification, checkout the [MIT licence](http://www.opensource.org/licenses/mit-license.php).

## Prerequisites
* requires .NET 4.
* best downloaded from [nuget](http://nuget.org/packages/Kiwi.Prevalence)

## The model
The application model is mainatined by a _Repository&lt;TModel&gt;_ which owns an instance of your preferred model,  _TModel_.

## Gated access
To maintain model consistency over time, its not allowed to interact directly with the model. Instead, the methods  _Repository&lt;TModel&gt;::Query(&lambda;)_ and  _Repository&lt;TModel&gt;::Execute(command)_ must be used.
### Synchronization
_Repository&lt;TModel&gt;::Qyery(&lambda;)_ ensures that your &lambda; (on the form _Func&lt;TModel,TResult&gt;_) can access the model in assumed read-only mode (a read lock is taken).

Modyfying operations are taken out by _Repository&lt;TModel&gt;::Query(command)_ where the command must implement _ICommand&lt;TModel,TResult&gt;_ and is executed in exclusive-write mode (a write lock is taken). 

### Marshalling
To further protect the model, results from either _Query_ or _Execute_ is _marshalled_, which in this contect means that a deep copy of the result is taken. This guarantess that the results are detached from the model, preventing nasty concurrency bugs.

## Snapshots
A snapshot is the model serialized to disk. A snapshot in essence captures the model state at a given point in time.

## Journalling
All commands must be Json-serializable and are captured in a journal file. Whenever a repository is restored (typically during application startup), the journal is replayed on the current snapshot to catch up with latest changes in the model.

## Serialization
The model and all commands must be Json-serializable and Json-deserializable.

* The JSON format somewhat limits the expressiveness of your model. In particular, it may not contain cycles.
* The JSON format is chosen since its so forgiving and most of all, human and machine readable.

Actual serialization is carried out by [Kiwi.Json](https://github.com/jlarsson/Kiwi.Json).

## Persistence
In few words - JSON serialized to disk files.
Expect to find some or all of these files
* &lt;repo name&gt;.journal - contains JSON-serialized commands
* &lt;repo name&gt;.snapshot - contains a JSON-serialized model
* &lt;repo name&gt;.journal.&lt;revision&gt; - backup of journal taken at a specific journal revision
* &lt;repo name&gt;.snapshot.&lt;revision&gt; - backup of snapshot taken at a specific journal revision

## Performance tuning
### Parametrized instantiation
The _Repository_ class takes most strategic decisions from a _IRepositoryConfiguration_. A custom implementation (or instantiation of the default) allows customization of 
* Command serialization (the format used in the journal)
* Marshalling
* Synchronization

Each of the above strategies are quite small abstractions and should be fairly easy to implement if a non-default behaviour is wanted.

### Skip marshalling
For applications where the model isn't at risk of being compromised by pusblishing results from _Query_ or _Execute_, an additional parameter _QueryOptions.NoMarshall_ can be supplied that effectively skips the marshalling step all together.
### Custom marshalling
Since marshalling doesn't affect persistence in the journal or snapshot in any way, it's possible to setup a repository with a custom marshaller (say one based on _BinaryFormatter_, allowing cycles in the data).
### Custom synchronization
The default synchronization uses _System.Threading.ReaderWriterLock_, but other schemes can be specified by passing a custom _ISynchronize_ instance to the repository.

## What if I think prevalence is better than sliced bread but dislike this particular implementation?
* Fork and patch, fork and patch...
* or, use another library. Personally, I can recommend [#livedb](http://livedb.devrex.se/).
* or, if you rather roll your own, use our code as inspiration or
* or, start from this [minimal implementation](https://gist.github.com/1103582) provided by by one of the early driving forces, [Klaus Wuestefeld](https://github.com/klauswuestefeld).
