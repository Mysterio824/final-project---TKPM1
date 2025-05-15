using DevTools.UI.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyFormatterTool
{
    class CurrencyFormatterTool : ITool
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly Dictionary<string, CultureInfo> _currencyCultures = new Dictionary<string, CultureInfo>
    {
        { "USD", new CultureInfo("en-US") },
        { "EUR", new CultureInfo("fr-FR") },
        { "GBP", new CultureInfo("en-GB") },
        { "JPY", new CultureInfo("ja-JP") },
        { "CAD", new CultureInfo("en-CA") },
        { "AUD", new CultureInfo("en-AU") },
        { "CHF", new CultureInfo("de-CH") },
        { "CNY", new CultureInfo("zh-CN") },
        { "INR", new CultureInfo("hi-IN") },
        { "BRL", new CultureInfo("pt-BR") }
    };

        public bool ValidateCurrencyCode(string currencyCode)
        {
            if (string.IsNullOrWhiteSpace(currencyCode) || currencyCode.Length != 3)
                return false;

            try
            {
                foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
                {
                    try
                    {
                        var region = new RegionInfo(culture.Name);
                        if (region.ISOCurrencySymbol.Equals(currencyCode, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            catch
            {
                return _currencyCultures.ContainsKey(currencyCode.ToUpper());
            }

            return false;
        }

        // Formats a number according to the specified currency and culture
        public string FormatCurrency(decimal amount, string currencyCode)
        {
            if (!ValidateCurrencyCode(currencyCode))
                return "Invalid currency code";

            CultureInfo culture;
            if (!_currencyCultures.TryGetValue(currencyCode, out culture))
            {
                // Use a generic culture if specific one is not in our dictionary
                culture = CultureInfo.CreateSpecificCulture("en-US");
            }

            return amount.ToString("C", new CultureInfo(culture.Name) { NumberFormat = { CurrencySymbol = new RegionInfo(culture.Name).CurrencySymbol } });
        }

        public object Execute(object input)
        {
            return input;
        }

        public UserControl GetUI()
        {
            return new CurrencyFormatterToolUI(this);
        }
    }
}
