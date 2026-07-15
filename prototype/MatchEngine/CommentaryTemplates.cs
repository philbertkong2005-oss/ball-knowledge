namespace BallKnowledge.MatchEngine;

internal static class CommentaryTemplates
{
    private static readonly string[] KickoffTemplates =
    [
        "Good afternoon from the touchline. {HOME} and {AWAY} are under way.",
        "The whistle goes and we're off, {HOME} against {AWAY}.",
        "Here we go then, {HOME} kick us off against {AWAY}.",
        "The old set crackles and the match begins: {HOME} versus {AWAY}.",
        "A bright start on the radio, {HOME} and {AWAY} have kicked off.",
    ];

    private static readonly string[] GoalTemplates =
    [
        "Goal for {TEAM}! {SCORER} has buried it.",
        "{SCORER} scores for {TEAM}, and the crowd has found its voice.",
        "It's in for {TEAM}! {SCORER} makes the breakthrough.",
        "{TEAM} strike and {SCORER} was the man in the right place.",
        "{SCORER} turns it home for {TEAM}.",
        "What a finish from {SCORER}; {TEAM} are celebrating.",
        "{TEAM} have their goal and {SCORER} claims it.",
        "That is a proper centre-forward's finish by {SCORER} for {TEAM}.",
        "The net ripples and {SCORER} has scored for {TEAM}.",
        "{TEAM} cash in, {SCORER} does the damage.",
    ];

    private static readonly string[] LateGoalTemplates =
    [
        "Late drama now, {TEAM} have struck through {SCORER}.",
        "Inside the last ten and {SCORER} has turned this on its head for {TEAM} with a late strike.",
        "What a time to score, {SCORER} for {TEAM} with a late goal and the ground erupts.",
        "That could be the late twist of the afternoon, {SCORER} for {TEAM}.",
    ];

    private static readonly string[] SaveTemplates =
    [
        "{KEEPER} gets behind it for {TEAM}.",
        "A sharp stop there by {KEEPER} for {TEAM}.",
        "{KEEPER} saves and keeps {TEAM} in good order.",
        "Not this time, says {KEEPER} for {TEAM}.",
        "{KEEPER} stands up well and keeps it out for {TEAM}.",
        "A strong hand from {KEEPER} for {TEAM}.",
    ];

    private static readonly string[] ChanceTemplates =
    [
        "{SCORER} takes aim for {TEAM} but drags it wide.",
        "{TEAM} work a sight of goal and {SCORER} cannot keep it down.",
        "Half a chance for {TEAM}; {SCORER} snatches at it.",
        "{SCORER} gets loose for {TEAM} but the finish is missing.",
        "{TEAM} threaten again and {SCORER} cannot steer it on target.",
        "{SCORER} had a look there for {TEAM} and wastes it.",
    ];

    private static readonly string[] CornerTemplates =
    [
        "Corner now for {TEAM}.",
        "{TEAM} have forced another corner.",
        "Set piece coming, corner to {TEAM}.",
        "{TEAM} keep the pressure on and win a corner.",
    ];

    private static readonly string[] HalfTimeTemplates =
    [
        "Half-time here: {HOME} {HSCORE}, {AWAY} {ASCORE}.",
        "The whistle for the break goes, it's {HOME} {HSCORE}, {AWAY} {ASCORE}.",
        "At the interval, {HOME} {HSCORE}, {AWAY} {ASCORE}.",
        "We've reached half-time and the board shows {HOME} {HSCORE}, {AWAY} {ASCORE}.",
        "That is the first half done: {HOME} {HSCORE}, {AWAY} {ASCORE}.",
    ];

    private static readonly string[] FullTimeTemplates =
    [
        "Full-time now: {HOME} {HSCORE}, {AWAY} {ASCORE}.",
        "That's the lot, {HOME} {HSCORE}, {AWAY} {ASCORE}.",
        "The final whistle goes with {HOME} {HSCORE}, {AWAY} {ASCORE}.",
        "All over here, {HOME} {HSCORE}, {AWAY} {ASCORE}.",
        "We are done for the day: {HOME} {HSCORE}, {AWAY} {ASCORE}.",
    ];

