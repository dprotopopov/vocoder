using System.Linq;

namespace vocoder.ClassLibrary
{
    /// <summary>
    ///     Вычисление корреляции двух функций
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
            return _array1.Zip(_array2, (x, y) => (x*y)).Sum();
        }
    }
}