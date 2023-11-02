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
}