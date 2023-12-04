using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;

namespace AmongUsSpecimen.Utils;

[AttributeUsage(AttributeTargets.Class)]
internal sealed class RegisterMonoBehaviourAttribute : Attribute
{
    private static readonly HashSet<Assembly> RegisteredAssemblies = new();

    /// <summary>
    /// Gets il2cpp interfaces to be injected with this type.
    /// </summary>
    private Type[] Interfaces { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterMonoBehaviourAttribute"/> class without any interfaces.
    /// </summary>
    public RegisterMonoBehaviourAttribute()
    {
        Interfaces = Type.EmptyTypes;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterMonoBehaviourAttribute"/> class with interfaces.
    /// </summary>
    /// <param name="interfaces">Il2Cpp interfaces to be injected with this type.</param>
    public RegisterMonoBehaviourAttribute(params Type[] interfaces)
    {
        Interfaces = interfaces;
    }

    public static void RegisterType(Type type, Type[] interfaces)
    {
        var baseTypeAttribute = type.BaseType?.GetCustomAttribute<RegisterMonoBehaviourAttribute>();
        if (baseTypeAttribute != null)
        {
            RegisterType(type.BaseType!, baseTypeAttribute.Interfaces);
        }

        if (ClassInjector.IsTypeRegisteredInIl2Cpp(type))
        {
            return;
        }

        try
        {
            ClassInjector.RegisterTypeInIl2Cpp(type, new RegisterTypeOptions { Interfaces = interfaces });
        }
        catch (Exception e)
        {
            Specimen.Instance.Log.LogError($"Failed to register {type.FullDescription()}: {e}");
        }
    }

    /// <summary>
    /// Registers all Il2Cpp types annotated with <see cref="RegisterMonoBehaviourAttribute"/> in the specified <paramref name="assembly"/>.
    /// </summary>
    /// <remarks>This is called automatically on plugin assemblies so you probably don't need to call this.</remarks>
    /// <param name="assembly">The assembly to search.</param>
    internal static void RegisterAssembly(Assembly assembly)
    {
        if (RegisteredAssemblies.Contains(assembly)) return;
        RegisteredAssemblies.Add(assembly);
        var types = assembly.GetClassesByAttribute<RegisterMonoBehaviourAttribute>();

        foreach (var type in types)
        {
            RegisterType(type.Type, type.Attribute.Interfaces);
        }
    }
}