
using Microsoft.VisualBasic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ConvertSSMLDotEscape
{
    public class ConvertSSMLDotEscape
    {

        public static void Main(string[] args)
        {
           var output = ConvertToSsmlText(".2Hello     .p90.v70",  "1s");
            Console.WriteLine(output);
        }

        public static string ConvertToSsmlText(string translatedText, string breakTime)
        {
            string pattern = @"\.\d+(\.\d+)?";
            string ssml = "<speak>";
            int lastIndex = 0;
            bool isProsodyOpen = false;
            ////.0.8Good morning => <prosody rate="0.8">Good morning</prosody>
            foreach (Match match in Regex.Matches(translatedText, pattern))
            {
                string textBefore = translatedText.Substring(lastIndex, match.Index - lastIndex);
                ssml += textBefore;
                if (isProsodyOpen)
                {
                    ssml += "</prosody>";
                }
                string rateValue = match.Value.Substring(1);
                ssml += $"<prosody rate=\"{rateValue}\">";
                isProsodyOpen = true;

                lastIndex = match.Index + match.Length;
            }

            ssml += translatedText.Substring(lastIndex);
            if (isProsodyOpen)
            {
                ssml += "</prosody>";
            }

            ssml = PitchAndVolumeSynthesizer(ssml, @"\.p(-?\d+(\.\d+)?)", true);
            ssml = PitchAndVolumeSynthesizer(ssml, @"\.v(-?\d+(\.\d+)?)", false);

            return $"{ssml.Replace("  ", $"<break time=\"{breakTime}\"/>")
                .Replace(". ", ".<break time=\"300ms\"/>")
                .Replace(",", ",<break time=\"100ms\"/>")
                .Replace("!", "!<break time=\"300ms\"/>")
                .Replace("?", "?<break time=\"300ms\"/>")}</speak>";
        }

        public static string PitchAndVolumeSynthesizer(string translatedText, string pattern, bool isPitch)
        {
            string ssml = string.Empty;
            int lastIndex = 0;
            bool isProsodyOpen = false;
            ////.0.8Good morning => <prosody rate="0.8">Good morning</prosody>
            foreach (Match match in Regex.Matches(translatedText, pattern))
            {
                string textBefore = translatedText.Substring(lastIndex, match.Index - lastIndex);
                ssml += textBefore;
                if (isProsodyOpen)
                {
                    ssml += "</prosody>";
                }
                string pitchValue = match.Value.Substring(2);
                if (isPitch)
                {
                    ssml += $"<prosody pitch=\"{pitchValue}%\">";
                }
                else
                {
                    ssml += $"<prosody volume=\"{pitchValue}dB\">";
                }

                isProsodyOpen = true;
                lastIndex = match.Index + match.Length;
            }

            ssml += translatedText.Substring(lastIndex);
            if (isProsodyOpen)
            {
                ssml += "</prosody>";
            }

            return ssml;
        }
    }
}