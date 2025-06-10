// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatchFailure.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Reactive;

using Sundew.DiscriminatedUnions;

/// <summary>
/// Represents the result of an operation that failed to find a matching result.
/// </summary>
/// <remarks>This enumeration is used to indicate specific failure scenarios where an operation completes
/// successfully but does not produce a matching result. It can be used to distinguish between different types of
/// outcomes in pattern matching or search operations.</remarks>
[DiscriminatedUnion]
public enum MatchFailure
{
    /// <summary>
    /// Indicates that the operation completed successfully but no matching result was found.
    /// </summary>
    CompletedWithoutMatch,
}