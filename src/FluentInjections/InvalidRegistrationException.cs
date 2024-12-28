// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace FluentInjections;

[Serializable]
internal class InvalidRegistrationException : Exception
{
    public InvalidRegistrationException()
    {
    }

    public InvalidRegistrationException(string? message) : base(message)
    {
    }

    public InvalidRegistrationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}