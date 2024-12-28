// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;

using Moq;

using System;
using System.Linq.Expressions;

namespace FluentInjections.Tests.Extensions;

public static class LoggerExtensions
{
    public static void VerifyLog<T>(this Mock<ILogger<T>> loggerMock, LogLevel[] logLevels, string message, Times? times = null)
    {
        var timesToUse = times ?? Times.Once();
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => logLevels.Contains(l)),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            timesToUse);
    }
    public static void VerifyLog<T>(this Mock<ILogger<T>> loggerMock, LogLevel logLevel, string message, Exception expectedException)
    {
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == logLevel),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                It.Is<Exception>(ex => ex.GetType() == expectedException.GetType()),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once());
    }

    public static void VerifyLog<T>(this Mock<ILogger<T>> loggerMock, LogLevel logLevel, string message, Times? times = null)
    {
        var timesToUse = times ?? Times.Once();
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == logLevel),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            timesToUse);
    }

    public static void VerifyLog<T>(this Mock<ILogger<T>> loggerMock, LogLevel logLevel, Expression<Func<string, bool>> messageExpression, Times? times = null)
    {
        var timesToUse = times ?? Times.Once();

        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == logLevel),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => messageExpression.Compile().Invoke(v.ToString()!)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            timesToUse);
    }
}
