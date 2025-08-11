// SP25.WebApi/Services/PriceCatalog.cs
using Microsoft.Extensions.Hosting;
using SP25.Business.Services.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

public class PriceCatalog : IPriceCatalog
{
    private readonly Dictionary<string, decimal> _prices;

    private record MenuCategory(string category, List<MenuItem> items);
    private record MenuItem(string name, decimal price);

    public PriceCatalog(IHostEnvironment env)
    {
        // path către Data/menu.json
        var path = Path.Combine(env.ContentRootPath, "Data", "menu.json");
        if (!File.Exists(path))
        {
            _prices = new(StringComparer.OrdinalIgnoreCase);
            return;
        }

        var json = File.ReadAllText(path);
        var cats = JsonSerializer.Deserialize<List<MenuCategory>>(json) ?? new();
        _prices = new(StringComparer.OrdinalIgnoreCase);

        foreach (var c in cats)
        {
            foreach (var it in c.items ?? new())
            {
                var key = Normalize(it.name);
                if (!_prices.ContainsKey(key))
                    _prices[key] = it.price;
            }
        }
    }

    public decimal? GetPriceOrNull(string? productName)
    {
        if (string.IsNullOrWhiteSpace(productName)) return null;

        var key = Normalize(productName);
        if (_prices.TryGetValue(key, out var p)) return p;

        var noDia = RemoveDiacritics(key);
        if (_prices.TryGetValue(noDia, out p)) return p;

        // fallback fuzzy light: caută containere (atenție la coliziuni)
        var hit = _prices.FirstOrDefault(kv => kv.Key.Contains(noDia, StringComparison.OrdinalIgnoreCase));
        return hit.Equals(default(KeyValuePair<string, decimal>)) ? null : hit.Value;
    }

    private static string Normalize(string s)
    {
        s = (s ?? "").Trim();
        s = RemoveDiacritics(s);
        s = System.Text.RegularExpressions.Regex.Replace(s, @"\s+", " ");
        return s;
    }

    private static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(System.Text.NormalizationForm.FormD);
        var sb = new System.Text.StringBuilder(capacity: normalized.Length);

        foreach (var ch in normalized)
        {
            var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }
        return sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
    }
}
