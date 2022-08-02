    using System;
    using UnityEngine;

    public class CompoundFloatField : IFloatField
    {
        private readonly FloatField[] fields;

        public int size => fields[0].size;

        public CompoundFloatField(params FloatField[] fields)
        {
            
            this.fields = fields;
        }
        
        public int GetIndex(int x, int y)
        {
            throw new NotImplementedException();
        }

        public int GetIndex(Vector2Int v)
        {
            throw new NotImplementedException();
        }

        public int GetXFromIndex(int index)
        {
            throw new NotImplementedException();
        }

        public int GetYFromIndex(int index)
        {
            throw new NotImplementedException();
        }

        public float GetValue(int index)
        {
            throw new NotImplementedException();
        }

        public float GetValue(int x, int y)
        {
            throw new NotImplementedException();
        }

        public void SetValue(int index, float value)
        {
            throw new NotImplementedException();
        }

        public void SetValue(int x, int y, float value)
        {
            throw new NotImplementedException();
        }

        public void ChangeValue(int x, int y, Func<float, float> changeFunction)
        {
            throw new NotImplementedException();
        }

        public void ChangeValue(int x, int y, Func<int, int, float, float> changeFunction)
        {
            throw new NotImplementedException();
        }

        public void ChangeAll(Func<float, float> changeFunction)
        {
            throw new NotImplementedException();
        }

        public void ChangeAll(Func<int, int, float, float> changeFunction)
        {
            throw new NotImplementedException();
        }

        public bool IsInBounds(int x, int y)
        {
            throw new NotImplementedException();
        }

        public void BlendValue(int x, int y, BlendMode mode, float value)
        {
            throw new NotImplementedException();
        }

        public void BlendAll(BlendMode mode, float value)
        {
            throw new NotImplementedException();
        }

        public void BlendAll(BlendMode mode, FloatField values)
        {
            throw new NotImplementedException();
        }

        public void Remap(float min, float max)
        {
            throw new NotImplementedException();
        }
    }
