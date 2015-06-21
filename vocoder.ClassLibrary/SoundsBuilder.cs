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

        public double[] GetData()
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
                        var fftw = new fftw_complexarray(list.Select(x => (Complex) Math.Abs(x)).ToArray());
                        var data = fftw.GetData_Real();
                        double s = Math.Sqrt(data.Sum(x => x * x));
                        return data.Select(x => x / s).ToArray();
                    }
                }
            }
        }
    }
}