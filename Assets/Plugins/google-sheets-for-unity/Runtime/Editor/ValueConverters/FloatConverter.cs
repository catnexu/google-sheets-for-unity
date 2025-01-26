using System;
using System.Globalization;

namespace catnexu.gsl.Editor.ValueConverters
{
    public class FloatConverter : ValueConverter<float>
    {
        public override object Convert(string input, Type type)
        {
            float result = 0;
            var ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";
            float.TryParse(input, NumberStyles.Any, ci, out result);
            return result;
        }

        public override string Convert(object input)
        {
            var ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";
            return ((float)input).ToString(ci);
        }
    }
}