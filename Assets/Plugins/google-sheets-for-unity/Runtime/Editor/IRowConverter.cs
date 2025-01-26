using System;
using System.Collections.Generic;

namespace catnexu.gsl.Editor
{
    public interface IRowConverter
    {
        List<object> Convert(List<List<object>> table, Type outObjectType);
    }
}