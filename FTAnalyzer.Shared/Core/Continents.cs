namespace FTAnalyzer
{
    // GEDCOM locations occasionally have a continent tacked on as a spurious extra "country"
    // segment (e.g. "Rushton Spencer, Staffordshire, England, Europe"). These aren't real
    // countries and FixCountryTypos/ShiftCountryToRegion don't know what to do with them, so
    // they need stripping before any country-aware fixing runs.
    public static class Continents
    {
        public const string AFRICA = "Africa", NORTH_AMERICA = "North America", CENTRAL_AMERICA = "Central America", SOUTH_AMERICA = "South America",
            EUROPE = "Europe", ASIA = "Asia", MIDDLE_EAST = "Middle East", OCEANIA = "Oceania", ORIENT = "Orient";

        static readonly HashSet<string> KNOWN_CONTINENTS = new([
            AFRICA, NORTH_AMERICA, CENTRAL_AMERICA, SOUTH_AMERICA, EUROPE, ASIA, MIDDLE_EAST, OCEANIA, ORIENT
        ], StringComparer.OrdinalIgnoreCase);

        public static bool IsContinent(string country) => KNOWN_CONTINENTS.Contains(country);
    }
}
