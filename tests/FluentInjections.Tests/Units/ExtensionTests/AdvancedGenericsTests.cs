// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace FluentInjections.Tests.Units.ExtensionTests;

public class AdvancedGenericsTests
{
    [Fact]
    public void OpenGenericInterface_CanInstantiateClosedGenericClass()
    {
        // Arrange
        IRepository<int> intRepo = new ConcreteRepository<int>();

        // Act & Assert
        Assert.NotNull(intRepo);
    }

    [Fact]
    public void OpenGenericClass_CanInstantiateClosedGenericClass()
    {
        // Arrange
        var genericClass = new GenericClass<string>();

        // Act & Assert
        Assert.NotNull(genericClass);
    }

    [Fact]
    public void GenericInterface_WithConstraints_EnforcesRules()
    {
        // Arrange 
        IProcessor<DerivedClass> processor = new ConcreteProcessor<DerivedClass>();

        // Act & Assert
        Assert.NotNull(processor);

        // This should fail at compile time 
        // IProcessor<BaseClass> invalidProcessor = new ConcreteProcessor<DerivedClass>(); 
    }

    [Fact]
    public void GenericClass_WithConstraints_EnforcesRules()
    {
        // Arrange
        var constrainedClass = new ConstrainedClass<DerivedClass>();

        // Act & Assert
        Assert.NotNull(constrainedClass);

        // This should fail at compile time
        // var invalidConstrainedClass = new ConstrainedClass<BaseClass>(); 
    }

    [Fact]
    public void Covariance_InInterface()
    {
        // Arrange
        IEnumerable<DerivedClass> derivedList = new List<DerivedClass>();
        IEnumerable<BaseClass> baseList = derivedList; // Covariance allowed

        // Act & Assert
        Assert.NotNull(baseList);
    }

    [Fact]
    public void Contravariance_InInterface()
    {
        // Arrange
        Action<BaseClass> baseAction = (BaseClass obj) => { /* Do something */ };
        Action<DerivedClass> derivedAction = baseAction; // Contravariance allowed

        // Act & Assert
        derivedAction(new DerivedClass());
    }

    private class ConcreteProcessor<T> : IProcessor<DerivedClass>
    {
    }
}
