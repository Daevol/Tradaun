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
