using System;
using System.Collections.Generic;

namespace BenchmarkTool.Generators
{
public sealed class XorShiftRng: Random
{
    public XorShiftRng(ulong seed1, ulong seed2)
    {
        if (seed1 == 0 && seed2 == 0)
            throw new ArgumentException("seed1 and seed 2 cannot both be zero.");

        s[0] = seed1;
        s[1] = seed2;
    }

    public XorShiftRng()
    {
        var bytes = Guid.NewGuid().ToByteArray();

        s[0] = BitConverter.ToUInt64(bytes, 0);
        s[1] = BitConverter.ToUInt64(bytes, 8);
    }

    public ulong NextUlong()
    {
        unchecked
        {
            ulong s0 = s[p];
            ulong s1 = s[p = (p + 1) & 15];
            s1 ^= s1 << 31;
            s[p] = s1 ^ s0 ^ (s1 >> 11) ^ (s0 >> 30);
            return s[p]*1181783497276652981;
        }
    }

    public long NextLong(long maxValue)
    {
        return (int)NextLong(0, maxValue);
    }

    public long NextLong(long minValue, long maxValue)
    {
        if (minValue > maxValue)
            throw new ArgumentOutOfRangeException(nameof(minValue), "minValue cannot exceed maxValue");

        if (minValue == maxValue)
            return minValue;

        return (int) (NextUlong() / ((double)ulong.MaxValue / (maxValue - minValue)) + minValue);
    }

    public override int Next()
    {
        return (int) NextLong(0, int.MaxValue + 1L);
    }

    public override int Next(int maxValue)
    {
        return (int) NextLong(0, maxValue + 1L);
    }

    public override int Next(int minValue, int maxValue)
    {
        return (int) NextLong(minValue, maxValue);
    }

    public override void NextBytes(byte[] buffer)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));

        int remaining = buffer.Length;                                 

        while (remaining > 0)
        {
            var next = BitConverter.GetBytes(NextUlong());
            int n = Math.Min(next.Length, remaining);

            Array.Copy(next, 0, buffer, buffer.Length-remaining, n);
            remaining -= n;
        }
    }

    public override double NextDouble()
    {
        return NextUlong() / (ulong.MaxValue + 1.0);
    }


    readonly ulong[] s = new ulong[16];
    int p;
}

}