using Application;
using Domain;
using Microsoft.Extensions.Logging;
using QuikSharp;

namespace Infrastructure;

public class FeaturePipeline : IFeaturePipeline
{
    private readonly Queue<decimal> _deltas = new();
    private readonly Queue<decimal> _basis = new();
    private readonly int _window = 20;

    public Features Update(Quote l1, Level2Quote l2, Trade trade, decimal brent, decimal usdRub, string[] news)
    {
        decimal delta = l1.Ask - l1.Bid;
        _deltas.Enqueue(delta);
        if (_deltas.Count > _window) _deltas.Dequeue();
        decimal meanDelta = _deltas.Any() ? _deltas.Average() : 0m;

        decimal basis = l1.Ask - brent * usdRub;
        _basis.Enqueue(basis);
        if (_basis.Count > _window) _basis.Dequeue();
        double zScore = 0;
        if (_basis.Count >= 2)
        {
            double mean = (double)_basis.Average();
            double sd = Math.Sqrt(_basis.Select(b => Math.Pow((double)b - mean, 2)).Sum() / _basis.Count);
            if (sd > 1e-9) zScore = ((double)basis - mean) / sd;
        }

        decimal volatility = _deltas.Count >= 2
            ? (decimal)Math.Sqrt(_deltas.Select(d => Math.Pow((double)(d - meanDelta), 2)).Sum() / _deltas.Count)
            : 0m;

        var sentiment = news.Any(n => n.Contains("bull", StringComparison.OrdinalIgnoreCase)) ? Sentiment.Bull :
            news.Any(n => n.Contains("bear", StringComparison.OrdinalIgnoreCase)) ? Sentiment.Bear : Sentiment.Neutral;

        return new Features(delta, zScore, (double)volatility, sentiment);
    }
}

public class OrderExecutor : IOrderExecutor
{
    private readonly Quik _quik;
    private readonly ILogger<OrderExecutor> _logger;

    public OrderExecutor(Quik quik, ILogger<OrderExecutor> logger)
    {
        _quik = quik;
        _logger = logger;
    }

    public async Task ExecuteAsync(OrderAction action, Quote quote, CancellationToken token)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        cts.CancelAfter(TimeSpan.FromSeconds(3));
        _logger.LogInformation("Placing {action} order at {price}", action, quote.Ask);
        await Task.Delay(100, cts.Token);
        _logger.LogInformation("Order executed");
    }
}

public class RiskGuardian : IRiskGuardian
{
    private readonly Queue<decimal> _returns = new();
    private readonly int _window = 100;

    public bool CheckRisk(decimal pnl, decimal slippage, decimal equity)
    {
        _returns.Enqueue(pnl);
        if (_returns.Count > _window) _returns.Dequeue();
        if (equity <= 0) return false;
        var sorted = _returns.OrderBy(x => x).ToList();
        int index = (int)(0.01 * sorted.Count);
        decimal var99 = sorted.ElementAtOrDefault(index);
        if (pnl + var99 < -0.007m * equity) return false;
        if (slippage > 0.004m) return false;
        return true;
    }
}
