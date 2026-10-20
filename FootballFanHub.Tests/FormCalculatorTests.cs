using FootballFanHub.Models;
using FootballFanHub.Services;
using Xunit;

namespace FootballFanHub.Tests;

public class FormCalculatorTests
{
    [Fact]
    public void ResultFor_ReturnsWin_WhenTeamScoresMoreAsHomeTeam()
    {
        var match = CreateMatch(homeTeamId: 1, awayTeamId: 2, homeGoals: 3, awayGoals: 1);

        var result = FormCalculator.ResultFor(match, teamId: 1);

        Assert.Equal('W', result);
    }

    [Fact]
    public void ResultFor_ReturnsLoss_WhenTeamScoresLessAsAwayTeam()
    {
        var match = CreateMatch(homeTeamId: 1, awayTeamId: 2, homeGoals: 2, awayGoals: 0);

        var result = FormCalculator.ResultFor(match, teamId: 2);

        Assert.Equal('L', result);
    }

    [Fact]
    public void ResultFor_ReturnsDraw_WhenScoresAreEqual()
    {
        var match = CreateMatch(homeTeamId: 1, awayTeamId: 2, homeGoals: 2, awayGoals: 2);

        var result = FormCalculator.ResultFor(match, teamId: 1);

        Assert.Equal('D', result);
    }

    [Fact]
    public void ResultFor_ReturnsQuestionMark_WhenEitherScoreIsMissing()
    {
        var match = CreateMatch(homeTeamId: 1, awayTeamId: 2, homeGoals: null, awayGoals: 1);

        var result = FormCalculator.ResultFor(match, teamId: 1);

        Assert.Equal('?', result);
    }

    [Fact]
    public void GetForm_ReturnsMostRecentResultsFirst_AndLimitsTheCount()
    {
        var matches = new[]
        {
            CreateMatch(1, 2, 1, 0, new DateTime(2026, 1, 1)),
            CreateMatch(1, 3, 0, 2, new DateTime(2026, 1, 3)),
            CreateMatch(1, 4, 2, 2, new DateTime(2026, 1, 2)),
            CreateMatch(1, 5, 3, 0, new DateTime(2026, 1, 4))
        };

        var result = FormCalculator.GetForm(matches, teamId: 1, count: 3);

        Assert.Equal(new[] { 'W', 'L', 'D' }, result);
    }

    private static MatchSummary CreateMatch(
        int homeTeamId,
        int awayTeamId,
        int? homeGoals,
        int? awayGoals,
        DateTime? date = null) => new()
        {
            HomeTeamId = homeTeamId,
            AwayTeamId = awayTeamId,
            HomeGoals = homeGoals,
            AwayGoals = awayGoals,
            UtcDate = date ?? DateTime.UtcNow
        };
}
