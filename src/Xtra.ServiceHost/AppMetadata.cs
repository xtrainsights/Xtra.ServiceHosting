using System;
using System.Reflection;

using Xtra.ServiceHost.Internals;


namespace Xtra.ServiceHost
{

    public static class AppMetadata
    {

        public static Lazy<string> Title = new Lazy<string>(() => ExecutingAssembly.Value.GetAssemblyAttribute<AssemblyTitleAttribute>(a => a?.Title));
        public static Lazy<string> Product = new Lazy<string>(() => ExecutingAssembly.Value.GetAssemblyAttribute<AssemblyProductAttribute>(a => a?.Product));
        public static Lazy<string> Description = new Lazy<string>(() => ExecutingAssembly.Value.GetAssemblyAttribute<AssemblyDescriptionAttribute>(a => a?.Description));
        public static Lazy<string> Company = new Lazy<string>(() => ExecutingAssembly.Value.GetAssemblyAttribute<AssemblyCompanyAttribute>(a => a?.Company));
        public static Lazy<string> Copyright = new Lazy<string>(() => ExecutingAssembly.Value.GetAssemblyAttribute<AssemblyCopyrightAttribute>(a => a?.Copyright));
        public static Lazy<string> Trademark = new Lazy<string>(() => ExecutingAssembly.Value.GetAssemblyAttribute<AssemblyTrademarkAttribute>(a => a?.Trademark));

        private static readonly Lazy<Assembly> ExecutingAssembly = new Lazy<Assembly>(Assembly.GetEntryAssembly);

    }

}