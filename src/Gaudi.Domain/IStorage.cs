namespace Gaudi.Domain;
    
public interface IStorage<TId, T> where T : AggregateRoot<T, TId>, new()
{
    Task<Result> Store(T item);
    Task<Result> Delete(TId id);
    Task<Result<T>> ById(TId id);
}
