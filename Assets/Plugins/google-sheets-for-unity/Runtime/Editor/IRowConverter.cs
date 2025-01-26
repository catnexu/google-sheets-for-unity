using System;
using System.Collections.Generic;

namespace catnexu.googlesheetsforunity.Editor
{
    public interface IRowConverter
    {
        List<object> Convert(List<List<object>> table, Type outObjectType);
    }
}