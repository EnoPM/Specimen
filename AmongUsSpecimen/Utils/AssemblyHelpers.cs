using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AmongUsSpecimen.Utils;

public static class AssemblyHelpers
{
    public static List<Assembly> AllAssemblies
    {
        get
        {
            var assemblies = new List<Assembly>();
            var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (assemblyFolder != null)
            {
                assemblies.AddRange(Directory.GetFiles(assemblyFolder, "*.dll").Select(Assembly.LoadFrom));
            }

            return assemblies;
        }
    }

    internal static Assembly EmbeddedAssemblyResolver(object sender, ResolveEventArgs args)
    {
        var libName = args.Name.Split(", ")[0];
        return !Specimen.EmbeddedLibraries.Contains(libName) ? null : Assembly.GetExecutingAssembly().LoadEmbeddedLibrary(libName);
    }
}