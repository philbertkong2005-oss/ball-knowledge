// Compile-time compatibility shims so the Phase 1 engine's C# 9+/11 record features
// (`init` accessors and `required` members) compile for netstandard2.1 (the Unity target).
// These types exist in net8.0's BCL already, so this file is excluded from the net8.0 build
// via the #if guard — net8.0 behaviour is completely untouched. Packaging plumbing only:
// no engine logic, no public API. See docs/phase2-greybox.md (the bridge).
#if NETSTANDARD2_1
namespace System.Runtime.CompilerServices
{
    using System;

    // Enables `init`-only setters.
    internal static class IsExternalInit { }

    // Enables `required` members (C# 11).
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    internal sealed class RequiredMemberAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    internal sealed class CompilerFeatureRequiredAttribute : Attribute
    {
        public CompilerFeatureRequiredAttribute(string featureName) => FeatureName = featureName;
        public string FeatureName { get; }
        public bool IsOptional { get; init; }
        public const string RefStructs = nameof(RefStructs);
        public const string RequiredMembers = nameof(RequiredMembers);
    }
}

namespace System.Diagnostics.CodeAnalysis
{
    using System;

    // Lets a constructor declare it sets all `required` members.
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    internal sealed class SetsRequiredMembersAttribute : Attribute { }
}

namespace System.Text.Json.Serialization
{
    using System;

    // The engine only ever uses [JsonPropertyName] as inert metadata — it never calls
    // JsonSerializer (all (de)serialisation lives in the host: Console/Program.cs, or Unity).
    // Referencing the real System.Text.Json package for netstandard2.1 would force 8 transitive
    // BCL DLLs into Unity's Plugins/, several of which Unity already ships => duplicate-assembly
    // conflicts, plus IL2CPP reflection-stripping risk. Declaring the attribute here instead makes
    // the Unity DLL dependency-free. Shape matches the real attribute, so Models.cs is unchanged
    // and the net8.0 build (which uses the real BCL type) is untouched.
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    internal sealed class JsonPropertyNameAttribute : Attribute
    {
        public JsonPropertyNameAttribute(string name) => Name = name;
        public string Name { get; }
    }
}
#endif
