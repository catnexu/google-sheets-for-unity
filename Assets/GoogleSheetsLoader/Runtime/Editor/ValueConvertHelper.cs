using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace catnexu.gsl.Editor
{
    public static class ValueConvertHelper
    {
        private static readonly Dictionary<Type, IValueConverter> s_valueConverters;

        static ValueConvertHelper()
        {
            s_valueConverters = new Dictionary<Type, IValueConverter>();
            IEnumerable<Type> types = GetChildTypesByInterface<IValueConverter>();
            foreach (var type in types)
            {
                if (Activator.CreateInstance(type) is IValueConverter converter)
                {
                    s_valueConverters.TryAdd(converter.Type, converter);
                }
            }
        }

        public static object Convert(string input, Type type)
        {
            Type converterType = type;
            if (converterType.IsEnum)
            {
                converterType = typeof(Enum);
            }

            return s_valueConverters.TryGetValue(converterType, out IValueConverter converter) ? converter.Convert(input, type) : null;
        }
        
        public static IEnumerable<Type> GetChildTypesByInterface<T>()
        {
            Assembly[] assemblyArray = AppDomain.CurrentDomain.GetAssemblies();
            for (int index = 0; index < assemblyArray.Length; ++index)
            {
                IEnumerable<Type> types = assemblyArray[index].GetTypes()
                                                              .Where(myType => myType.IsClass && !myType.IsAbstract && typeof (T).IsAssignableFrom(myType));
                foreach (Type type in types)
                {
                    yield return type;
                }
            }
            assemblyArray = (Assembly[]) null;
        }
    }
}