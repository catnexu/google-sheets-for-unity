using System;

namespace catnexu.gsl.Editor.ValueConverters
{
    public class StringConverter : ValueConverter<string>
    {
        public override object Convert(string input, Type type)
        {
            return string.IsNullOrEmpty(input) ? string.Empty : input;
        }
    }
}