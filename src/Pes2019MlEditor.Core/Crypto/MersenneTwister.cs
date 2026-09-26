namespace Pes2019MlEditor.Core.Crypto;

/// <summary>
/// MT19937 (Mersenne Twister) 32-bit PRNG matching Takuji Nishimura &amp; Makoto Matsumoto (2002/1/26)
/// as used by PES 2016-2021 encryption routines.
/// </summary>
public sealed class MersenneTwister
{
    private const int N = 624;
    private const int M = 397;
    private const uint MATRIX_A = 0x9908B0DFU;
    private const uint UPPER_MASK = 0x80000000U;
    private const uint LOWER_MASK = 0x7FFFFFFFU;

    private readonly uint[] _mt = new uint[N];
    private int _mti = N + 1;

    public void InitGenRand(uint s)
    {
        _mt[0] = s;
        for (_mti = 1; _mti < N; _mti++)
        {
            _mt[_mti] = (1812433253U * (_mt[_mti - 1] ^ (_mt[_mti - 1] >> 30)) + (uint)_mti);
        }
    }

    public void InitByArray(uint[] initKey)
    {
        ArgumentNullException.ThrowIfNull(initKey);
        InitGenRand(19650218U);
        int i = 1;
        int j = 0;
        int k = N > initKey.Length ? N : initKey.Length;

        for (; k > 0; k--)
        {
            _mt[i] = (_mt[i] ^ ((_mt[i - 1] ^ (_mt[i - 1] >> 30)) * 1664525U))
                     + initKey[j] + (uint)j;
            i++;
            j++;
            if (i >= N)
            {
                _mt[0] = _mt[N - 1];
                i = 1;
            }
            if (j >= initKey.Length)
            {
                j = 0;
            }
        }

        for (k = N - 1; k > 0; k--)
        {
            _mt[i] = (_mt[i] ^ ((_mt[i - 1] ^ (_mt[i - 1] >> 30)) * 1566083941U))
                     - (uint)i;
            i++;
            if (i >= N)
            {
                _mt[0] = _mt[N - 1];
                i = 1;
            }
        }

        _mt[0] = 0x80000000U; // MSB is 1; assuring non-zero initial array
    }

    public uint GenRandInt32()
    {
        uint y;
        Span<uint> mag01 = [0x0U, MATRIX_A];

        if (_mti >= N)
        {
            int kk;
            if (_mti == N + 1)
            {
                InitGenRand(5489U);
            }

            for (kk = 0; kk < N - M; kk++)
            {
                y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                _mt[kk] = _mt[kk + M] ^ (y >> 1) ^ mag01[(int)(y & 1U)];
            }

            for (; kk < N - 1; kk++)
            {
                y = (_mt[kk] & UPPER_MASK) | (_mt[kk + 1] & LOWER_MASK);
                _mt[kk] = _mt[kk + (M - N)] ^ (y >> 1) ^ mag01[(int)(y & 1U)];
            }

            y = (_mt[N - 1] & UPPER_MASK) | (_mt[0] & LOWER_MASK);
            _mt[N - 1] = _mt[M - 1] ^ (y >> 1) ^ mag01[(int)(y & 1U)];

            _mti = 0;
        }

        y = _mt[_mti++];

        // Tempering
        y ^= (y >> 11);
        y ^= (y << 7) & 0x9D2C5680U;
        y ^= (y << 15) & 0xEFC60000U;
        y ^= (y >> 18);

        return y;
    }
}
