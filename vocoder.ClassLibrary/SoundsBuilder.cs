using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Speech.Synthesis;
using FFTWSharp;
using NAudio.Wave;

namespace vocoder.ClassLibrary
{
    /// <summary>
    ///     Подготовка образцов звуков.
    ///     Вычисление спектра огибающей функции.
    ///     Образцы дополняются тишиной до заданной продолжительности.
    /// </summary>
    public class SoundsBuilder : IDisposable
    {
        private readonly double _duration;
        private readonly int _frequency;
        private readonly string _phoneme;

        public SoundsBuilder(string phoneme, double duration, int frequency)
        {
            _phoneme = phoneme;
            _duration = duration;
            _frequency = frequency;
        }

        public void Dispose()
        {
        }

        public Complex[] GetData_Complex()
        {
            var length = (int) (_duration*_frequency);
            using (var memoryStream = new MemoryStream())
            {
                var synthesizer = new SpeechSynthesizer();
                synthesizer.SetOutputToWaveStream(memoryStream);
                synthesizer.Speak(_phoneme);
                memoryStream.Seek(0, SeekOrigin.Begin);
                using (var waveFileReader = new WaveFileReader(memoryStream))
                {
                    var newFormat = new WaveFormat(_frequency, 16, 1);
                    using (var waveFormatConversionStream = new WaveFormatConversionStream(newFormat, waveFileReader))
                    {
                        var reader = new BinaryReader(waveFormatConversionStream);
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
                        double[] array = list.Select(x => (double) Math.Abs(x)).ToArray();
                        var input = new fftw_complexarray(array.Select(x => new Complex(x, 0)).ToArray());
                        var output = new fftw_complexarray(count);
                        fftw_plan.dft_1d(count, input, output, fftw_direction.Forward, fftw_flags.Estimate).Execute();
                        Complex[] data = output.GetData_Complex();
                        double s = Math.Sqrt(data.Select(x => x.Magnitude).Sum(x => x*x));
                        return data.Select(x => x/s).ToArray();
                    }
                }
            }
        }
    }
}