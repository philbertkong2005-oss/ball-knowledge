using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BallKnowledge.BridgeTests;

/// <summary>
/// REFERENCE IMPLEMENTATION for the Unity-side config loader (Phase 2, Task 2 copies this
/// into the Unity asmdef verbatim — Unity cannot reference this test project).
///
/// Why this exists: the engine DLL shipped to Unity is dependency-free, so its
/// [JsonPropertyName] attributes are compile-time shims internal to the engine assembly
/// (NetstandardCompat.cs) — no JSON library recognises them by type. Newtonsoft with default
/// settings therefore binds every design/*.json value as zero/null, silently. A snake_case
/// naming strategy is NOT a fix either: constants.json mixes snake_case top-level keys with
/// camelCase formation keys, and teams.json has "ATK". The only correct mapping is the one
/// the engine itself declares — so this resolver reads the shim attribute from metadata by
/// FULL NAME (immune to which assembly declares it, and to the attribute being internal).
///
/// Writable = true lets Newtonsoft populate the records' init-only setters by reflection.
/// </summary>
public sealed class EngineJsonContractResolver : DefaultContractResolver
{
    private const string AttributeFullName = "System.Text.Json.Serialization.JsonPropertyNameAttribute";

    protected override JsonProperty CreateProperty(
        System.Reflection.MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);
        foreach (var attribute in member.GetCustomAttributesData())
        {
            if (attribute.AttributeType.FullName != AttributeFullName) continue;
            if (attribute.ConstructorArguments.Count < 1) continue;
            if (attribute.ConstructorArguments[0].Value is string name && name.Length > 0)
            {
                property.PropertyName = name;
                property.Writable = true;
            }
        }

        return property;
    }
}
