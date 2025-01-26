using System;

namespace catnexu.googlesheetsforunity.Editor
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