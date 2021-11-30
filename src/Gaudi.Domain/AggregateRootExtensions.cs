namespace Gaudi.Domain;

public static class AggregateRootExtensions
{
    public static Task<Result> Store<T, TId>(this Task<Result<T>> updatedOrientatie, IStorage<TId, T> storage) where T : AggregateRoot<T, TId>, new()
        => updatedOrientatie
            .Then(orientatie => storage.Store(orientatie));
}
