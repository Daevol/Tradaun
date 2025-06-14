using Domain;

namespace Application;

public class SignalService : ISignalService
{
    public OrderAction? GenerateSignal(Features f)
    {
        if (f.ZScoreBasis > 1.5 && f.Sentiment == Sentiment.Bull)
            return OrderAction.Buy;
        if (f.ZScoreBasis < -1.5 && f.Sentiment == Sentiment.Bear)
            return OrderAction.Sell;
        return null;
    }
}
