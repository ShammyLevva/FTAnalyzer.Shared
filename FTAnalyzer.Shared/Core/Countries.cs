﻿using System.Collections.Generic;
#if __PC__
using NetTopologySuite.Geometries;
#endif

namespace FTAnalyzer
{
    public static class Countries
    {
        public const string SCOTLAND = "Scotland", ENGLAND = "England", CANADA = "Canada", UNITED_STATES = "United States",
            WALES = "Wales", IRELAND = "Ireland", UNITED_KINGDOM = "United Kingdom", NEW_ZEALAND = "New Zealand", AUSTRALIA = "Australia",
            UNKNOWN_COUNTRY = "Unknown", ENG_WALES = "England and Wales", INDIA = "India", FRANCE = "France", GERMANY = "Germany",
            ITALY = "Italy", SPAIN = "Spain", BELGIUM = "Belgium", SOUTH_AFRICA = "South Africa", NORTHERN_IRELAND = "Northern Ireland",
            EGYPT = "Egypt", HUNGARY = "Hungary", MALTA = "Malta", DENMARK = "Denmark", SWEDEN = "Sweden", NORWAY = "Norway",
            FINLAND = "Finland", ICELAND = "Iceland", SWITZERLAND = "Switzerland", AUSTRIA = "Austria", NETHERLANDS = "Netherlands",
            CHINA = "China", ZIMBABWE = "Zimbabwe", JAPAN = "Japan", RUSSIA = "Russia", POLAND = "Poland", ST_LUCIA = "Saint Lucia",
            LUXEMBOURG = "Luxembourg", ISLE_OF_MAN = "Isle of Man", GREECE = "Greece", LIBYA = "Libya", NIGERIA = "Nigeria",
            BULGARIA = "Bulgaria", CYPRUS = "Cyprus", ESTONIA = "Estonia", LATVIA = "Latvia", LIECHTENSTIEN = "Liechtenstien",
            LITHUANIA = "Lithuania", ALBANIA = "Albania", ANDORRA = "Andorra", ARMENIA = "Armenia", AZERBAIJAN = "Azerbaijan",
            BELARUS = "Belarus", MACEDONIA = "Macedonia", MOLDOVA = "Moldova", MONACO = "Monaco", MONTENEGRO = "Montenegro",
            PORTUGAL = "Portugal", ROMANIA = "Romania", SAN_MARINO = "San Marino", TURKEY = "Turkey", UKRAINE = "Ukraine",
            BRAZIL = "Brazil", MAURITIUS = "Mauritius", UAE = "United Arab Emirates", AFGHANISTAN = "Afghanistan",
            ARGENTINA = "Argentina", BARBADOS = "Barbados", BANGLADESH = "Bangladesh", BAHAMAS = "Bahamas", SRI_LANKA = "Sri Lanka",
            CUBA = "Cuba", INDONESIA = "Indonesia", ISRAEL = "Israel", IRAQ = "Iraq", IRAN = "Iran", JORDAN = "Jordan",
            JAMAICA = "Jamaica", KENYA = "Kenya", MEXICO = "Mexico", SINGAPORE = "Singapore", PAKISTAN = "Pakistan", ANGOLA = "Angola",
            BAHRAIN = "Bahrain", BURUNDI = "Burundi", BENIN = "Benin", BOLIVIA = "Bolivia", BHUTAN = "Bhutan", BOTSWANA = "Botswana",
            BELIZE = "Belize", CONGO = "Congo", CENTRAL_AFRICAN_REPUBLIC = "Central African Republic", CHILE = "Chile",
            CAMEROON = "Cameroon", COLOMBIA = "Colombia", COSTA_RICA = "Costa Rica", CAPE_VERDE = "Cape Verde", DOMINICA = "Dominica",
            DOMINICAN_REPUBLIC = "Dominican Republic", ALGERIA = "Algeria", ECUADOR = "Ecuador", ERITREA = "Eritrea",
            ETHIOPIA = "Ethiopia", FIJI = "Fiji", DJIBOUTI = "Djibouti", MICRONESIA = "Micronesia", GABON = "Gabon", GRENADA = "Grenada",
            GHANA = "Ghana", GAMBIA = "Gambia", GUINEA = "Guinea", EQUATORIAL_GUINEA = "Equatorial Guinea", GUATEMALA = "Guatemala",
            GUYANA = "Guyana", HONDURAS = "Honduras", CROATIA = "Croatia", HAITI = "Haiti", KYRGYZSTAN = "Kyrgyzstan",
            CAMBODIA = "Cambodia", KIRIBATI = "Kiribati", COMOROS = "Comoros", KUWAIT = "Kuwait", KAZAKHSTAN = "Kazakhstan",
            LEBANON = "Lebanon", LIBERIA = "Liberia", LESOTHO = "Lesotho", MOROCCO = "Morocco", MADAGASCAR = "Madagascar",
            MALI = "Mali", MARSHALL_ISLANDS = "Marshall Islands", MYANMAR = "Myanmar", MONGOLIA = "Mongolia",
            MAURITANIA = "Mauitania", MALDIVES = "Maldives", MALAWI = "Malawi", MALAYSIA = "Malaysia", MOZAMBIQUE = "Mozambique",
            NAMIBIA = "Namibia", NIGER = "Niger", NICARAGUA = "Nicaragua", NEPAL = "Nepal", NAURU = "Nauru", OMAN = "Oman",
            PANAMA = "Panama", PERU = "Peru", PAPUA_NEW_GUINEA = "Papua New Guinea", PHILIPPINES = "Philippines", PALAU = "Palau",
            PARAGUAY = "Paraguay", QATAR = "Qatar", SERBIA = "Serbia", RWANDA = "Rwanda", SAUDI_ARABIA = "Saudi Arabia",
            SOLOMON_ISLANDS = "Solomon Islands", SEYSCHELLES = "Seyschelles", SUDAN = "Sudan", SLOVENIA = "Slovenia",
            SLOVAKIA = "Slovakia", CZECH_REPUBLIC = "Czech Republic", CZECHOSLOVAKIA = "Czechoslovakia", SENEGAL = "Senegal",
            SIERRA_LEONE = "Sierra Leone", SOMALIA = "Somalia", SURINAME = "Suriname", SOUTH_SUDAN = "South Sudan",
            EL_SALVADOR = "El Salvador", SYRIA = "Syria", SWAZILAND = "Swaziland", CHAD = "Chad", SOUTH_KOREA = "South Korea",
            NORTH_KOREA = "North Korea", KOREA = "Korea", IVORY_COAST = "Ivory Coast", TOGO = "Togo", THAILAND = "Thailand",
            TAJIKISTAN = "Tajikistan", TURKMENISTAN = "Turkmenistan", TUNISIA = "Tunisia", TONGA = "Tonga", TUVALU = "Tuvalu",
            TRINIDAD_TOBAGO = "Trinidad and Tobago", TANZANIA = "Tanzania", UGANDA = "Uganda", URUGUAY = "Uruguay",
            UZBEKISTAN = "Uzbekistan", VENEZUELA = "Venezuela", VIETNAM = "Vietnam", VANUATU = "Vanuatu", SAMOA = "Samoa",
            YEMEN = "Yemen", ZAMBIA = "Zambia", BURKINA_FASO = "Burkina Faso", BOSNIA = "Bosnia and Herzegovina",
            CHANNEL_ISLANDS = "Channel Islands", GIBRALTAR = "Gibraltar", IVORY_COAST_FR = "Côte d'Ivoire",
            IVORY_COAST_FR2 = "Cote d'Ivoire", HONG_KONG = "Hong Kong", ARUBA = "Aruba", ANGUILLA = "Anguilla",
            AMERICAN_SAMOA = "American Samoa", ANTIGUA_BARBUDA = "Antigua and Barbuda", BERMUDA = "Bermuda",
            BRUNEI = "Brunei", BRUNEI_FULL = "Brunei Darussalam", DR_CONGO = "DR Congo", COOK_ISLANDS = "Cook Islands",
            STKITTS = "Saint Kitts and Nevis", LAO = "Lao", LAO_FULL = "Lao People's Democratic Republic",
            PALESTINE = "Palestine", TIMOR_LESTE = "Timor-Leste", TAIWAN = "Taiwan", GUINEA_BISSAU = "Guinea-Bissau",
            SAO_TOME_PRINCIPE = "Sao Tome and Principe", TOKELAU = "Tokelau", SAINT_VINCENT = "Saint Vincent",
            SAINT_VINCENT_FULL = "Saint Vincent and the Grenadines", ANTARTICA = "Antartica", CAYMAN = "Cayman Islands",
            WESTERN_SAHARA = "Western Sahara", FALKLAND_ISLANDS = "Falkland Islands", FAROES = "Faroe Islands",
            GUADELOUPE = "Guadeloupe", GREENLAND = "Greenland", FRENCH_GUIANA = "French Guiana", GUAM = "Guam",
            MACAO = "Macao", MONSERRAT = "Monserrat", MARTINIQUE = "Martinique", MAYOTTE = "Mayotte",
            NEW_CALEDONIA = "New Caledonia", NIEU = "Nieu", PUERTO_RICO = "Puerto Rico",
            FRENCH_POLYNESIA = "French Polynesia", SAINT_HELENA = "Saint Helena", AT_SEA = "At Sea", OVERSEAS_UK = "Vessels UK and Overseas";

