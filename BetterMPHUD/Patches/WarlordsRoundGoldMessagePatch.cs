using System;
using System.Reflection;
using System.Text.RegularExpressions;
using HarmonyLib;

namespace BetterMPHUD.Patches
{
    public static class WarlordsRoundGoldMessagePatch
    {
        private const string WarlordsNetworkComponentTypeName = "MBWarlords.LastConcord.WarlordsNetworkComponent";
        private const string RoundGoldFeedKindName = "RoundGoldFeed";

        private static readonly Regex CurrentPayoutRegex =
            new Regex(@"Current payout:\s*([+-]?\d+)\s+gold after the round\.", RegexOptions.Compiled);

        private static readonly Regex AppliedPayoutRegex =
            new Regex(@"Payout:\s*([+-]?\d+)\s+gold\.", RegexOptions.Compiled);

        private static readonly Regex SignedAmountRegex =
            new Regex(@"[+-]\d+", RegexOptions.Compiled);

        private static bool _patched;

        public static bool TryPatch(Harmony harmony)
        {
            if (_patched)
                return true;

            if (harmony == null)
                return false;

            Type networkComponentType = AccessTools.TypeByName(WarlordsNetworkComponentTypeName);
            if (networkComponentType == null)
                return false;

            MethodInfo handlerMethod = AccessTools.Method(networkComponentType, "HandleDisplayServerMsgMessage");
            MethodInfo prefixMethod = AccessTools.Method(typeof(WarlordsRoundGoldMessagePatch), nameof(Prefix));
            if (handlerMethod == null || prefixMethod == null)
                return false;

            harmony.Patch(handlerMethod, prefix: new HarmonyMethod(prefixMethod));
            _patched = true;
            return true;
        }

        private static void Prefix(object __0)
        {
            if (!ProfileManager.GetActiveSettings().WarlordsOnlyShowRoundGoldTotal)
                return;

            if (!IsRoundGoldFeed(__0))
                return;

            PropertyInfo messageProperty = AccessTools.Property(__0.GetType(), "Message");
            FieldInfo messageField = AccessTools.Field(__0.GetType(), "Message");
            string message = messageProperty != null
                ? messageProperty.GetValue(__0, null) as string
                : messageField != null ? messageField.GetValue(__0) as string : null;

            string rewritten = RewriteRoundGoldMessage(message);
            if (string.IsNullOrWhiteSpace(rewritten) || rewritten == message)
                return;

            if (messageProperty != null && messageProperty.CanWrite)
                messageProperty.SetValue(__0, rewritten, null);
            else if (messageField != null)
                messageField.SetValue(__0, rewritten);
        }

        private static bool IsRoundGoldFeed(object message)
        {
            if (message == null)
                return false;

            PropertyInfo kindProperty = AccessTools.Property(message.GetType(), "Kind");
            FieldInfo kindField = AccessTools.Field(message.GetType(), "Kind");
            object kind = kindProperty != null
                ? kindProperty.GetValue(message, null)
                : kindField != null ? kindField.GetValue(message) : null;

            return kind != null && string.Equals(kind.ToString(), RoundGoldFeedKindName, StringComparison.Ordinal);
        }

        private static string RewriteRoundGoldMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return message;

            Match currentPayoutMatch = CurrentPayoutRegex.Match(message);
            if (currentPayoutMatch.Success)
                return "Battle gold: " + currentPayoutMatch.Groups[1].Value + " gold after the round.";

            Match appliedPayoutMatch = AppliedPayoutRegex.Match(message);
            if (appliedPayoutMatch.Success)
                return "Spoils delivered: " + appliedPayoutMatch.Groups[1].Value + " gold.";

            if (message.StartsWith("Spoils delivered:", StringComparison.Ordinal))
            {
                MatchCollection matches = SignedAmountRegex.Matches(message);
                if (matches.Count > 0)
                {
                    int total = 0;
                    foreach (Match match in matches)
                    {
                        int value;
                        if (int.TryParse(match.Value, out value))
                            total += value;
                    }

                    return "Spoils delivered: " + FormatSignedValue(total) + " gold.";
                }
            }

            return message;
        }

        private static string FormatSignedValue(int value)
        {
            return value > 0 ? "+" + value : value.ToString();
        }
    }
}
