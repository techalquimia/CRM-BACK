using System.Globalization;
using System.Text.RegularExpressions;
using Google.Cloud.Vision.V1;

namespace RouteEvidence.Services;

public class TicketOcrService : ITicketOcrService
{
    private readonly ImageAnnotatorClient _client;
    private readonly ILogger<TicketOcrService> _logger;

    private static readonly Regex[] TotalWeightPatterns =
    [
        new Regex(@"peso\s*total\s*[:=]?\s*([\d.,]+)", RegexOptions.IgnoreCase),
        new Regex(@"total\s*[:=]?\s*([\d.,]+)\s*(?:kg|ton)?", RegexOptions.IgnoreCase),
        new Regex(@"bruto\s*[:=]?\s*([\d.,]+)", RegexOptions.IgnoreCase),
    ];

    private static readonly Regex[] TaraPatterns =
    [
        new Regex(@"tara\s*[:=]?\s*([\d.,]+)", RegexOptions.IgnoreCase),
        new Regex(@"tare\s*[:=]?\s*([\d.,]+)", RegexOptions.IgnoreCase),
    ];

    private static readonly Regex[] NetWeightPatterns =
    [
        new Regex(@"peso\s+neto\s*[:=]?\s*([\d.,]+)", RegexOptions.IgnoreCase),
        new Regex(@"neto\s*[:=]?\s*([\d.,]+)", RegexOptions.IgnoreCase),
        new Regex(@"carga\s*[:=]?\s*([\d.,]+)", RegexOptions.IgnoreCase),
    ];

    public TicketOcrService(ILogger<TicketOcrService> logger)
    {
        _client = ImageAnnotatorClient.Create();
        _logger = logger;
    }

    public async Task<TicketOcrResult> ExtractTicketDataAsync(
        string gcsBucket,
        string gcsObjectKey,
        CancellationToken ct = default)
    {
        var image = Image.FromUri($"gs://{gcsBucket}/{gcsObjectKey}");
        var response = await _client.DetectDocumentTextAsync(image);

        var fullText = response?.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(fullText))
        {
            _logger.LogWarning("No text extracted from ticket image gs://{Bucket}/{Key}", gcsBucket, gcsObjectKey);
            return new TicketOcrResult(fullText, null, null, null);
        }

        _logger.LogInformation("Extracted {Length} chars from ticket", fullText.Length);

        var totalWeight = TryParseWeight(TotalWeightPatterns, fullText);
        var tara = TryParseWeight(TaraPatterns, fullText);
        var netWeight = TryParseWeight(NetWeightPatterns, fullText);

        return new TicketOcrResult(fullText, totalWeight, tara, netWeight);
    }

    private static double? TryParseWeight(Regex[] patterns, string text)
    {
        foreach (var regex in patterns)
        {
            var match = regex.Match(text);
            if (match.Success && TryParseNumber(match.Groups[1].Value, out var value))
                return value;
        }

        return null;
    }

    private static bool TryParseNumber(string s, out double value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(s)) return false;

        s = s.Trim().Replace(",", CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
        return double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }
}
