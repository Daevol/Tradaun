using Application;
using Domain;
using System;
using System.ServiceModel.Syndication;
using System.Xml;
using Microsoft.ML;
using QuikSharp;

namespace Infrastructure;

public class NewsRssService : INewsSource
{
    private readonly string[] _urls;
    private readonly HttpClient _client = new();

    public NewsRssService(IEnumerable<string> urls)
    {
        _urls = urls.ToArray();
    }

    public async Task<string[]> GetLatestAsync(CancellationToken token)
    {
        var list = new List<string>();
        foreach (var url in _urls)
        {
            try
            {
                using var stream = await _client.GetStreamAsync(url, token);
                using var reader = XmlReader.Create(stream);
                var feed = SyndicationFeed.Load(reader);
                if (feed != null)
                {
                    list.AddRange(feed.Items.Take(5).Select(i => i.Title.Text));
                }
            }
            catch { }
        }
        return list.ToArray();
    }
}

public class SentimentAnalyzer : ISentimentAnalyzer
{
    private readonly PredictionEngine<SentimentData, SentimentPrediction> _engine;

    public SentimentAnalyzer()
    {
        var ml = new MLContext();
        var data = ml.Data.LoadFromEnumerable(TrainingData());
        var pipeline = ml.Transforms.Text.FeaturizeText("Features", nameof(SentimentData.Text))
            .Append(ml.BinaryClassification.Trainers.SdcaLogisticRegression());
        var model = pipeline.Fit(data);
        _engine = ml.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(model);
    }

    private static IEnumerable<SentimentData> TrainingData() => new[]
    {
        new SentimentData { Text = "profit growth upgrade", Label = true },
        new SentimentData { Text = "record revenue bullish", Label = true },
        new SentimentData { Text = "loss downgrade scandal", Label = false },
        new SentimentData { Text = "bearish drop investigation", Label = false }
    };

    public Sentiment Analyze(string[] news)
    {
        if (news.Length == 0) return Sentiment.Neutral;
        int bull = 0, bear = 0;
        foreach (var n in news)
        {
            var pred = _engine.Predict(new SentimentData { Text = n });
            if (pred.PredictedLabel) bull++; else bear++;
        }
        if (bull > bear) return Sentiment.Bull;
        if (bear > bull) return Sentiment.Bear;
        return Sentiment.Neutral;
    }

    private class SentimentData
    {
        public bool Label { get; set; }
        public string Text { get; set; } = string.Empty;
    }

    private class SentimentPrediction
    {
        public bool PredictedLabel { get; set; }
    }
}

public class MarketDataService : IMarketData
{
    private readonly Quik _quik;
    private readonly Random _rnd = new();

    public MarketDataService(Quik quik)
    {
        _quik = quik;
    }

    public Task<(Quote l1, Level2Quote l2, Trade trade)> GetMarketDataAsync(string instrument, CancellationToken token)
    {
        // TODO: replace with real QUIK data retrieval
        var price = 100m + (decimal)_rnd.NextDouble();
        var q = new Quote(instrument, price - 0.1m, price + 0.1m, DateTime.UtcNow);
        var l2q = new Level2Quote(instrument, new List<(decimal, decimal)>(), new List<(decimal, decimal)>(), DateTime.UtcNow);
        var t = new Trade(instrument, price, 1, DateTime.UtcNow);
        return Task.FromResult((q, l2q, t));
    }

    public Task<(decimal brent, decimal usdRub)> GetExternalDriversAsync(CancellationToken token)
    {
        // placeholder for REST calls
        return Task.FromResult((80m, 90m));
    }
}
