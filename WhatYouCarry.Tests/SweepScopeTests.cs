using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The seed count of a sweep on a pull request and on main (D-480, D-481).</summary>
public sealed class SweepScopeTests
{
    [Fact]
    public void PullRequestRunsOneFifthAndMainRunsTheFullCount()
    {
        Assert.Equal(200, SweepScope.Seeds(1000, "1"));
        Assert.Equal(1000, SweepScope.Seeds(5000, "1"));
        Assert.Equal(20, SweepScope.Seeds(100, "1"));
        Assert.Equal(1000, SweepScope.Seeds(1000, "0"));
        Assert.Equal(1000, SweepScope.Seeds(1000, null));
    }

    [Theory]
    [InlineData("")]
    [InlineData("true")]
    [InlineData("2")]
    public void UnknownValueFailsAndNamesTheVariable(string value)
    {
        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => SweepScope.Seeds(1000, value));

        Assert.Contains(SweepScope.PullRequestVariable, error.Message, StringComparison.Ordinal);
        Assert.Contains($"'{value}'", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void CountThatDoesNotDivideFails()
    {
        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => SweepScope.Seeds(1001, "1"));

        Assert.Contains("1001", error.Message, StringComparison.Ordinal);
    }

    /// <summary>xUnit reads no trait of an outer class on a nested class, so each nested class of a sweep class carries its own (D-478, D-479).</summary>
    [Fact]
    public void EveryNestedClassOfASweepClassTakesTheCategory()
    {
        Type[] sweepClasses = [typeof(ProcgenTests), typeof(ReplayTests), typeof(CameraTests), typeof(BotTests)];
        List<Type> checkedTypes = [];
        foreach (Type sweepClass in sweepClasses)
        {
            checkedTypes.Add(sweepClass);
            checkedTypes.AddRange(sweepClass.GetNestedTypes().Where(nested => nested.IsClass && !nested.IsAbstract));
        }

        foreach (Type type in checkedTypes)
        {
            bool tagged = CustomAttributeData.GetCustomAttributes(type).Any(attribute =>
                attribute.AttributeType == typeof(TraitAttribute)
                && (string?)attribute.ConstructorArguments[0].Value == "Category"
                && (string?)attribute.ConstructorArguments[1].Value == SweepScope.SweepCategory);
            Assert.True(tagged, $"The class '{type.FullName}' lacks the category '{SweepScope.SweepCategory}' (D-479).");
        }

        Assert.Equal(8, checkedTypes.Count);
    }

    [Fact]
    public void CiSetsTheVariableOnEveryTestJob()
    {
        string workflow = RepositoryRoot.ReadFile(".github/workflows/ci.yml");

        Assert.Contains($"{SweepScope.PullRequestVariable}: ${{{{ github.event_name == 'pull_request' && '1' || '0' }}}}", workflow, StringComparison.Ordinal);
    }
}
