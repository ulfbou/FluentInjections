// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace FluentInjections.Tests.Units.ExtensionTests;

public class TryConvertToTests
{
    [Theory]
    [InlineData(10, typeof(int), 10)]
    [InlineData(10.5f, typeof(double), 10.5)]
    [InlineData("true", typeof(bool), true)]
    [InlineData("hello", typeof(string), "hello")]
    public void SuccessfulConversions(object value, Type targetType, object expectedResult)
    {
        // Act
        var result = TryConvertTo(value, targetType);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(expectedResult, result.Value);
    }

    [Theory]
    [InlineData("invalid", typeof(int))]
    [InlineData(10, typeof(string))]
    [InlineData(null, typeof(int))]
    public void UnsuccessfulConversions(object value, Type targetType)
    {
        // Act
        var result = TryConvertTo(value, targetType);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Value);
    }

    private static (bool Success, object? Value) TryConvertTo(object value, Type targetType)
    {
        try
        {
            var convertedValue = Convert.ChangeType(value, targetType);
            return (true, convertedValue);
        }
        catch (InvalidCastException)
        {
            return (false, null);
        }
    }
}
