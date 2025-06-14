using Application;
using Domain;

namespace Tests;

public class SignalServiceTests
{
    [Test]
    public void GeneratesBuySignal()
    {
        var service = new SignalService();
        var features = new Features(0, 2.0, 0, Sentiment.Bull);
        var signal = service.GenerateSignal(features);
        Assert.That(signal, Is.EqualTo(OrderAction.Buy));
    }

    [Test]
    public void GeneratesSellSignal()
    {
        var service = new SignalService();
        var features = new Features(0, -2.0, 0, Sentiment.Bear);
        var signal = service.GenerateSignal(features);
        Assert.That(signal, Is.EqualTo(OrderAction.Sell));
    }

    [Test]
    public void GeneratesNoSignal()
    {
        var service = new SignalService();
        var features = new Features(0, 0, 0, Sentiment.Neutral);
        var signal = service.GenerateSignal(features);
        Assert.That(signal, Is.Null);
    }
}
