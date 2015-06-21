using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FFTWSharp;
using NAudio.Wave;
using System.Numerics;

namespace vocoder.ClassLibrary
{
    /// <summary>
    ///     Вычисление спектра огибающей функции.
    ///     Данные нарезаются на фрагменты заданной длины.
    /// </summary>
    public class SpectrumBuilder : IDisposable
    {
        private readonly double[] _data;
        private readonly int _frequency;
        private readonly int _length;

        public SpectrumBuilder(int length, int frequency)
        {
            _length = length;
            _frequency = frequency;
            var fftw = new fftw_complexarray(_length);
            fftw.SetZeroData();
            _data = fftw.GetData_Real();
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
                try
                {
                    for (;;)
                    {
                        var list = new List<Int16>();
                        for (int i = 0; i < _length; i++)
                        {
                            list.Add(reader.ReadInt16());
                        }
                        int count = list.Count;
                        double[] array = list.Select(x => (double) x).ToArray();
                        var input = new fftw_complexarray(array.Select(x=>new Complex(x,0)).ToArray());
                        var output = new fftw_complexarray(count);
                        fftw_plan.dft_1d(count, input, output, fftw_direction.Forward, fftw_flags.Estimate).Execute();
                        var data = output.GetData_Complex();
                        for (int i = 0; i < _length; i++)
                            _data[i] = Math.Max(_data[i], data[i].Magnitude);
                    }
                }
                catch
                {
                }
            }
        }

        public double[] GetData()
        {
            double s = Math.Sqrt(_data.Sum(x => x * x));
            return _data.Select(x => x / s).ToArray();
        }
    }
}