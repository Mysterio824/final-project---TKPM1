using DevTools.UI.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneParserTool
{
    class PhoneParserTool : ITool
    {
        public event PropertyChangedEventHandler PropertyChanged;

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        public PhoneParseResult ParsePhone(string phoneNumber, string countryCode)
        {
            var result = new PhoneParseResult();

            if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(countryCode))
                return ErrorResult("Invalid input", countryCode);

            if (countryCode.StartsWith("+"))
                countryCode = countryCode.Substring(1);

            if (!int.TryParse(countryCode, out int countryCodeInt))
                return ErrorResult("Invalid country code", countryCode);

            string region = GetRegionCodeForCountryCode(countryCode);
            result.CountryShort = region;
            result.CountryFull = GetCountryName(region);
            result.CountryCallingCode = "+" + countryCode;

            // Basic validation (very loose)
            string cleaned = new string(phoneNumber.Where(char.IsDigit).ToArray());

            if (cleaned.Length >= 8 && cleaned.Length <= 15)
            {
                result.IsValid = "Unknown";
                result.IsPossible = "Yes";
                result.Type = "Unknown";

                result.InternationalFormat = "+" + countryCode + " " + cleaned;
                result.NationalFormat = cleaned;
                result.E164Format = "+" + countryCode + cleaned;
                result.RFC3966Format = "tel:+" + countryCode + cleaned;
            }
            else
            {
                result = ErrorResult("Likely invalid", countryCode);
            }

            return result;
        }

        private PhoneParseResult ErrorResult(string reason, string countryCode)
        {
            return new PhoneParseResult
            {
                CountryShort = "Unknown",
                CountryFull = "Unknown",
                CountryCallingCode = "+" + countryCode,
                IsValid = "No",
                IsPossible = "No",
                Type = "Unknown",
                InternationalFormat = reason,
                NationalFormat = reason,
                E164Format = reason,
                RFC3966Format = reason
            };
        }

        private string GetRegionCodeForCountryCode(string code)
        {
            // Very simplified mapping; ideally you’d use an actual map or external source
            var map = new Dictionary<string, string>
        {
            {"1", "US"}, {"44", "GB"}, {"49", "DE"}, {"33", "FR"}, {"91", "IN"},
            {"81", "JP"}, {"86", "CN"}, {"61", "AU"}, {"55", "BR"}, {"7", "RU"}, {"84", "VN"}
        };
            return map.TryGetValue(code, out var region) ? region : "Unknown";
        }

        private string GetCountryName(string isoCode)
        {
            var countries = new Dictionary<string, string>
        {
            {"US", "United States"}, {"GB", "United Kingdom"}, {"DE", "Germany"},
            {"FR", "France"}, {"IN", "India"}, {"JP", "Japan"}, {"CN", "China"},
            {"AU", "Australia"}, {"BR", "Brazil"}, {"RU", "Russia"}, {"VN", "Vietnam"}
        };
            return countries.TryGetValue(isoCode, out var name) ? name : isoCode;
        }

        public object Execute(object input)
        {
            if (input is PhoneParseInput phoneInput)
            {
                return ParsePhone(phoneInput.PhoneNumber, phoneInput.CountryCode);
            }
            return new PhoneParseResult();
        }

        public UserControl GetUI()
        {
            return new PhoneParserToolUI(this);
        }
    }


    // Input class for passing data to Execute method
    public class PhoneParseInput
    {
        public string PhoneNumber { get; set; }
        public string CountryCode { get; set; }
    }

    // Result class that holds all the parsed information
    public class PhoneParseResult
    {
        public string CountryShort { get; set; } = string.Empty;
        public string CountryFull { get; set; } = string.Empty;
        public string CountryCallingCode { get; set; } = string.Empty;
        public string IsValid { get; set; } = string.Empty;
        public string IsPossible { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string InternationalFormat { get; set; } = string.Empty;
        public string NationalFormat { get; set; } = string.Empty;
        public string E164Format { get; set; } = string.Empty;
        public string RFC3966Format { get; set; } = string.Empty;
    }
}