        static readonly ISet<string> KNOWN_COUNTRIES = new HashSet<string>(new string[] {
            SCOTLAND, ENGLAND, CANADA, UNITED_STATES, WALES, IRELAND, UNITED_KINGDOM, NEW_ZEALAND, AUSTRALIA, INDIA, FRANCE, GERMANY,
            ITALY, SPAIN, BELGIUM, SOUTH_AFRICA, NORTHERN_IRELAND, EGYPT, HUNGARY, MALTA, DENMARK, SWEDEN, NORWAY, FINLAND, ICELAND,
            SWITZERLAND, AUSTRIA, NETHERLANDS, CHINA, ZIMBABWE, JAPAN, RUSSIA, POLAND, ST_LUCIA, LUXEMBOURG, ISLE_OF_MAN, GREECE,
            LIBYA, NIGERIA, BULGARIA, CYPRUS, ESTONIA, LATVIA, LIECHTENSTIEN, LITHUANIA, ALBANIA, ARMENIA, ANDORRA, AZERBAIJAN,
            BELARUS, MOLDOVA, MONACO, MONTENEGRO, PORTUGAL, ROMANIA, SAN_MARINO, TURKEY, UKRAINE, BRAZIL, MAURITIUS, UAE, AFGHANISTAN,
            ARGENTINA, BARBADOS, BANGLADESH, BAHAMAS, SRI_LANKA, CUBA, INDONESIA, ISRAEL, IRAN, IRAQ, JORDAN, JAMAICA, KENYA, MEXICO,
            SINGAPORE, PAKISTAN, ANGOLA, BAHRAIN, BURUNDI, BENIN, BOLIVIA, BHUTAN, BOTSWANA, BELIZE, CONGO, CENTRAL_AFRICAN_REPUBLIC,
            CHILE, CAMEROON, COLOMBIA, COSTA_RICA, CAPE_VERDE, DOMINICA, DOMINICAN_REPUBLIC, ALGERIA, ECUADOR, ERITREA, ETHIOPIA,
            FIJI, DJIBOUTI, MICRONESIA, GABON, GAMBIA, GRENADA, GHANA, GUINEA, EQUATORIAL_GUINEA, GUATEMALA, GUYANA, HONDURAS, CROATIA,
            HAITI, KYRGYZSTAN, CAMBODIA, KIRIBATI, COMOROS, KUWAIT, KAZAKHSTAN, LEBANON, LIBERIA, LESOTHO, MOROCCO, MADAGASCAR,
            MALI, MACEDONIA, MARSHALL_ISLANDS, MYANMAR, MONGOLIA, MAURITANIA, MALDIVES, MALAWI, MALAYSIA, MOZAMBIQUE, NAMIBIA, NIGER,
            NICARAGUA, NEPAL, NAURU, OMAN, PANAMA, PERU, PAPUA_NEW_GUINEA, PHILIPPINES, PALAU, PARAGUAY, QATAR, SERBIA, RWANDA,
            SAUDI_ARABIA, SOLOMON_ISLANDS, SEYSCHELLES, SUDAN, SLOVAKIA, SLOVENIA, CZECH_REPUBLIC, CZECHOSLOVAKIA, SENEGAL, SIERRA_LEONE,
            SOMALIA, SURINAME, SOUTH_SUDAN, EL_SALVADOR, SYRIA, SWAZILAND, CHAD, SOUTH_KOREA, NORTH_KOREA, KOREA, IVORY_COAST, TOGO,
            THAILAND, TAJIKISTAN, TURKMENISTAN, TUNISIA, TONGA, TUVALU, TRINIDAD_TOBAGO, TANZANIA, UGANDA, URUGUAY, UZBEKISTAN, VENEZUELA,
            VIETNAM, VANUATU, SAMOA, YEMEN, ZAMBIA, BURKINA_FASO, BOSNIA, CHANNEL_ISLANDS, GIBRALTAR, HONG_KONG, ARUBA, IVORY_COAST_FR,
            IVORY_COAST_FR2, ANGUILLA, AMERICAN_SAMOA, ANTIGUA_BARBUDA, BERMUDA, BRUNEI, BRUNEI_FULL, DR_CONGO, COOK_ISLANDS, STKITTS,
            LAO, LAO_FULL, PALESTINE, TIMOR_LESTE, TAIWAN, GUINEA_BISSAU, SAO_TOME_PRINCIPE, TOKELAU, SAINT_VINCENT, SAINT_VINCENT_FULL,
            ANTARTICA, CAYMAN, WESTERN_SAHARA, FALKLAND_ISLANDS, FAROES, GUADELOUPE, GREENLAND, FRENCH_GUIANA, GUAM, MACAO, MONSERRAT,
            MARTINIQUE, MAYOTTE, NEW_CALEDONIA, NIEU, PUERTO_RICO, FRENCH_POLYNESIA, SAINT_HELENA, AT_SEA, OVERSEAS_UK
        });

