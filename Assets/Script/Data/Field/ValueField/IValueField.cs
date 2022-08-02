public interface IValueField<T> : IReadableValueField<T>, IWriteableValueField<T>
{
    public T[] values { get; }
}
