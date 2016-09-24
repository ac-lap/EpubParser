using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EpubReader
{
    public class Utils
    {
        public static readonly RegexOptions REO_ = RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant;
        public static readonly RegexOptions REO_c = RegexOptions.Compiled | Utils.REO_;
        public static readonly RegexOptions REO_s = RegexOptions.Singleline | Utils.REO_;
        public static readonly RegexOptions REO_cs = RegexOptions.Compiled | Utils.REO_s;
        public static readonly RegexOptions REO_m = RegexOptions.Multiline | Utils.REO_;
        public static readonly RegexOptions REO_cm = RegexOptions.Compiled | Utils.REO_m;
        public static readonly RegexOptions REO_i = RegexOptions.IgnoreCase | Utils.REO_;
        public static readonly RegexOptions REO_ci = RegexOptions.IgnoreCase | Utils.REO_c;
        public static readonly RegexOptions REO_si = RegexOptions.Singleline | Utils.REO_i;
        public static readonly RegexOptions REO_csi = RegexOptions.Compiled | Utils.REO_si;
        public static readonly RegexOptions REO_mi = RegexOptions.Multiline | Utils.REO_i;
        public static readonly RegexOptions REO_cmi = RegexOptions.Compiled | Utils.REO_mi;
        public static readonly RegexOptions REO_wsi = Utils.REO_si ^ RegexOptions.IgnorePatternWhitespace;
        public static readonly RegexOptions REO_wcm = Utils.REO_cm ^ RegexOptions.IgnorePatternWhitespace;
        private static Regex regex_ReplaceBlockTags = new Regex("(?<!^\\s*)<(p|div|h1|h2|h3|h4|h5|h6)[^>]*>", Utils.REO_cmi);
        private static Regex regex_ReplaceParaTags = new Regex("<(p)[^>]*>", Utils.REO_cmi);
        private static Regex regex_OpenHeadingTags = new Regex("<(h1|h2|h3|h4|h5|h6)[^>]*>", Utils.REO_cmi);
        private static Regex regex_OpenBoldTags = new Regex("<(strong)[^>]*>", Utils.REO_cmi);
        private static Regex regex_CloseHeadingTags = new Regex("</(h1|h2|h3|h4|h5|h6)[^>]*>", Utils.REO_cmi);
        private static Regex regex_CloseBoldTags = new Regex("</(strong)[^>]*>", Utils.REO_cmi);
        private static Regex regex_CleanHtmlTags = new Regex("</?(\\w+|\\s*!--)[^>]*>", Utils.REO_c);
        private static Regex regex_SpecialSymbols = new Regex("(?<defined>(&nbsp|&quot|&mdash|&ldquo|&rdquo|\\&\\#8211|\\&\\#8212|&\\#8230|\\&\\#171|&laquo|&raquo|&amp);?)|(?<other>\\&\\#\\d+;?)", Utils.REO_ci);
        private static Regex regex_SpecialSymbolsReplace = new Regex("\\ {2,}", Utils.REO_c);

        public static string ClearText(string text)
        {
            if (text == null)
                return (string)null;
            return Utils.ClearSpecialSymbols(Utils.CleanHtmlTags(Utils.ReplaceBlockTagsToNewLineCharacter(text)));
        }

        public static Tuple<string, List<byte>> ForamttedText(string input)
        {
            if (input == null)
                return null;

            string output = input.Replace("\n", String.Empty);
            output = output.Replace("\r", String.Empty);
            output = Utils.ReplaceHeadingTagsToBoldCharacter(output);
            output = Utils.ReplaceParaTagsToNewLineCharacter(output);
            output = Utils.ClearSpecialSymbols(Utils.CleanHtmlTags(output));

            return FormatContent(output);
        }

        private static Tuple<string, List<byte>> FormatContent(string iText)
        {
            List<byte> bList = Enumerable.Repeat((byte)0, iText.Length + 1).ToList();

            StringBuilder oText = new StringBuilder(iText);
            bool boldStart = false;

            for (int i=0;i<oText.Length;i++)
            {
                if (oText[i] == '╠')
                {
                    boldStart = true;
                    oText.Remove(i, 1);
                    i--;
                }
                else if (oText[i] == '╣')
                {
                    boldStart = false;
                    oText.Remove(i, 1);
                    i--;
                }

                if (boldStart == true)
                {
                    bList[i] = 1;
                }
            }

            return Tuple.Create<string, List<byte>>(oText.ToString(), bList);
        }

        private static string ReplaceHeadingTagsToBoldCharacter(string text)
        {
            if (text == null)
                return (string)null;
            string output = Utils.regex_CloseHeadingTags.Replace(Utils.regex_OpenHeadingTags.Replace(text, "\n\n╠"), "╣\n");
            return Utils.regex_CloseBoldTags.Replace(Utils.regex_OpenBoldTags.Replace(output, "╠"), "╣");
        }

        private static string ReplaceParaTagsToNewLineCharacter(string text)
        {
            if (text == null)
                return (string)null;
            return Utils.regex_ReplaceParaTags.Replace(text, "\n");
        }

        public static string CleanHtmlTags(string text)
        {
            if (text == null)
                return (string)null;
            return Utils.regex_CleanHtmlTags.Replace(text, " ");
        }

        private static string ReplaceBlockTagsToNewLineCharacter(string text)
        {
            if (text == null)
                return (string)null;
            return Utils.regex_ReplaceBlockTags.Replace(text, "\n");
        }

        private static string ClearSpecialSymbols(string text)
        {
            if (text == null)
                return (string)null;
            return Utils.regex_SpecialSymbolsReplace.Replace(Utils.regex_SpecialSymbols.Replace(text, new MatchEvaluator(Utils.SpecialSymbolsEvaluator)), " ");
        }

        private static string SpecialSymbolsEvaluator(Match m)
        {
            if (!m.Groups["defined"].Success)
                return " ";
            switch (m.Groups["defined"].Value.ToLower())
            {
                case "&nbsp;":
                    return " ";
                case "&nbsp":
                    return " ";
                case "&quot;":
                    return "\"";
                case "&quot":
                    return "\"";
                case "&mdash;":
                    return " ";
                case "&mdash":
                    return " ";
                case "&ldquo;":
                    return "\"";
                case "&ldquo":
                    return "\"";
                case "&rdquo;":
                    return "\"";
                case "&rdquo":
                    return "\"";
                case "&#8211;":
                    return "-";
                case "&#8211":
                    return "-";
                case "&#8212;":
                    return "-";
                case "&#8212":
                    return "-";
                case "&#8230":
                    return "...";
                case "&#171;":
                    return "\"";
                case "&#171":
                    return "\"";
                case "&laquo;":
                    return "\"";
                case "&laquo":
                    return "\"";
                case "&raquo;":
                    return "\"";
                case "&raquo":
                    return "\"";
                case "&amp;":
                    return "&";
                case "&amp":
                    return "&";
                default:
                    return " ";
            }
        }
    }
}
