using System.Text;

namespace Vertigo.Wheel.Data
{
    /// <summary>
    /// Single source of truth for "{Name}" / "{Name} x{Amount}" reward label formatting.
    /// Centralizes the rule so <see cref="RewardDefinition"/>, the outcome popup text resolver,
    /// and <c>RewardInventory.BuildSummary</c> stay in lockstep.
    /// </summary>
    public static class WheelRewardLabelFormat
    {
        /// <summary>
        /// Returns "{name}" when <paramref name="alwaysShowAmount"/> is false and amount &lt;= 1,
        /// otherwise "{name} x{amount}".
        /// </summary>
        public static string Format(string displayName, int amount, bool alwaysShowAmount)
        {
            string safeName = displayName ?? string.Empty;
            return ShouldShowAmount(amount, alwaysShowAmount)
                ? safeName + " x" + amount
                : safeName;
        }

        /// <summary>Appends the formatted label to <paramref name="builder"/>.</summary>
        public static void AppendTo(StringBuilder builder, string displayName, int amount, bool alwaysShowAmount)
        {
            if (builder == null) return;

            builder.Append(displayName ?? string.Empty);
            if (ShouldShowAmount(amount, alwaysShowAmount))
            {
                builder.Append(" x");
                builder.Append(amount);
            }
        }

        private static bool ShouldShowAmount(int amount, bool alwaysShowAmount)
        {
            return alwaysShowAmount || amount > 1;
        }
    }
}
