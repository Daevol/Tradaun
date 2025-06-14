using Microsoft.Extensions.Logging;
using Prometheus;

namespace Infrastructure;

public class PrometheusLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new PrometheusLogger(categoryName);
    public void Dispose() { }
}

public class PrometheusLogger : ILogger
{
    private readonly Counter _counter;
    public PrometheusLogger(string category)
    {
        _counter = Metrics.CreateCounter($"{category.Replace('.', '_')}_logs_total", "log entries", new CounterConfiguration
        {
            LabelNames = new[] { "level" }
        });
    }

    public IDisposable? BeginScope<TState>(TState state) => null;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _counter.WithLabels(logLevel.ToString()).Inc();
        Console.WriteLine(formatter(state, exception));
    }
}
