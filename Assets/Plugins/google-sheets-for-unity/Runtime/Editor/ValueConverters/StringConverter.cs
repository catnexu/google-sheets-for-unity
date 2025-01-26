using System;

namespace catnexu.googlesheetsforunity.Editor
{
    public class StringConverter : ValueConverter<string>
    {
        public override object Convert(string input, Type type)
        {
            return string.IsNullOrEmpty(input) ? string.Empty : input;
        }
    }
}