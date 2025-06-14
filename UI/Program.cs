using Application;
using Domain;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Prometheus;
using QuikSharp;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: true);
builder.Services.AddSingleton<ISentimentAnalyzer, SentimentAnalyzer>();
builder.Services.AddSingleton<IFeaturePipeline, FeaturePipeline>();
builder.Services.AddSingleton<ISignalService, SignalService>();
builder.Services.AddSingleton<IRiskGuardian, RiskGuardian>();
builder.Services.AddSingleton<Quik>();
builder.Services.AddSingleton<IOrderExecutor, OrderExecutor>();
builder.Services.AddSingleton<INewsSource>(sp =>
    new NewsRssService(sp.GetRequiredService<IConfiguration>().GetSection("NewsFeeds").Get<string[]>() ?? Array.Empty<string>()));
builder.Services.AddSingleton<IMarketData, MarketDataService>();
builder.Services.AddLogging(l => l.ClearProviders().AddProvider(new PrometheusLoggerProvider()));
builder.Services.AddHostedService<BotService>();

var host = builder.Build();
var metricServer = new KestrelMetricServer(port:1234);
metricServer.Start();
await host.RunAsync();

public class BotService : BackgroundService
{
    private readonly IFeaturePipeline _pipeline;
    private readonly ISignalService _signals;
    private readonly IOrderExecutor _executor;
    private readonly IRiskGuardian _risk;
    private readonly INewsSource _news;
    private readonly IMarketData _market;
    private readonly ILogger<BotService> _logger;

    public BotService(IFeaturePipeline pipeline, ISignalService signals, IOrderExecutor executor,
        IRiskGuardian risk, INewsSource news, IMarketData market, ILogger<BotService> logger)
    {
        _pipeline = pipeline;
        _signals = signals;
        _executor = executor;
        _risk = risk;
        _news = news;
        _market = market;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        decimal equity = 100000;
        while (!stoppingToken.IsCancellationRequested)
        {
            var (quote, l2, trade) = await _market.GetMarketDataAsync("TQBR", stoppingToken);
            var (brent, usd) = await _market.GetExternalDriversAsync(stoppingToken);
            var news = await _news.GetLatestAsync(stoppingToken);
            var features = _pipeline.Update(quote, l2, trade, brent, usd, news);
            var signal = _signals.GenerateSignal(features);
            if (signal != null && _risk.CheckRisk(0, 0, equity))
            {
                await _executor.ExecuteAsync(signal.Value, quote, stoppingToken);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}
