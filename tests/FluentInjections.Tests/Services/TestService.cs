// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace FluentInjections.Tests.Services;

internal sealed class TestService : ITestService
{
    public string Param1 { get; private set; }
    public int Param2 { get; private set; }

    public TestService() : this(string.Empty, 0) { }

    public TestService(string param1, int param2)
    {
        Param1 = param1;
        Param2 = param2;
    }

    public class TestServiceOptions
    {
        public string Param1 { get; set; } = string.Empty;
        public int Param2 { get; set; }
    }

    public void DoSomething()
    {
        Param1 = "something";
    }
}

internal sealed class TestServiceWithDefaultValues : ITestService
{
    public string Param1 { get; private set; }
    public int Param2 { get; private set; }

    public TestServiceWithDefaultValues()
    {
        Param1 = "default";
        Param2 = -1;
    }

    public void DoSomething()
    {
        Param1 = "something";
    }
}

internal sealed class TestServiceWithDependency : ITestService
{
    public string Param1 { get; private set; }
    public int Param2 { get; private set; }
    public TestServiceWithDependency(IExampleService exampleService)
    {
        Param1 = "dependency";
        Param2 = 42;
    }
    public void DoSomething()
    {
        Param1 = "something";
    }
}

public interface IExampleService { }

internal class ExampleService : IExampleService { }

internal class AnotherExampleService : IExampleService { }

internal class ComplexService
{
    public string Param1 { get; }
    public int Param2 { get; }

    public ComplexService(string param1, int param2)
    {
        Param1 = param1;
        Param2 = param2;
    }
}
