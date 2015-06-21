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
                        var fftw = new fftw_complexarray(list.Select(x => (Complex) x).ToArray());
                        double[] data = fftw.GetData_Real();
                        for (int i = 0; i < _length; i++)
                            _data[i] = Math.Max(_data[i], Math.Abs(data[i]));
                    }
                }
                catch
                {
                }
            }
        }

        public double[] GetData()
        {
            double s = Math.Sqrt(_data.Sum(x => x*x));
            return _data.Select(x => x/s).ToArray();
        }
    }
}