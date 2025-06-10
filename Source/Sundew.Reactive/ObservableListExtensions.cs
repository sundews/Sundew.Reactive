// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObservableListExtensions.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Reactive;

using System;
using System.Collections.Generic;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using DynamicData;
using Sundew.Base;

/// <summary>
/// Extensions for <see cref="IObservableList{TItem}"/> with easy to use methods.
/// </summary>
public static class ObservableListExtensions
{
    /// <summary>
    /// Matches the items in the observable list using the specified match function and returns a task that completes when a match is found or the observable list completes without a match.
    /// </summary>
    /// <typeparam name="TItem">The item type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="observableList">The observable list.</param>
    /// <param name="matchFunc">The match function.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public static Task<R<TResult, Failure<MatchFailure>>> MatchAsync<TItem, TResult>(this IObservableList<TItem> observableList, Func<IReadOnlyList<TItem>, R<TResult>> matchFunc)
        where TResult : notnull
        where TItem : notnull
    {
        return observableList.MatchAsync(matchFunc, Cancellation.None);
    }

    /// <summary>
    /// Matches the items in the observable list using the specified match function and returns a task that completes when a match is found or the observable list completes without a match.
    /// </summary>
    /// <typeparam name="TItem">The item type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="observableList">The observable list.</param>
    /// <param name="matchFunc">The match function.</param>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public static Task<R<TResult, Failure<MatchFailure>>> MatchAsync<TItem, TResult>(this IObservableList<TItem> observableList, Func<IReadOnlyList<TItem>, R<TResult>> matchFunc, Cancellation cancellation)
        where TResult : notnull
        where TItem : notnull
    {
        var taskCompletionSource = new TaskCompletionSource<R<TResult, Failure<MatchFailure>>>();
        var enabler = cancellation.EnableCancellation();
        enabler.Register(x => taskCompletionSource.TrySetResult(R.Error(Failure<MatchFailure>._Canceled(x))));
        _ = observableList.Connect().ForEachChange(change =>
        {
            var matchResult = matchFunc(observableList.Items);
            if (matchResult.TryGet(out var result))
            {
                taskCompletionSource.TrySetResult(R.Success(result));
            }
        }).ToTask(cancellation.Token)
            .ContinueWith(
                x =>
                {
                    enabler.Dispose();
                    if (enabler.IsCancellationRequested)
                    {
                        taskCompletionSource.TrySetResult(R.Error(Failure<MatchFailure>._Canceled(enabler.CancelReason.Value)));
                    }
                    else if (!taskCompletionSource.Task.IsCompleted)
                    {
                        taskCompletionSource.TrySetResult(x is { IsFaulted: true, Exception: not null }
                            ? R.Error(Failure<MatchFailure>._ExceptionOccured(x.Exception))
                            : R.Error(Failure<MatchFailure>._Failed(MatchFailure.CompletedWithoutMatch)));
                    }
                },
                TaskScheduler.Default);
        var initialMatch = matchFunc(observableList.Items);
        if (initialMatch.TryGet(out var initialResult))
        {
            taskCompletionSource.TrySetResult(R.Success(initialResult));
            enabler.Cancel();
        }

        return taskCompletionSource.Task;
    }
}