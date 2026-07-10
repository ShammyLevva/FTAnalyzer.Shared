#if !__PC__
using System.Drawing;
#endif

namespace FTAnalyzer.Theme
{
    /// <summary>
    /// Brand palette shared between FTAnalyzer.Windows and FTAnalyzer.Web.
    /// Canonical source: keep in sync with the :root custom properties in
    /// FTAnalyzer.Web/wwwroot/app.css (--color-*/--rz-* tokens). CSS cannot
    /// consume these constants directly, so both files must be updated together.
    /// </summary>
    public static class Colors
    {
        // ── Light theme (default) ──────────────────────────────────────────
        public static readonly Color PrimaryForestGreen = Color.FromArgb(0x36, 0x4F, 0x29); // --color-primary #364F29
        public static readonly Color PrimaryForestGreenLight = Color.FromArgb(0x4A, 0x6B, 0x39); // --rz-primary-light #4a6b39
        public static readonly Color PrimaryForestGreenPale = Color.FromArgb(0x89, 0x9F, 0x7E); // PrimaryForestGreenLight blended ~35% toward white - a soft sage for UI chrome that sits on the dark green header without feeling heavy
        public static readonly Color SecondaryCharcoalBark = Color.FromArgb(0x2A, 0x27, 0x23); // --color-secondary #2A2723
        public static readonly Color AccentWarmBronze = Color.FromArgb(0x80, 0x77, 0x6D); // --color-accent-warm #80776D (non-text only)
        public static readonly Color AccentActionAmber = Color.FromArgb(0xC7, 0x88, 0x2C); // --color-accent-action #C7882C
        public static readonly Color BgParchment = Color.FromArgb(0xF2, 0xEB, 0xDF); // --color-bg-parchment #F2EBDF
        public static readonly Color GoldDark = Color.FromArgb(0x6B, 0x5A, 0x28); // --color-gold-dark #6B5A28 (light-mode titles/hero)
        // Warm linen/ivory rather than the web app's pure #FFFFFF - WinForms renders card
        // surfaces (grids, text boxes, dialogs) as flat, harshly-bordered rectangles with none of
        // the browser's soft shadow/rounding, so pure white read as a stark box against
        // BgParchment; #FAF7F1 stays in the same warm palette family while keeping enough lift
        // over parchment to still register as a distinct raised surface.
        public static readonly Color BgCard = Color.FromArgb(0xFA, 0xF7, 0xF1); // WinForms-only tweak of --color-bg-card (web app keeps #FFFFFF in app.css)
        public static readonly Color Border = Color.FromArgb(0xDD, 0xD7, 0xCC); // --color-border #DDD7CC
        public static readonly Color GenderMale = Color.FromArgb(0x2E, 0x6D, 0xA4); // --color-gender-male #2E6DA4
        public static readonly Color GenderFemale = Color.FromArgb(0xA9, 0x3D, 0x64); // --color-gender-female #A93D64
        public static readonly Color TierBronze = Color.FromArgb(0x8B, 0x69, 0x14); // --color-tier-bronze #8B6914
        public static readonly Color TierSilver = Color.FromArgb(0x6B, 0x72, 0x80); // --color-tier-silver #6B7280
        public static readonly Color TierPlatinum = Color.FromArgb(0x5A, 0x65, 0x71); // --color-tier-platinum #5A6571

        public static readonly Color Warning = Color.FromArgb(0xBE, 0x7E, 0x24); // --rz-warning #BE7E24
        public static readonly Color Info = Color.FromArgb(0x00, 0x68, 0x7A); // --rz-info #00687A
        public static readonly Color Danger = Color.FromArgb(0xB7, 0x1C, 0x1C); // --rz-danger #b71c1c

        // ── Dark theme ──────────────────────────────────────────────────────
        public static readonly Color DarkBgMain = Color.FromArgb(0x1A, 0x18, 0x16); // --color-bg-main (dark) #1A1816
        public static readonly Color DarkBgCard = Color.FromArgb(0x2A, 0x27, 0x23); // --color-bg-card (dark) #2A2723
        public static readonly Color DarkSecondaryText = Color.FromArgb(0xF2, 0xF3, 0xEF); // --color-secondary (dark) #F2F3EF
        public static readonly Color DarkAccentAction = Color.FromArgb(0xC7, 0x88, 0x2C); // --color-accent-action (dark) #C7882C
        public static readonly Color DarkAccentWarm = Color.FromArgb(0xB5, 0xAD, 0xA4); // --color-accent-warm (dark) #B5ADA4
        public static readonly Color DarkBorder = Color.FromArgb(0x3D, 0x38, 0x32); // --color-border (dark) #3D3832
        public static readonly Color DarkGenderMale = Color.FromArgb(0x5B, 0x9B, 0xD5); // --color-gender-male (dark) #5B9BD5
        public static readonly Color DarkGenderFemale = Color.FromArgb(0xE8, 0x7F, 0xA0); // --color-gender-female (dark) #E87FA0
        public static readonly Color DarkGoldLight = Color.FromArgb(0xE8, 0xD5, 0xA3); // --color-gold-light (dark titles/hero) #E8D5A3
        public static readonly Color DarkPrimary = Color.FromArgb(0x7B, 0xAF, 0x5A); // --rz-primary (dark) #7BAF5A
        public static readonly Color DarkWarning = Color.FromArgb(0xC7, 0x88, 0x2C); // --rz-warning (dark) #C7882C
        public static readonly Color DarkInfo = Color.FromArgb(0x4C, 0xC0, 0xD4); // --rz-info (dark) #4CC0D4
        public static readonly Color DarkDanger = Color.FromArgb(0xEF, 0x53, 0x50); // --rz-danger (dark) #EF5350
        public static readonly Color DarkPrimaryPale = Color.FromArgb(0x5F, 0x7F, 0x47); // DarkPrimary blended ~35% toward DarkBgCard - dark-mode counterpart of PrimaryForestGreenPale, a muted row-selection highlight distinct from the plain dark card background without the header's full brightness
    }
}
