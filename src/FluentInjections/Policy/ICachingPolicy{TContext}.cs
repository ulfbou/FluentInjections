using FluentInjections.Context;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentInjections.Policy;

public interface ICachingPolicy<TContext> : ICachingPolicy where TContext : IContext { }
