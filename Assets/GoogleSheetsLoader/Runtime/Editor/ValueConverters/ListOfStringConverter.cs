using System;
using System.Collections.Generic;
using System.Linq;

namespace catnexu.gsl.Editor.ValueConverters
{
    public class ListOfStringConverter : ValueConverter<List<string>>
    {
        public override object Convert(string input, Type type)
        {
            string[] values = input.Split(',');
            if (values != null && values.Length > 0)
            {
                return values.ToList();
            }
            
            return null;
        }

        public override string Convert(object input)
        {
            if (input is IList<string> list)
            {
                return list.Aggregate((x, y) => $"{x},{y}");
            }

            return string.Empty;
        }
    }
}