public interface IWriteableFloatField : IWriteableValueField<float>
{
    public void BlendValue(int x, int y, BlendMode mode, float value);

    public void BlendAll(BlendMode mode, float value);

    public void BlendAll(BlendMode mode, FloatField values);

    public void Remap(float min, float max);
}
