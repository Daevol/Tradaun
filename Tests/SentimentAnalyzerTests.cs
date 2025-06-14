using Infrastructure;

namespace Tests;

public class SentimentAnalyzerTests
{
    [Test]
    public void DetectsBullishSentiment()
    {
        var analyzer = new SentimentAnalyzer();
        var sentiment = analyzer.Analyze(new[] { "profit growth" });
        Assert.That(sentiment, Is.EqualTo(Domain.Sentiment.Bull));
    }

    [Test]
    public void DetectsBearishSentiment()
    {
        var analyzer = new SentimentAnalyzer();
        var sentiment = analyzer.Analyze(new[] { "loss scandal" });
        Assert.That(sentiment, Is.EqualTo(Domain.Sentiment.Bear));
    }
}
