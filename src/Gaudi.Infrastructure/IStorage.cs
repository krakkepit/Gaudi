namespace Gaudi.Infrastructure
{
    internal interface IAggregateRoot { }

    internal interface IAggregateRoot<TId> : IAggregateRoot
    {
        TId Id { get; }
    }

    internal interface IStorage<TId, T> where T : IAggregateRoot<TId>
    {
        internal Task<Result> Store(T item);
        internal Task<Result> Delete(TId id);
        internal Task<Result<T>> ById(TId id);
    }

    internal class TransferStorage<TId, T, TStore1, TStore2> : IStorage<TId, T> where TStore1 : IStorage<TId, T> where TStore2 : IStorage<TId, T> where T : IAggregateRoot<TId>
    {
        private TStore1 From { get; init; }
        private TStore2 To { get; init; }
        public TransferStorage(TStore1 from, TStore2 to)
        {
            From = from ?? throw new ArgumentNullException(nameof(from));
            To = to ?? throw new ArgumentNullException(nameof(to));
        }

        Task<Result> IStorage<TId, T>.Store(T item)
            => To.Store(item);

        Task<Result> IStorage<TId, T>.Delete(TId id)
            => From.Delete(id)
                .Then(To.Delete(id));

        Task<Result<T>> IStorage<TId, T>.ById(TId id)
        => To.ById(id)
            .Else(ifInvalid:
                From.ById(id)
                    .ActAsync(oldDbo => To.Store(oldDbo)
                    .ThenReturn<T>(oldDbo))
            .Else(ifInvalid: Result.WithMessages<T>(ValidationMessage.Error($"{nameof(T)} with id {id} was not found in both stores."))));
    } 
}
