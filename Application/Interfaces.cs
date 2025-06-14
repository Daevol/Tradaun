using Domain;

namespace Application;

public record Features(decimal Delta, double ZScoreBasis, double Volatility, Sentiment Sentiment);

public interface IFeaturePipeline
{
    Features Update(Quote l1, Level2Quote l2, Trade trade, decimal brent, decimal usdRub, string[] news);
}

public interface ISignalService
{
    OrderAction? GenerateSignal(Features features);
}

public interface IOrderExecutor
{
    Task ExecuteAsync(OrderAction action, Quote quote, CancellationToken token);
}

public interface IRiskGuardian
{
    bool CheckRisk(decimal pnl, decimal slippage, decimal equity);
}

public interface INewsSource
{
    Task<string[]> GetLatestAsync(CancellationToken token);
}

public interface ISentimentAnalyzer
{
    Sentiment Analyze(string[] news);
}

public interface IMarketData
{
    Task<(Quote l1, Level2Quote l2, Trade trade)> GetMarketDataAsync(string instrument, CancellationToken token);
    Task<(decimal brent, decimal usdRub)> GetExternalDriversAsync(CancellationToken token);
}
