using Serilog.Core;
using Serilog.Events;
namespace Aether.Api;
// No reflection-based object destructuring in the native host. Log explicit scalar fields.
public sealed class ScalarLogPolicy : IDestructuringPolicy
{
    public bool TryDestructure(object value, ILogEventPropertyValueFactory factory, out LogEventPropertyValue result)
    {
        result = new ScalarValue(value.ToString());
        return true;
    }
}
