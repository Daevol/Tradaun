namespace Domain;

public record Quote(string Instrument, decimal Bid, decimal Ask, DateTime Timestamp);

public record Level2Quote(string Instrument, IReadOnlyList<(decimal Price, decimal Volume)> Bids,
    IReadOnlyList<(decimal Price, decimal Volume)> Asks, DateTime Timestamp);

public record Trade(string Instrument, decimal Price, decimal Volume, DateTime Timestamp);

public enum Sentiment
{
    Neutral,
    Bull,
    Bear
}

public enum OrderAction
{
    Buy,
    Sell
}
