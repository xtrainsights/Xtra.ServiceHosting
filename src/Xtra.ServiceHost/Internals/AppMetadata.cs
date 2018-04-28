using System;
using System.Reflection;

namespace Xtra.ServiceHost.Internals
{

    public static class AppMetadata
    {

        public static Lazy<string> Title = new Lazy<string>(() => EntryAssembly.Value.GetAssemblyAttribute<AssemblyTitleAttribute>(a => a?.Title));
        public static Lazy<string> Product = new Lazy<string>(() => EntryAssembly.Value.GetAssemblyAttribute<AssemblyProductAttribute>(a => a?.Product));
        public static Lazy<string> Description = new Lazy<string>(() => EntryAssembly.Value.GetAssemblyAttribute<AssemblyDescriptionAttribute>(a => a?.Description));
        public static Lazy<string> Company = new Lazy<string>(() => EntryAssembly.Value.GetAssemblyAttribute<AssemblyCompanyAttribute>(a => a?.Company));
        public static Lazy<string> Copyright = new Lazy<string>(() => EntryAssembly.Value.GetAssemblyAttribute<AssemblyCopyrightAttribute>(a => a?.Copyright));
        public static Lazy<string> Trademark = new Lazy<string>(() => EntryAssembly.Value.GetAssemblyAttribute<AssemblyTrademarkAttribute>(a => a?.Trademark));

        private static readonly Lazy<Assembly> EntryAssembly = new Lazy<Assembly>(Assembly.GetEntryAssembly);

    }

}