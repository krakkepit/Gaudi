using MongoDB.Driver;
using Qowaiv.DomainModel;
using System.Linq;

namespace Gaudi.Infrastructure
{
    public class MongoDbStorage<TId, T> : IStorage<TId, T> where T : AggregateRoot<T, TId>, new()
    {
        private readonly IMongoCollection<T> _collection;
        private bool _indexesCreated;

        public MongoDbStorage(IMongoCollection<T> collection)
        {
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
        }

        public async Task<Result<T>> ById(TId id)
        {
            await EnsureIndexAsync();
            try
            {
                var results = (await _collection.FindAsync(x => x.Id != null && x.Id.Equals(id))).ToList();
                if (results == null || results.Count <= 0)
                {
                    return Result.WithMessages<T>(ValidationMessage.Error($"Retrieving object by Id {id} did not yield any results"));
                }

                return results.First();
            }
            catch (Exception e)
            {
                return Result.WithMessages<T>(ValidationMessage.Error($"Exception thrown while trying to retrieve object by Id {id}: {e.Message}"));
            }
        }

        public async Task<Result> Delete(TId id)
        {
            await EnsureIndexAsync();
            try
            {
                await _collection.DeleteOneAsync(x => x.Id != null && x.Id.Equals(id));
                return Result.OK;
            }
            catch (Exception e)
            {
                return Result.WithMessages<T>(ValidationMessage.Error($"Exception thrown while trying to retrieve object by Id {id}: {e.Message}"));
            }
        }

        public async Task<Result> Store(T item)
        {
            ArgumentNullException.ThrowIfNull(item);

            //item.TimeToLive = options.TimeToLive;
            try
            {
                await _collection.ReplaceOneAsync(
                    x => x.Id != null && x.Id.Equals(item.Id),
                    item,
                    new ReplaceOptions { IsUpsert = true });

                return Result.OK;
            }
            catch (Exception e)
            {
                return Result.WithMessages<T>(
                    ValidationMessage.Error($"Exception thrown while trying to update object by Id {item.Id}: {e.Message}"));
            }
        }

        /// <summary>Ensures that the index on BcNumber exists.</summary>
        private async Task EnsureIndexAsync()
        {
            // check if indexes have been created
            // if the flag is set to true; skip the creation to avoid trying to create them each time
            if (_indexesCreated)
            {
                return;
            }

            var indexes = new[]
            {
                Builders<T>.IndexKeys
                    .Ascending(d => d.Id)
            };

            await _collection.Indexes.CreateManyAsync(
                indexes.Select(index => new CreateIndexModel<T>(index)));

            _indexesCreated = true;
        }
    }
}