    public static string RenderKickoff(string home, string away, Random rng)
    {
        return Fill(Pick(KickoffTemplates, rng), ("HOME", home), ("AWAY", away));
    }

    public static string RenderGoal(string team, string scorer, int minute, string? assist, Random rng)
    {
        var template = minute >= 80 ? Pick(LateGoalTemplates, rng) : Pick(GoalTemplates, rng);
        var rendered = Fill(template, ("TEAM", team), ("SCORER", scorer));
        if (!string.IsNullOrWhiteSpace(assist))
        {
            rendered += $" Assist from {assist}.";
        }

        return rendered;
    }

    public static string RenderSave(string team, string keeper, Random rng)
    {
        return Fill(Pick(SaveTemplates, rng), ("TEAM", team), ("KEEPER", keeper));
    }

    public static string RenderChance(string team, string scorer, Random rng)
    {
        return Fill(Pick(ChanceTemplates, rng), ("TEAM", team), ("SCORER", scorer));
    }

    public static string RenderCorner(string team, Random rng)
    {
        return Fill(Pick(CornerTemplates, rng), ("TEAM", team));
    }

    public static string RenderHalfTime(string home, string away, int homeScore, int awayScore, Random rng)
    {
        return Fill(Pick(HalfTimeTemplates, rng), ("HOME", home), ("AWAY", away), ("HSCORE", homeScore.ToString()), ("ASCORE", awayScore.ToString()));
    }

    public static string RenderFullTime(string home, string away, int homeScore, int awayScore, Random rng)
    {
        return Fill(Pick(FullTimeTemplates, rng), ("HOME", home), ("AWAY", away), ("HSCORE", homeScore.ToString()), ("ASCORE", awayScore.ToString()));
    }

    public static string RenderFactorLeak(AppliedFactor factor, string home, string away)
    {
        var line = factor.Name switch
        {
            "striker knock" => $"{factor.TeamName} look short of a yard up front already.",
            "striker ruled out" => $"{factor.TeamName} have had to reshuffle a key attacking name before the off.",
            "keeper hungover" => $"{factor.TeamName}'s keeper looks anything but settled.",
            "keeper elite form" => $"{factor.TeamName}'s keeper is carrying himself like a man in top nick.",
            "playmaker suspended" => $"{factor.TeamName} are missing their usual orchestra leader in midfield.",
            "hot streak" => $"{factor.TeamName} have one player brimming with confidence this week.",
            "cold streak" => $"{factor.TeamName} have a player who looks a touch off his game.",
            "training bust-up" => $"{factor.TeamName} do not look entirely harmonious in the warm-up.",
            "model pro" => $"{factor.TeamName} have a lad out there who looks razor sharp.",
            "winning morale" => $"{factor.TeamName} have the air of a side enjoying itself.",
            "losing crisis" => $"{factor.TeamName} sound like a club carrying too much noise.",
            "cup rotation" => $"{factor.TeamName} have clearly left one notable attacker out of the starting side.",
            "surprise formation" => $"{factor.TeamName} appear to have sprung an unfamiliar shape on us.",
            "new-manager bounce" => $"{factor.TeamName} are playing with fresh urgency today.",
            "pay dispute" => $"{factor.TeamName} do not look entirely at peace with themselves.",
            "waterlogged pitch" => "The waterlogged pitch is holding the ball up badly and it could turn scrappy.",
            "high wind" => "The high wind is bothering anything hit from distance.",
            "derby" => $"This one has the feel of a derby, {home} and {away} both cagey.",
            "dead rubber" => "There is a dead rubber looseness to the contest so far.",
            _ => $"{factor.TeamName ?? home} have a wrinkle the board could never tell you about.",
        };

        if (!line.Contains(factor.LeakToken, StringComparison.OrdinalIgnoreCase))
        {
            line += $" [{factor.LeakToken}]";
        }

        return line;
    }

    private static string Pick(IReadOnlyList<string> templates, Random rng) => templates[rng.Next(templates.Count)];

    private static string Fill(string template, params (string Key, string Value)[] values)
    {
        foreach (var (key, value) in values)
        {
            template = template.Replace($"{{{key}}}", value, StringComparison.Ordinal);
        }

        return template;
    }
}
