// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObservableCacheExtensionsTests.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Reactive.Tests;

using System;
using System.Threading.Tasks;
using AwesomeAssertions;
using DynamicData;
using NUnit.Framework;
using Sundew.Base;

[TestFixture]
public class ObservableCacheExtensionsTests
{
    [Test]
    public async Task MatchAsync_When_CacheAlreadyFulfillsPredicate_Then_ResultIsExpectedResult()
    {
        var sourceCache = new SourceCache<string, int>(x => x.Length);
        sourceCache.Edit(x => x.AddOrUpdate("1"));
        sourceCache.Edit(x => x.AddOrUpdate("22"));
        sourceCache.Edit(x => x.AddOrUpdate("333"));

        var resultTask = sourceCache.MatchAsync(cache =>
        {
            return cache switch
            {
                [var first, var second, ..] => R.Success((first, second)),
                _ => R.Error().Omits<(string, string)>(),
            };
        });

        var result = await resultTask;

        using (Assert.EnterMultipleScope())
        {
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(("1", "22"));
        }
    }

    [Test]
    public async Task MatchAsync_When_PredicateSucceeds_Then_ResultIsExpectedResult()
    {
        var sourceCache = new SourceCache<string, int>(x => x.Length);

        var resultTask = sourceCache.MatchAsync(cache =>
        {
            return cache switch
            {
                [var first, var second, ..] => R.Success((first, second)),
                _ => R.Error().Omits<(string, string)>(),
            };
        });
        sourceCache.Edit(x => x.AddOrUpdate("1"));
        sourceCache.Edit(x => x.AddOrUpdate("22"));
        sourceCache.Edit(x => x.AddOrUpdate("333"));

        var result = await resultTask;

        using (Assert.EnterMultipleScope())
        {
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(("1", "22"));
        }
    }

    [Test]
    public async Task MatchAsync_When_PredicateNeverSucceeds_Then_ResultIsTimeout()
    {
        var sourceCache = new SourceCache<string, int>(x => x.Length);

        var resultTask = sourceCache.MatchAsync(
            cache =>
            {
                return cache switch
                {
                    [var first, var second] => R.Success((first, second)),
                    _ => R.Error().Omits<(string, string)>(),
                };
            },
            new Cancellation(TimeSpan.FromMilliseconds(50)));

        sourceCache.Edit(x => x.AddOrUpdate("1"));

        var result = await resultTask;

        using (Assert.EnterMultipleScope())
        {
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(Failure<MatchFailure>._Canceled(CancelReason.Timeout));
        }
    }
}