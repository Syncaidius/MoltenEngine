using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public class Perlin2D
    {
        public int Seed { get; set; }

        int _seed;

        public Perlin2D(int seed)
        {
            _seed = seed;
            Random r = new Random(_seed);
        }

        /// <summary>Randomly generates a noise value between -1.0 and 1.0</summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="octaves">The number of octaves.</param>
        /// <param name="amplitude">The amplitude.</param>
        /// <param name="frequency">The frequency.</param>
        /// <param name="persistence">The persistence or strength of the noise.</param>
        /// <returns></returns>
        public double Noise(int x, int y, int octaves = 8, double amplitude = 1.0, double frequency = 0.015, double persistence = 0.65)
        {
            //returns -1 to 1
            double total = 0.0;
            double freq = frequency, amp = amplitude;
            for (int i = 0; i < octaves; ++i)
            {
                total = total + Smooth(x * freq, y * freq) * amp;
                freq *= 2;
                amp *= persistence;
            }
            if (total < -2.4) total = -2.4;
            else if (total > 2.4) total = 2.4;

            return (total / 2.4);
        }

        private double NoiseGeneration(int x, int y)
        {
            int n = x + y * 57;
            n = (n << 13) ^ n;

            return (1.0 - ((n * (n * n * 15731 + 789221) + _seed) & 0x7fffffff) / 1073741824.0);
        }

        private double Interpolate(double x, double y, double a)
        {
            double value = (1 - Math.Cos(a * Math.PI)) * 0.5;
            return x * (1 - value) + y * value;
        }

        private double Smooth(double x, double y)
        {
            double n1 = NoiseGeneration((int)x, (int)y);
            double n2 = NoiseGeneration((int)x + 1, (int)y);
            double n3 = NoiseGeneration((int)x, (int)y + 1);
            double n4 = NoiseGeneration((int)x + 1, (int)y + 1);

            double i1 = Interpolate(n1, n2, x - (int)x);
            double i2 = Interpolate(n3, n4, x - (int)x);

            return Interpolate(i1, i2, y - (int)y);
        }
    }
}
