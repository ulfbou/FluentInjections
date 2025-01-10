// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace FluentInjections.Context;

public interface IContext
{
    IDictionary<string, object> Data { get; }
    void AddOrUpdate(string key, object value);
    bool TryGetValue<T>(string key, out T value);
    bool Remove(string key);
}
