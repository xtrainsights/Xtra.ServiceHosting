using System;
using System.Reflection;


namespace Xtra.ServiceHost.Internals
{

    internal static class ExtendAssembly
    {

        public static string GetAssemblyAttribute<T>(this Assembly assembly, Func<T, string> value)
            where T : Attribute
        {
            T attribute = (T)Attribute.GetCustomAttribute(assembly, typeof(T));
            return value.Invoke(attribute);
        }

    }

}
