using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using FFTWSharp;
using NAudio.Wave;

namespace vocoder.ClassLibrary
{
    /// <summary>
    ///     Вычисление спектра огибающей функции.
    ///     Данные дополняются тишиной до заданной продолжительности.
    ///     Данные записываются в обратном порядке.
    /// </summary>
    public class WaveBuilder : IDisposable
    {
        private readonly double _duration;
        private readonly int _frequency;
        private readonly MemoryStream _memoryStream = new MemoryStream();

        public WaveBuilder(double duration, int frequency)
        {
            _duration = duration;
            _frequency = frequency;
        }

        public void Dispose()
        {
        }

        public void Add(WaveStream waveStream)
        {
            var newFormat = new WaveFormat(_frequency, 16, 1);
            using (var waveFormatConversionStream = new WaveFormatConversionStream(newFormat, waveStream))
            {
                var reader = new BinaryReader(waveFormatConversionStream);
                var writer = new BinaryWriter(_memoryStream);
                try
                {
                    for (;;)
                        writer.Write(reader.ReadInt16());
                }
                catch
                {
                }
            }
        }

        public Complex[] GetData_Complex()
        {
            var length = (int) (_duration*_frequency);
            _memoryStream.Seek(0, SeekOrigin.Begin);
            var reader = new BinaryReader(_memoryStream);
            var list = new List<Int16>();
            int i = 0;
            try
            {
                for (; i < length; i++)
                    list.Add(reader.ReadInt16());
            }
            catch
            {
                for (; i < length; i++)
                    list.Add(0);
            }
            int count = list.Count;
            double[] array = list.Select(x => (double) x).ToArray();
            var input = new fftw_complexarray(array.Select(x => new Complex(x, 0)).ToArray());
            var output = new fftw_complexarray(count);
            fftw_plan.dft_1d(count, input, output, fftw_direction.Forward, fftw_flags.Estimate).Execute();
            return output.GetData_Complex();
        }
    }
}