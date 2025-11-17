// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObservableListExtensionsTests.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Reactive.Development.Tests;

using System;
using System.Threading.Tasks;
using AwesomeAssertions;
using DynamicData;
using NUnit.Framework;
using Sundew.Base;

[TestFixture]
public class ObservableListExtensionsTests
{
    [Test]
    public async Task MatchAsync_When_ListAlreadyFulfillsPredicate_Then_ResultIsExpectedResult()
    {
        var sourceList = new SourceList<int>();
        sourceList.Add(1);
        sourceList.Add(2);
        sourceList.Add(3);

        var resultTask = sourceList.MatchAsync(list =>
        {
            return list switch
            {
                [var first, var second, ..] => R.Success((first, second)),
                _ => R.Error().Omits<(int, int)>(),
            };
        });

        var result = await resultTask;

        using (Assert.EnterMultipleScope())
        {
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be((1, 2));
        }
    }

    [Test]
    public async Task MatchAsync_When_PredicateSucceeds_Then_ResultIsExpectedResult()
    {
        var sourceList = new SourceList<int>();

        var resultTask = sourceList.MatchAsync(list =>
        {
            return list switch
            {
                [var first, var second, ..] => R.Success((first, second)),
                _ => R.Error().Omits<(int, int)>(),
            };
        });
        sourceList.Add(1);
        sourceList.Add(2);
        sourceList.Add(3);

        var result = await resultTask;

        using (Assert.EnterMultipleScope())
        {
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be((1, 2));
        }
    }

    [Test]
    public async Task MatchAsync_When_PredicateNeverSucceeds_Then_ResultIsTimeout()
    {
        var sourceList = new SourceList<int>();

        var resultTask = sourceList.MatchAsync(
            list =>
            {
                return list switch
                {
                    [var first, var second] => R.Success((first, second)),
                    _ => R.Error().Omits<(int, int)>(),
                };
            },
            new Cancellation(TimeSpan.FromMilliseconds(50)));
        sourceList.Add(1);

        var result = await resultTask;

        using (Assert.EnterMultipleScope())
        {
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(Failure<MatchFailure>._Canceled(CancelReason.Timeout));
        }
    }
}