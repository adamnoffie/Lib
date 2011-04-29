// ----------------------------------------------------------------------------
// Validation.cs
// Copyright (c) 2009 Adam Nofsinger <adam.nofsinger@gmail.com>
//
// Permission to use, copy, modify, and distribute this software for any
// purpose with or without fee is hereby granted, provided that the above
// copyright notice and this permission notice appear in all copies.
//
// THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
// WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
// MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
// ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
// WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
// ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
// OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
// ----------------------------------------------------------------------------
using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Globalization;

namespace Utility
{
    public static class Validation
    {

        const double minTotal = 99.9;
        const double maxTotal = 100.1;

        /// <summary>
        /// Verifies that all of the strings are valid decimals, and they are in the range
        /// </summary>
        /// <param name="percentages"></param>
        /// <returns></returns>
        public static bool ValidatePercentages(bool allowAllBlank, bool allowBlank, params string[] percentages)
        {
            bool valid = true;

            if (percentages.Any(s => !(string.IsNullOrEmpty(s) || s == "0")))
            {
                double total = 0.0;
                for (int i = 0; i < percentages.Length; i++)
                {
                    double percentage;
                    if (!double.TryParse(percentages[i], out percentage))  // verify valid decimal
                    {
                        if (!allowBlank || !string.IsNullOrEmpty(percentages[i]))
                        {
                            valid = false;
                            break;
                        }
                    }
                    total += percentage;
                    if (percentage < 0 || total > maxTotal)  // verify total does not exceed max, and is positive
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid && total < minTotal)      // verify total is more than minvalue
                    valid = false;
            }
            else // all blank
            {
                valid = allowAllBlank;
            }

            return valid;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allowAllBlank"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool ValidateIntegers(bool allowBlank, int minValue, int maxValue, params string[] values)
        {
            bool valid = true;
            int val = 0;
            
            for (int i = 0; i < values.Length; i++)
            {
                if (!int.TryParse(values[i], out val))      // verify int is valid
                {
                    if (!allowBlank || !string.IsNullOrEmpty(values[i]))
                    {
                        valid = false;
                        break;
                    }
                }
                else if (val < minValue || val > maxValue)       // verify int is inside range
                {
                    valid = false;
                    break;
                }
            }

            return valid;
        }

        /// <summary>
        /// Verifies that some strings are valid dollar amounts like
        /// 33.23 $1,200 $242,242,423.324
        /// </summary>
        /// <param name="allowBlank"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool ValidateDollarAmounts(bool allowBlank, double minValue, double maxValue, 
            bool allowPennies, params string[] values)
        {
            bool valid = true;
            double val = 0.0;
            NumberStyles numStyles = NumberStyles.Currency;
            if (!allowPennies)
                numStyles = numStyles & (~NumberStyles.AllowDecimalPoint);

            for (int i = 0; i < values.Length; i++)
            {
                // verify money is valid 
                if (!double.TryParse(values[i], numStyles, NumberFormatInfo.CurrentInfo, out val))
                {
                    if (!string.IsNullOrEmpty(values[i]) || !allowBlank)
                    {
                        valid = false;
                        break;
                    }
                }
                else if (val < minValue || val > maxValue)       // verify moeny is in allowed range
                {
                    valid = false;
                    break;
                }
            }

            return valid;
        }
    }
}
