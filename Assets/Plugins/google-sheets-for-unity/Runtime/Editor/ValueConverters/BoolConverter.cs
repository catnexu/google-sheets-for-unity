using System;

namespace catnexu.gsl.Editor.ValueConverters
{
    public class BoolConverter : ValueConverter<bool>
    {
        public override object Convert(string input, Type type)
        {
            bool result = false;
            bool.TryParse(input, out result);
            return result;
        }
    }
}