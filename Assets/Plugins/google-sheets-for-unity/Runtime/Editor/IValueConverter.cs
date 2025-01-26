using System;

namespace catnexu.googlesheetsforunity.Editor
{
    public interface IValueConverter
    {
        Type Type { get; }
        
        object Convert(string input, Type type);
        string Convert(object input);
    }
}