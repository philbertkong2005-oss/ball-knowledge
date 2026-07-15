namespace BallKnowledge.MatchEngine;

public sealed class TeamRuntime
{
    private TeamRuntime(TeamDefinition source, List<PlayerRuntime> namedPlayers, GenericProfile genericProfile)
    {
        Source = source;
        NamedPlayers = namedPlayers;
        GenericProfile = genericProfile;
        Formation = source.BaseFormation;
    }

    public TeamDefinition Source { get; }

    public List<PlayerRuntime> NamedPlayers { get; }

    public GenericProfile GenericProfile { get; }

    public string Formation { get; set; }

    public double TeamAtkModifier { get; set; } = 1d;

    public double TeamDefModifier { get; set; } = 1d;

    public double MatchShotModifier { get; set; } = 1d;

    public double MatchOnTargetModifier { get; set; } = 1d;

    public double MatchConversionModifier { get; set; } = 1d;

    public double MatchAttackModifier { get; set; } = 1d;

    public IEnumerable<PlayerRuntime> AvailableNamedScorers => NamedPlayers.Where(player => player.Available && player.Role == PlayerRole.Attacker);

    public string Signature =>
        $"{Source.Name}|{Formation}|{TeamAtkModifier:0.000}|{TeamDefModifier:0.000}|{MatchShotModifier:0.000}|{MatchOnTargetModifier:0.000}|{MatchConversionModifier:0.000}|{MatchAttackModifier:0.000}|{string.Join(',', NamedPlayers.Select(player => player.Signature))}";

    public static TeamRuntime Create(TeamDefinition source)
    {
        var namedPlayers = source.NamedPlayers.Select(player => new PlayerRuntime(player)).ToList();
        var genericProfile = new GenericProfile(
            MatchEngine.Clamp(source.Atk * 0.72d, 28d, 75d),
            MatchEngine.Clamp(source.Atk * 0.78d, 32d, 82d),
            MatchEngine.Clamp(((source.Atk + source.Def) / 2d) * 0.74d, 28d, 78d),
            MatchEngine.Clamp(source.Def * 0.78d, 35d, 80d));
        return new TeamRuntime(source, namedPlayers, genericProfile);
    }

    public static double GetMagnitude(EngineConfig config, FactorTier tier) => config.FactorTierMagnitudes.GetMagnitude(tier);

    public double GetBaseAttackPool()
    {
        return NamedPlayers.Where(player => player.Role is PlayerRole.Attacker or PlayerRole.Playmaker)
                   .Sum(player => player.BaseAttackContribution) +
               GenericProfile.StarterInvolvement;
    }

    public double GetLiveAttackPool()
    {
        return NamedPlayers.Where(player => player.Available && player.Role is PlayerRole.Attacker or PlayerRole.Playmaker)
                   .Sum(player => player.LiveAttackContribution) +
               GenericProfile.StarterInvolvement +
               (UnavailableOutfieldCount() * GenericProfile.SubstituteInvolvement);
    }

    public double GetEffectiveKeeperReliability()
    {
        return GetKeeper()?.EffectiveReliability ?? GenericProfile.GenericKeeperReliability;
    }

    public string GetKeeperName() => GetKeeper()?.DisplayName ?? "the stand-in keeper";

    public PlayerRuntime? GetPrimaryStriker()
    {
        return NamedPlayers
            .Where(player => player.Role == PlayerRole.Attacker)
            .OrderByDescending(player => player.Involvement)
            .FirstOrDefault();
    }

    public PlayerRuntime? GetPlaymaker()
    {
        return NamedPlayers
            .Where(player => player.Role == PlayerRole.Playmaker)
            .OrderByDescending(player => player.Passing)
            .FirstOrDefault();
    }

    public PlayerRuntime? GetKeeper()
    {
        return NamedPlayers.SingleOrDefault(player => player.Role == PlayerRole.Keeper);
    }

    public PlayerRuntime? GetFactorTargetPlayer(Random rng)
    {
        var eligible = NamedPlayers.Where(player => player.Available).ToList();
        return eligible.Count == 0 ? null : eligible[rng.Next(eligible.Count)];
    }

    public void MarkUnavailable(PlayerRuntime player)
    {
        player.Available = false;
    }

