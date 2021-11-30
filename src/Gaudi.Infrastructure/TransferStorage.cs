using Qowaiv.DomainModel;

namespace Gaudi.Infrastructure;

internal class TransferStorage<TId, T> : IStorage<TId, T> where T : AggregateRoot<T, TId>, new()
{
    private IStorage<TId, T> From { get; init; }
    private IStorage<TId, T> To { get; init; }
    public TransferStorage(IStorage<TId, T> from, IStorage<TId, T> to)
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
