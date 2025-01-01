// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;

namespace FluentInjections.Internal.Configurators;

/// <summary>
/// Represents a configurator that provides methods to bind and configure services within the application.
/// </summary>
internal abstract class Configurator<TBinding, TDescriptor> : IConfigurator<TBinding>, IDisposable where TBinding : IBinding
{
    protected readonly List<TDescriptor> _bindings = new();
    protected readonly ILogger _logger;
    internal IReadOnlyList<TDescriptor> Bindings => _bindings;
    public ConflictResolutionMode ConflictResolution { get; set; }

    internal Configurator(ILogger logger)
    {
        ConflictResolution = ConflictResolutionMode.WarnAndReplace;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public void Register()
    {
        ValidateBindings();

        foreach (var binding in _bindings)
        {
            Register(binding);
        }
    }

    /// <inheritdoc />
    protected abstract void Register(TDescriptor binding);

    /// <inheritdoc />
    protected abstract void ValidateBindings();

    /// <inheritdoc />
    public void Dispose()
    {
        _bindings.Clear();
    }
}
