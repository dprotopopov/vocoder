using System;
using System.Linq;
using FFTWSharp;
using NAudio.Wave;

namespace vocoder.ClassLibrary
{
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
            _data = fftw.GetData_double();
        }

        public void Dispose()
        {
        }

        public SpectrumBuilder Add(WaveStream waveStream)
        {
            using (var reader = new WaveFileReader(waveStream))
            {
                var newFormat = new WaveFormat(_frequency, 8, 1);
                using (var waveFormatConversionStream = new WaveFormatConversionStream(newFormat, reader))
                {
                    var buffer = new byte[_length];
                    while (waveFormatConversionStream.Read(buffer, 0, _length) == _length)
                    {
                        var fftw = new fftw_complexarray(Enumerable.Cast<double>(buffer).ToArray());
                        double[] data = fftw.GetData_double();
                        for (int i = 0; i < _length; i++) _data[i] += Math.Abs(data[i]);
                    }
                }
            }
            return this;
        }

        public double[] Normalize()
        {
            double s = _data.Sum();
            return _data.Select(x => x/s).ToArray();
        }
    }
}