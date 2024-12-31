// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace FluentInjections.Tests.Internal.Services;

public class FailingService : IFailingService
{
    public FailingService()
    {
        throw new Exception("FailingService");
    }
}
