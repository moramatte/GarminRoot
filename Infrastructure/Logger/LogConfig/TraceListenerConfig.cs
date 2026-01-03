using Infrastructure;
using Infrastructure.Collections;
using Infrastructure.Logger;
using Infrastructure.Logger.Enterprise;
using Infrastructure.Logger.Formatters;
using Infrastructure.Logger.Tracers;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Awos7.WindowsService.LogConfig;

public enum TraceListeners
{
    Console,
    InternalEvent,
    RollingFile,
    Udp,
    Memory,
    Loggly,
    Log4NetFile,
    InMemory,
    LogObject,
}

[Serializable]
public readonly record struct ConfigValue()
{
    public readonly string Name { get; init; }
    public readonly string Value { get; init; }

    public ConfigValue(string name, int value) : this(name, value.ToString()) { }
    public ConfigValue(string name, string value) : this()
    {
        this.Name = name;
        this.Value = value;
    }
}

[Serializable]
public readonly record struct TraceListenerConfig 
{
    public readonly string Name { get; init; }
    public readonly TraceListeners Type { get; init; }
    public readonly TraceEventType Filter { get; init; }
    public readonly string Formatter { get; init; }
    public readonly EquatableList<ConfigValue> Values { get; init; }

    public TraceListenerConfig()
    {
        Values = new EquatableList<ConfigValue>();
    }

    public TraceFilter GetFilter()
    {
        return new LogLevelFilter(Filter);
    }

    public ILogFormatter GetFormatter()
    {
        if (Formatter == null)
            return new RawFormatter();

        return ServiceLocator.Resolve<IFormatterRepository>().Deserialize(Formatter);
    }

    public T Get<T>(string name)
    {
        var val = Values.List.Find(x => x.Name == name).Value;
        return (T)Convert.ChangeType(val, typeof(T));
    }

    public void Set<T>(string name, T value)
    {
        Values.List.Add(new ConfigValue() { Name = name, Value = value.ToString() });
    }

    public ITraceListener[] Build()
    {
        return Type switch
        {
            TraceListeners.RollingFile => [new RollingFlatFileTraceListener(this)],
            TraceListeners.Udp => [new UdpTraceListener(this)],
            TraceListeners.Console => [new CustomConsoleTraceListener(this)],
            TraceListeners.Memory => [new LogObjectTraceListener(this with { Formatter = "Awos" }), new InMemoryTraceListener(this with { Formatter = "AwosMemoryFormat" })],
            TraceListeners.Log4NetFile => [new Log4NetFileTraceListener(this)],
            TraceListeners.Loggly => [new LogglyTraceListener(this)],
            TraceListeners.InMemory => [new InMemoryTraceListener(this)],
            TraceListeners.LogObject => [new LogObjectTraceListener(this)],
            _ => throw new NotImplementedException($"Unidentified trace listener {Type}"),
        };
    }
}
