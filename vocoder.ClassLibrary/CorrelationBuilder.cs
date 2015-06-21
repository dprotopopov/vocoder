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
            int count = _array1.Length;
            var input1 = new fftw_complexarray(_array1.Select(x => new Complex(x, 0)).ToArray());
            var input2 = new fftw_complexarray(_array2.Reverse().Select(x => new Complex(x, 0)).ToArray());
            var output1 = new fftw_complexarray(count);
            var output2 = new fftw_complexarray(count);
            fftw_plan.dft_1d(count, input1, output1, fftw_direction.Forward, fftw_flags.Estimate).Execute();
            fftw_plan.dft_1d(count, input2, output2, fftw_direction.Forward, fftw_flags.Estimate).Execute();
            Complex[] complexs = output1.GetData_Complex().Zip(output2.GetData_Complex(), (x, y) => (x*y)).ToArray();
            var input3 = new fftw_complexarray(complexs);
            var output3 = new fftw_complexarray(count);
            fftw_plan.dft_1d(count, input3, output3, fftw_direction.Backward, fftw_flags.Estimate).Execute();
            return output3.GetData_Complex()[0].Magnitude;
        }
    }
}