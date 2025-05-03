using DevTools.UI.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBANValidatorTool
{
    class IBANValidatorTool : ITool
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Method to validate IBAN
        public IBANValidationResult ValidateIBAN(string iban)
        {
            var result = new IBANValidationResult();

            // Remove spaces and convert to uppercase
            string cleanedIBAN = iban.Replace(" ", "").ToUpper();
            result.CleanedIBAN = cleanedIBAN;

            // Check if IBAN is valid
            result.IsValid = IsValidIBAN(cleanedIBAN);

            if (result.IsValid)
            {
                // Extract country code
                result.CountryCode = cleanedIBAN.Substring(0, 2);

                // Extract BBAN (Basic Bank Account Number)
                result.BBAN = cleanedIBAN.Substring(4);

                // Format IBAN for friendly display
                result.FormattedIBAN = FormatIBAN(cleanedIBAN);

                // Check if it's a QR-IBAN (Swiss QR-IBANs have institution ID starting with 30)
                result.IsQRIBAN = result.CountryCode == "CH" && cleanedIBAN.Substring(4, 2) == "30";
            }

            return result;
        }

        private bool IsValidIBAN(string iban)
        {
            if (string.IsNullOrWhiteSpace(iban) || iban.Length < 5)
                return false;

            // Check country code (first 2 chars)
            string countryCode = iban.Substring(0, 2);
            if (!char.IsLetter(countryCode[0]) || !char.IsLetter(countryCode[1]))
                return false;

            // Check that the next 2 chars are digits (checksum digits)
            if (!char.IsDigit(iban[2]) || !char.IsDigit(iban[3]))
                return false;

            // Move the first 4 characters to the end
            string rearrangedIBAN = iban.Substring(4) + iban.Substring(0, 4);

            // Convert letters to numbers (A=10, B=11, ..., Z=35)
            StringBuilder numericIBAN = new StringBuilder();
            foreach (char c in rearrangedIBAN)
            {
                if (char.IsLetter(c))
                    numericIBAN.Append((c - 'A' + 10).ToString());
                else
                    numericIBAN.Append(c);
            }

            // Perform mod 97 on the numeric IBAN
            // Process chunks of 9 digits at a time to avoid overflow
            string numericString = numericIBAN.ToString();

            // Process much smaller chunks to avoid integer overflow
            int chunkSize = 6; // Process 6 digits at a time instead of 9
            int remainder = 0;

            for (int i = 0; i < numericString.Length; i += chunkSize)
            {
                int currentChunkSize = Math.Min(chunkSize, numericString.Length - i);
                string currentChunk = remainder + numericString.Substring(i, currentChunkSize);

                // Try to parse safely, handling potential overflow
                if (long.TryParse(currentChunk, out long longValue))
                {
                    remainder = (int)(longValue % 97);
                }
                else
                {
                    // If it's still too large for long, process digit by digit
                    remainder = int.Parse(remainder.ToString());
                    for (int j = 0; j < currentChunkSize; j++)
                    {
                        int digit = int.Parse(numericString[i + j].ToString());
                        remainder = (remainder * 10 + digit) % 97;
                    }
                }
            }

            // If the remainder is 1, the IBAN is valid
            return remainder == 1;
        }


        private string FormatIBAN(string iban)
        {
            StringBuilder formattedIBAN = new StringBuilder();

            for (int i = 0; i < iban.Length; i++)
            {
                if (i > 0 && i % 4 == 0)
                    formattedIBAN.Append(' ');

                formattedIBAN.Append(iban[i]);
            }

            return formattedIBAN.ToString();
        }

        public object Execute(object input)
        {
            if (input is string ibanString)
            {
                return ValidateIBAN(ibanString);
            }
            return new IBANValidationResult { IsValid = false };
        }

        public UserControl GetUI()
        {
            return new IBANValidatorToolUI(this);
        }
    }

    public class IBANValidationResult
    {
        public bool IsValid { get; set; }
        public bool IsQRIBAN { get; set; }
        public string CountryCode { get; set; }
        public string BBAN { get; set; }
        public string FormattedIBAN { get; set; }
        public string CleanedIBAN { get; set; }
    }
}
