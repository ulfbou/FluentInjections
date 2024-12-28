﻿// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace FluentInjections.Tests.Services;

public interface ITestService
{
    string Param1 { get; }
    int Param2 { get; }

    void DoSomething();
}
