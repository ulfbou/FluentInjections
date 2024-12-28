// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;

using Moq;

namespace FluentInjections.Tests.Utilities.Logging;

/// <summary>
/// Provides logging mocks for testing.
/// </summary>
public static class LoggingUtilities
{
    public static ILogger Logger { get; } = new Mock<ILogger>().Object;
    public static ILoggerFactory LoggerFactory { get; } = new Mock<ILoggerFactory>().Object;
    public static Mock<ILogger<T>> CreateLoggerMock<T>() => new Mock<ILogger<T>>();
    public static Mock<ILogger<T>> CreateLoggerMock<T>(ILogger<T> logger) => new Mock<ILogger<T>>();
}
