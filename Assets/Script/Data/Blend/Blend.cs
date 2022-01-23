using System;
using UnityEngine;

public class Blend
{
    public static float Calc(BlendMode mode, float a, float b)
    {
        switch (mode)
        {
            case BlendMode.Add:
                return a + b;
            case BlendMode.Subtract:
                return a - b;
            case BlendMode.SubtractInverse:
                return b - a;
            case BlendMode.Min:
                return Mathf.Min(a, b);
            case BlendMode.Max:
                return Mathf.Max(a, b);
            case BlendMode.Multiply:
                return a * b;
            case BlendMode.Divide:
                return a / b;
            case BlendMode.DivideInverse:
                return b / a;
            case BlendMode.Delta:
                return Mathf.Abs(b - a);
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }
}
