using System;

namespace catnexu.gsl.Editor
{
    public interface IValueConverter
    {
        Type Type { get; }
        
        object Convert(string input, Type type);
        string Convert(object input);
    }
}