        static readonly ISet<string> UK_COUNTRIES = new HashSet<string>(new string[] {
            SCOTLAND, ENGLAND, WALES, ENG_WALES, UNITED_KINGDOM, NORTHERN_IRELAND, ISLE_OF_MAN, CHANNEL_ISLANDS, OVERSEAS_UK
        });

        static readonly ISet<string> CENSUS_COUNTRIES = new HashSet<string>(new string[] {
            SCOTLAND, ENGLAND, WALES, ENG_WALES, UNITED_KINGDOM, UNITED_STATES, CANADA, ISLE_OF_MAN, IRELAND, CHANNEL_ISLANDS, OVERSEAS_UK
        });

#if __PC__
        static readonly Dictionary<string, Envelope> BOUNDING_BOXES;
        static readonly Envelope WHOLE_WORLD = new(-180, 180, -90, 90);
#endif

        static Countries()
        {   // generate position at http://imeasuremap.com/?e=57.4552937099324,-4.98779296874996:0::rectangle:0
#if __PC__
            BOUNDING_BOXES = new Dictionary<string, Envelope>
            {
                { SCOTLAND, new Envelope(-7.974074, -0.463426, 54.571547, 60.970872) },
                { ENGLAND, new Envelope(-6.523879, 1.879409, 49.814376, 55.865022) },
                { WALES, new Envelope(-5.561202, -2.596147, 51.296580, 53.450153) },
                { UNITED_KINGDOM, new Envelope(-7.974074, 1.879409, 49.814376, 60.970872) },
                { IRELAND, new Envelope(-10.746749, -5.298783, 51.296580, 55.467681) },
                { NORTHERN_IRELAND, new Envelope(-8.329757, -5.298783, 53.872250, 55.467681) },
                { CANADA, new Envelope(-141, -52, 41.129387, 83.232810) },
                { UNITED_STATES, new Envelope(-169.136641, -66.086137, 17.665423, 71.626319) },
                { AUSTRALIA, new Envelope(112.728595, 154.343553, -44.134565, -9.219173) },
                { NEW_ZEALAND, new Envelope(166.199058, 178.689262, -47.405457, -34.187216) },
                { FRANCE, new Envelope(-5.231600, 8.357236, 42.237011, 51.173873) },
                { BELGIUM, new Envelope(2.436859, 6.533508, 49.389841, 51.530658) },
                { NETHERLANDS, new Envelope(3.205904, 7.324528, 50.886015, 53.756544) },
                { GERMANY, new Envelope(5.732761, 15.212713, 47.126544, 55.048204) },
                { SPAIN, new Envelope(-9.428367, 4.709787, 35.867189, 43.875768) },
                { PORTUGAL, new Envelope(-17.360492, -6.100757, 32.487006, 42.254591) },
                { ITALY, new Envelope(6.523787, 18.662428, 36.523271, 47.168847) },
                { MEXICO, new Envelope(-117.314102, -86.630537, 14.216935, 32.927605) }
            };
#endif
        }

        public static bool IsUnitedKingdom(string country) => UK_COUNTRIES.Contains(country);

        public static bool IsKnownCountry(string country) => KNOWN_COUNTRIES.Contains(country);

        public static bool IsCensusCountry(string country) => CENSUS_COUNTRIES.Contains(country);

        public static bool IsEnglandWales(string country) =>
            country == ENG_WALES || country == ENGLAND || country == WALES ||
            country == ISLE_OF_MAN || country == CHANNEL_ISLANDS || country == OVERSEAS_UK;

#if __PC__
        public static Envelope BoundingBox(string country)
        {
            if (BOUNDING_BOXES.ContainsKey(country))
                return BOUNDING_BOXES[country];
            else
                return WHOLE_WORLD;
        }
#endif
    }
}