    public ShooterProfile DrawShooter(Random rng)
    {
        var candidates = new List<(string Id, string DisplayName, double Weight, double Finishing, bool IsOtherPlayer)>();
        candidates.AddRange(NamedPlayers
            .Where(player => player.Available && player.Role == PlayerRole.Attacker)
            .Select(player => (player.Id, player.DisplayName, player.LiveInvolvement, player.EffectiveFinishing, false)));

        var genericWeight = GenericProfile.StarterInvolvement + (UnavailableOutfieldCount() * GenericProfile.SubstituteInvolvement);
        candidates.Add(("OTHER", "Other Player", genericWeight, GenericProfile.StarterFinishing, true));

        var totalWeight = candidates.Sum(item => item.Weight);
        var roll = rng.NextDouble() * totalWeight;
        var cumulative = 0d;
        foreach (var item in candidates)
        {
            cumulative += item.Weight;
            if (roll <= cumulative)
            {
                return new ShooterProfile(item.Id, item.DisplayName, item.Finishing, item.IsOtherPlayer);
            }
        }

        var fallback = candidates[^1];
        return new ShooterProfile(fallback.Id, fallback.DisplayName, fallback.Finishing, fallback.IsOtherPlayer);
    }

    public AssistProfile? DrawAssist(ShooterProfile scorer, Random rng, double assistChance)
    {
        if (rng.NextDouble() > assistChance)
        {
            return null;
        }

        var candidates = NamedPlayers
            .Where(player => player.Available && player.Id != scorer.Id && player.Role is PlayerRole.Attacker or PlayerRole.Playmaker)
            .Select(player => new AssistProfile(player.Id, player.DisplayName, player.EffectivePassing))
            .Where(player => player.Weight > 0d)
            .ToList();

        if (candidates.Count == 0)
        {
            return null;
        }

        var total = candidates.Sum(item => item.Weight);
        var roll = rng.NextDouble() * total;
        var cumulative = 0d;
        foreach (var candidate in candidates)
        {
            cumulative += candidate.Weight;
            if (roll <= cumulative)
            {
                return candidate;
            }
        }

        return candidates[^1];
    }

    public TeamDebugState ToDebugState()
    {
        return new TeamDebugState(
            Source.Name,
            Formation,
            11,
            NamedPlayers.Where(player => player.Available).Select(player => player.Id).ToList(),
            NamedPlayers.Where(player => !player.Available).Select(player => player.Id).ToList(),
            AvailableNamedScorers.Select(player => player.Id).ToList(),
            true);
    }

    private int UnavailableOutfieldCount()
    {
        return NamedPlayers.Count(player => !player.Available && player.Role != PlayerRole.Keeper);
    }
}

public sealed class PlayerRuntime
{
    public PlayerRuntime(PlayerDefinition source)
    {
        Source = source;
        Finishing = source.Finishing ?? 45d;
        Involvement = source.Involvement ?? ((source.Passing ?? 55d) * 0.85d);
        Passing = source.Passing ?? ((Finishing + Involvement) / 2d);
        Reliability = source.Reliability ?? 50d;
    }

    public PlayerDefinition Source { get; }

    public string Id => Source.Id;

    public PlayerRole Role => Source.Role;

    public string DisplayName => Source.Name;

    public bool Available { get; set; } = true;

    public double ConditionMod { get; set; } = 1d;

    public double Finishing { get; set; }

    public double Involvement { get; set; }

    public double Passing { get; set; }

    public double Reliability { get; set; }

    public double BaseAttackContribution => Involvement;

    public double LiveAttackContribution => BaseAttackContribution * ConditionMod;

    public double LiveInvolvement => Involvement * ConditionMod;

    public double EffectiveFinishing => Finishing * ConditionMod;

    public double EffectivePassing => Passing * ConditionMod;

    public double EffectiveReliability => Reliability * ConditionMod;

    public string Signature =>
        $"{Id}:{Available}:{ConditionMod:0.000}:{Finishing:0.0}:{Involvement:0.0}:{Passing:0.0}:{Reliability:0.0}";
}

public sealed record GenericProfile(
    double StarterFinishing,
    double StarterInvolvement,
    double SubstituteInvolvement,
    double GenericKeeperReliability);

public sealed record ShooterProfile(string Id, string DisplayName, double Finishing, bool IsOtherPlayer);

public sealed record AssistProfile(string Id, string DisplayName, double Weight);
