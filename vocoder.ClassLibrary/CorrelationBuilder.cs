using System.Linq;
using System.Numerics;
using FFTWSharp;

namespace vocoder.ClassLibrary
{
    /// <summary>
    ///     Вычисление корреляции двух функций используя быстрое преобразование Фурье
    /// </summary>
    public class CorrelationBuilder
    {
        private readonly double[] _array1;
        private readonly double[] _array2;

        public CorrelationBuilder(double[] array1, double[] array2)
        {
            _array1 = array1;
            _array2 = array2;
        }

        public double GetValue()
        {
            var fftw1 = new fftw_complexarray(_array1.Select(x => (Complex) x).ToArray());
            var fftw2 = new fftw_complexarray(_array2.Reverse().Select(x => (Complex) x).ToArray());
            var fftw =
                new fftw_complexarray(
                    fftw1.GetData_Real().Zip(fftw2.GetData_Real(), (x, y) => (Complex) (x*y)).ToArray());
            return fftw.GetData_Real()[0];
        }
    }
}