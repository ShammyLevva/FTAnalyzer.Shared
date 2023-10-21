using System.Text;

namespace FTAnalyzer
{
    class AnselEncoding : Encoding
    {
        public override int GetByteCount(char[] chars, int index, int count)
        {
            int byteCount = 0;
            for (int i = index; i < index + count; i++)
            {
                char ch = chars[i];
                if (ch < 0x80)
                {
                    byteCount++;
                }
                else
                {
                    int ansel = ConvertToAnsel(ch);
                    byteCount++;
                    if (ansel > 0xFF)
                        byteCount++;
                }
            }
            return byteCount;
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            int j = byteIndex;
            for (int i = charIndex; i < charIndex + charCount; i++)
            {
                char ch = chars[i];
                if (ch < 0x80)
                {
                    bytes[j++] = (byte) ch;
                }
                else
                {
                    int ansel = ConvertToAnsel(ch);
                    if (ansel <= 0xFF)
                    {
                        bytes[j++] = (byte) ansel;
                    }
                    else
                    {
                        bytes[j++] = (byte) (ansel / 256);
                        bytes[j++] = (byte) (ansel % 256);
                    }
                }
            }
            return j - byteIndex;
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            int charCount = 0;
            for (int i = index; i < index + count; i++)
            {
                byte b = bytes[i];
                if (b >= 0xE0 && b <= 0xFF)
                {
                    if ((i + 1 < index + count) && (bytes[i + 1] > 0))
                        i++;
                }
                charCount++;
            }
            return charCount;
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            int j = charIndex;
            for (int i = byteIndex; i < byteIndex + byteCount; i++)
            {
                byte b = bytes[i];
                char ch = (char)b;
                if (b >= 0xE0 && b <= 0xFF)
                {
                    if ((i + 1 < byteIndex + byteCount) && (bytes[i + 1] > 0))
                    {
                        ch = (char)ConvertTwoBytesToUnicode(b * 256 + bytes[i + 1]);
                        i++;
                    }
                    else
                        ch = (char)ConvertOneByteToUnicode(b);
                }
                chars[j++] = ch;
            }
            return j - charIndex;
        }

        public override int GetMaxByteCount(int charCount) => charCount * 2;

        public override int GetMaxCharCount(int byteCount) => byteCount;

        static int ConvertToAnsel(int unicode)
        {
            return unicode switch
            {
                0x00A1 => 0xC6,//  inverted exclamation mark
                0x00A3 => 0xB9,//  pound sign
                0x00A9 => 0xC3,//  copyright sign
                0x00AE => 0xAA,//  registered trade mark sign
                0x00B0 => 0xC0,//  degree sign, ring above
                0x00B1 => 0xAB,//  plus-minus sign
                0x00B7 => 0xA8,//  middle dot
                0x00B8 => 0xF020,//  cedilla
                0x00BF => 0xC5,//  inverted question mark
                0x00C0 => 0xE141,//  capital A with grave accent
                0x00C1 => 0xE241,//  capital A with acute accent
                0x00C2 => 0xE341,//  capital A with circumflex accent
                0x00C3 => 0xE441,//  capital A with tilde
                0x00C4 => 0xE841,//  capital A with diaeresis
                0x00C5 => 0xEA41,//  capital A with ring above
                0x00C6 => 0xA5,//  capital diphthong A with E
                0x00C7 => 0xF043,//  capital C with cedilla
                0x00C8 => 0xE145,//  capital E with grave accent
                0x00C9 => 0xE245,//  capital E with acute accent
                0x00CA => 0xE345,//  capital E with circumflex accent
                0x00CB => 0xE845,//  capital E with diaeresis
                0x00CC => 0xE149,//  capital I with grave accent
                0x00CD => 0xE249,//  capital I with acute accent
                0x00CE => 0xE349,//  capital I with circumflex accent
                0x00CF => 0xE849,//  capital I with diaeresis
                0x00D0 => 0xA3,//  capital icelandic letter Eth
                0x00D1 => 0xE44E,//  capital N with tilde
                0x00D2 => 0xE14F,//  capital O with grave accent
                0x00D3 => 0xE24F,//  capital O with acute accent
                0x00D4 => 0xE34F,//  capital O with circumflex accent
                0x00D5 => 0xE44F,//  capital O with tilde
                0x00D6 => 0xE84F,//  capital O with diaeresis
                0x00D8 => 0xA2,//  capital O with oblique stroke
                0x00D9 => 0xE155,//  capital U with grave accent
                0x00DA => 0xE255,//  capital U with acute accent
                0x00DB => 0xE355,//  capital U with circumflex
                0x00DC => 0xE855,//  capital U with diaeresis
                0x00DD => 0xE259,//  capital Y with acute accent
                0x00DE => 0xA4,//  capital Icelandic letter Thorn
                0x00DF => 0xCF,//  small German letter sharp s
                0x00E0 => 0xE161,//  small a with grave accent
                0x00E1 => 0xE261,//  small a with acute accent
                0x00E2 => 0xE361,//  small a with circumflex accent
                0x00E3 => 0xE461,//  small a with tilde
                0x00E4 => 0xE861,//  small a with diaeresis
                0x00E5 => 0xEA61,//  small a with ring above
                0x00E6 => 0xB5,//  small diphthong a with e
                0x00E7 => 0xF063,//  small c with cedilla
                0x00E8 => 0xE165,//  small e with grave accent
                0x00E9 => 0xE265,//  small e with acute accent
                0x00EA => 0xE365,//  small e with circumflex accent
                0x00EB => 0xE865,//  small e with diaeresis
                0x00EC => 0xE169,//  small i with grave accent
                0x00ED => 0xE269,//  small i with acute accent
                0x00EE => 0xE369,//  small i with circumflex accent
                0x00EF => 0xE869,//  small i with diaeresis
                0x00F0 => 0xBA,//  small Icelandic letter Eth
                0x00F1 => 0xE46E,//  small n with tilde
                0x00F2 => 0xE16F,//  small o with grave accent
                0x00F3 => 0xE26F,//  small o with acute accent
                0x00F4 => 0xE36F,//  small o with circumflex accent
                0x00F5 => 0xE46F,//  small o with tilde
                0x00F6 => 0xE86F,//  small o with diaeresis
                0x00F8 => 0xB2,//  small o with oblique stroke
                0x00F9 => 0xE175,//  small u with grave accent
                0x00FA => 0xE275,//  small u with acute accent
                0x00FB => 0xE375,//  small u with circumflex
                0x00FC => 0xE875,//  small u with diaeresis
                0x00FD => 0xE279,//  small y with acute accent
                0x00FE => 0xB4,//  small Icelandic letter Thorn
                0x00FF => 0xE879,//  small y with diaeresis
                0x0100 => 0xE541,//  capital a with macron
                0x0101 => 0xE561,//  small a with macron
                0x0102 => 0xE641,//  capital A with breve
                0x0103 => 0xE661,//  small a with breve
                0x0104 => 0xF141,//  capital A with ogonek
                0x0105 => 0xF161,//  small a with ogonek
                0x0106 => 0xE243,//  capital C with acute accent
                0x0107 => 0xE263,//  small c with acute accent
                0x0108 => 0xE343,//  capital c with circumflex
                0x0109 => 0xE363,//  small c with circumflex
                0x010A => 0xE743,//  capital c with dot above
                0x010B => 0xE763,//  small c with dot above
                0x010C => 0xE943,//  capital C with caron
                0x010D => 0xE963,//  small c with caron
                0x010E => 0xE944,//  capital D with caron
                0x010F => 0xE964,//  small d with caron
                0x0110 => 0xA3,//  capital D with stroke
                0x0111 => 0xB3,//  small D with stroke
                0x0112 => 0xE545,//  capital e with macron
                0x0113 => 0xE565,//  small e with macron
                0x0114 => 0xE645,//  capital e with breve
                0x0115 => 0xE665,//  small e with breve
                0x0116 => 0xE745,//  capital e with dot above
                0x0117 => 0xE765,//  small e with dot above
                0x0118 => 0xF145,//  capital E with ogonek
                0x0119 => 0xF165,//  small e with ogonek
                0x011A => 0xE945,//  capital E with caron
                0x011B => 0xE965,//  small e with caron
                0x011C => 0xE347,//  capital g with circumflex
                0x011D => 0xE367,//  small g with circumflex
                0x011E => 0xE647,//  capital g with breve
                0x011F => 0xE667,//  small g with breve
                0x0120 => 0xE747,//  capital g with dot above
                0x0121 => 0xE767,//  small g with dot above
                0x0122 => 0xF047,//  capital g with cedilla
                0x0123 => 0xF067,//  small g with cedilla
                0x0124 => 0xE348,//  capital h with circumflex
                0x0125 => 0xE368,//  small h with circumflex
                0x0128 => 0xE449,//  capital i with tilde
                0x0129 => 0xE469,//  small i with tilde
                0x012A => 0xE549,//  capital i with macron
                0x012B => 0xE569,//  small i with macron
                0x012C => 0xE649,//  capital i with breve
                0x012D => 0xE669,//  small i with breve
                0x012E => 0xF149,//  capital i with ogonek
                0x012F => 0xF169,//  small i with ogonek
                0x0130 => 0xE749,//  capital i with dot above
                0x0131 => 0xB8,//  small dotless i
                0x0134 => 0xE34A,//  capital j with circumflex
                0x0135 => 0xE36A,//  small j with circumflex
                0x0136 => 0xF04B,//  capital k with cedilla
                0x0137 => 0xF06B,//  small k with cedilla
                0x0139 => 0xE24C,//  capital L with acute accent
                0x013A => 0xE26C,//  small l with acute accent
                0x013B => 0xF04C,//  capital l with cedilla
                0x013C => 0xF06C,//  small l with cedilla
                0x013D => 0xE94C,//  capital L with caron
                0x013E => 0xE96C,//  small l with caron
                0x0141 => 0xA1,//  capital L with stroke
                0x0142 => 0xB1,//  small l with stroke
                0x0143 => 0xE24E,//  capital N with acute accent
                0x0144 => 0xE26E,//  small n with acute accent
                0x0145 => 0xF04E,//  capital n with cedilla
                0x0146 => 0xF06E,//  small n with cedilla
                0x0147 => 0xE94E,//  capital N with caron
                0x0148 => 0xE96E,//  small n with caron
                0x014C => 0xE54F,//  capital o with macron
                0x014D => 0xE56F,//  small o with macron
                0x014E => 0xE64F,//  capital o with breve
                0x014F => 0xE66F,//  small o with breve
                0x0150 => 0xEE4F,//  capital O with double acute
                0x0151 => 0xEE6F,//  small o with double acute
                0x0152 => 0xA6,//  capital ligature OE
                0x0153 => 0xB6,//  small ligature OE
                0x0154 => 0xE252,//  capital R with acute accent
                0x0155 => 0xE272,//  small r with acute accent
                0x0156 => 0xF052,//  capital r with cedilla
                0x0157 => 0xF072,//  small r with cedilla
                0x0158 => 0xE952,//  capital R with caron
                0x0159 => 0xE972,//  small r with caron
                0x015A => 0xE253,//  capital S with acute accent
                0x015B => 0xE273,//  small s with acute accent
                0x015C => 0xE353,//  capital s with circumflex
                0x015D => 0xE373,//  small s with circumflex
                0x015E => 0xF053,//  capital S with cedilla
                0x015F => 0xF073,//  small s with cedilla
                0x0160 => 0xE953,//  capital S with caron
                0x0161 => 0xE973,//  small s with caron
                0x0162 => 0xF054,//  capital T with cedilla
                0x0163 => 0xF074,//  small t with cedilla
                0x0164 => 0xE954,//  capital T with caron
                0x0165 => 0xE974,//  small t with caron
                0x0168 => 0xE455,//  capital u with tilde
                0x0169 => 0xE475,//  small u with tilde
                0x016A => 0xE555,//  capital u with macron
                0x016B => 0xE575,//  small u with macron
                0x016C => 0xE655,//  capital u with breve
                0x016D => 0xE675,//  small u with breve
                0x016E => 0xEAAD,//  capital U with ring above
                0x016F => 0xEA75,//  small u with ring above
                0x0170 => 0xEE55,//  capital U with double acute
                0x0171 => 0xEE75,//  small u with double acute
                0x0172 => 0xF155,//  capital u with ogonek
                0x0173 => 0xF175,//  small u with ogonek
                0x0174 => 0xE357,//  capital w with circumflex
                0x0175 => 0xE377,//  small w with circumflex
                0x0176 => 0xE359,//  capital y with circumflex
                0x0177 => 0xE379,//  small y with circumflex
                0x0178 => 0xE859,//  capital y with diaeresis
                0x0179 => 0xE25A,//  capital Z with acute accent
                0x017A => 0xE27A,//  small z with acute accent
                0x017B => 0xE75A,//  capital Z with dot above
                0x017C => 0xE77A,//  small z with dot above
                0x017D => 0xE95A,//  capital Z with caron
                0x017E => 0xE97A,//  small z with caron
                0x01A0 => 0xAC,//  capital O with horn
                0x01A1 => 0xBC,//  small o with horn
                0x01AF => 0xAD,//  capital U with horn
                0x01B0 => 0xBD,//  small u with horn
                0x01CD => 0xE941,//  capital a with caron
                0x01CE => 0xE961,//  small a with caron
                0x01CF => 0xE949,//  capital i with caron
                0x01D0 => 0xE969,//  small i with caron
                0x01D1 => 0xE94F,//  capital o with caron
                0x01D2 => 0xE96F,//  small o with caron
                0x01D3 => 0xE955,//  capital u with caron
                0x01D4 => 0xE975,//  small u with caron
                0x01E2 => 0xE5A5,//  capital ae with macron
                0x01E3 => 0xE5B5,//  small ae with macron
                0x01E6 => 0xE947,//  capital g with caron
                0x01E7 => 0xE967,//  small g with caron
                0x01E8 => 0xE94B,//  capital k with caron
                0x01E9 => 0xE96B,//  small k with caron
                0x01EA => 0xF14F,//  capital o with ogonek
                0x01EB => 0xF16F,//  small o with ogonek
                0x01F0 => 0xE96A,//  small j with caron
                0x01F4 => 0xE247,//  capital g with acute
                0x01F5 => 0xE267,//  small g with acute
                0x01FC => 0xE2A5,//  capital ae with acute
                0x01FD => 0xE2B5,//  small ae with acute
                0x02B9 => 0xA7,//  modified letter prime
                0x02BA => 0xB7,//  modified letter double prime
                0x02BE => 0xAE,//  modifier letter right half ring
                0x02BF => 0xB0,//  modifier letter left half ring
                0x0300 => 0xE1,//  grave accent
                0x0301 => 0xE2,//  acute accent
                0x0302 => 0xE3,//  circumflex accent
                0x0303 => 0xE4,//  tilde
                0x0304 => 0xE5,//  combining macron
                0x0306 => 0xE6,//  breve
                0x0307 => 0xE7,//  dot above
                0x0309 => 0xE0,//  hook above
                0x030A => 0xEA,//  ring above
                0x030B => 0xEE,//  double acute accent
                0x030C => 0xE9,//  caron
                0x0310 => 0xEF,//  candrabindu
                0x0313 => 0xFE,//  comma above
                0x0315 => 0xED,//  comma above right
                0x031C => 0xF8,//  combining half ring below
                0x0323 => 0xF2,//  dot below
                0x0324 => 0xF3,//  diaeresis below
                0x0325 => 0xF4,//  ring below
                0x0326 => 0xF7,//  comma below
                0x0327 => 0xF0,//  combining cedilla
                0x0328 => 0xF1,//  ogonek
                0x032E => 0xF9,//  breve below
                0x0332 => 0xF6,//  low line (= line below?)
                0x0333 => 0xF5,//  double low line
                0x1E00 => 0xF441,//  capital a with ring below
                0x1E01 => 0xF461,//  small a with ring below
                0x1E02 => 0xE742,//  capital b with dot above
                0x1E03 => 0xE762,//  small b with dot above
                0x1E04 => 0xF242,//  capital b with dot below
                0x1E05 => 0xF262,//  small b with dot below
                0x1E0A => 0xE744,//  capital d with dot above
                0x1E0B => 0xE764,//  small d with dot above
                0x1E0C => 0xF244,//  capital d with dot below
                0x1E0D => 0xF264,//  small d with dot below
                0x1E10 => 0xF044,//  capital d with cedilla
                0x1E11 => 0xF064,//  small d with cedilla
                0x1E1E => 0xE746,//  capital f with dot above
                0x1E1F => 0xE766,//  small f with dot above
                0x1E20 => 0xE547,//  capital g with macron
                0x1E21 => 0xE567,//  small g with macron
                0x1E22 => 0xE748,//  capital h with dot above
                0x1E23 => 0xE768,//  small h with dot above
                0x1E24 => 0xF248,//  capital h with dot below
                0x1E25 => 0xF268,//  small h with dot below
                0x1E26 => 0xE848,//  capital h with diaeresis
                0x1E27 => 0xE868,//  small h with diaeresis
                0x1E28 => 0xF048,//  capital h with cedilla
                0x1E29 => 0xF068,//  small h with cedilla
                0x1E2A => 0xF948,//  capital h with breve below
                0x1E2B => 0xF968,//  small h with breve below
                0x1E30 => 0xE24B,//  capital k with acute
                0x1E31 => 0xE26B,//  small k with acute
                0x1E32 => 0xF24B,//  capital k with dot below
                0x1E33 => 0xF26B,//  small k with dot below
                0x1E36 => 0xF24C,//  capital l with dot below
                0x1E37 => 0xF26C,//  small l with dot below
                0x1E3E => 0xE24D,//  capital m with acute
                0x1E3F => 0xE26D,//  small m with acute
                0x1E40 => 0xE74D,//  capital m with dot above
                0x1E41 => 0xE76D,//  small m with dot above
                0x1E42 => 0xF24D,//  capital m with dot below
                0x1E43 => 0xF26D,//  small m with dot below
                0x1E44 => 0xE74E,//  capital n with dot above
                0x1E45 => 0xE76E,//  small n with dot above
                0x1E46 => 0xF24E,//  capital n with dot below
                0x1E47 => 0xF26E,//  small n with dot below
                0x1E54 => 0xE250,//  capital p with acute
                0x1E55 => 0xE270,//  small p with acute
                0x1E56 => 0xE750,//  capital p with dot above
                0x1E57 => 0xE770,//  small p with dot above
                0x1E58 => 0xE752,//  capital r with dot above
                0x1E59 => 0xE772,//  small r with dot above
                0x1E5A => 0xF252,//  capital r with dot below
                0x1E5B => 0xF272,//  small r with dot below
                0x1E60 => 0xE753,//  capital s with dot above
                0x1E61 => 0xE773,//  small s with dot above
                0x1E62 => 0xF253,//  capital s with dot below
                0x1E63 => 0xF273,//  small s with dot below
                0x1E6A => 0xE754,//  capital t with dot above
                0x1E6B => 0xE774,//  small t with dot above
                0x1E6C => 0xF254,//  capital t with dot below
                0x1E6D => 0xF274,//  small t with dot below
                0x1E72 => 0xF355,//  capital u with diaeresis below
                0x1E73 => 0xF375,//  small u with diaeresis below
                0x1E7C => 0xE456,//  capital v with tilde
                0x1E7D => 0xE476,//  small v with tilde
                0x1E7E => 0xF256,//  capital v with dot below
                0x1E7F => 0xF276,//  small v with dot below
                0x1E80 => 0xE157,//  capital w with grave
                0x1E81 => 0xE177,//  small w with grave
                0x1E82 => 0xE257,//  capital w with acute
                0x1E83 => 0xE277,//  small w with acute
                0x1E84 => 0xE857,//  capital w with diaeresis
                0x1E85 => 0xE877,//  small w with diaeresis
                0x1E86 => 0xE757,//  capital w with dot above
                0x1E87 => 0xE777,//  small w with dot above
                0x1E88 => 0xF257,//  capital w with dot below
                0x1E89 => 0xF277,//  small w with dot below
                0x1E8A => 0xE758,//  capital x with dot above
                0x1E8B => 0xE778,//  small x with dot above
                0x1E8C => 0xE858,//  capital x with diaeresis
                0x1E8D => 0xE878,//  small x with diaeresis
                0x1E8E => 0xE759,//  capital y with dot above
                0x1E8F => 0xE779,//  small y with dot above
                0x1E90 => 0xE35A,//  capital z with circumflex
                0x1E91 => 0xE37A,//  small z with circumflex
                0x1E92 => 0xF25A,//  capital z with dot below
                0x1E93 => 0xF27A,//  small z with dot below
                0x1E97 => 0xE874,//  small t with diaeresis
                0x1E98 => 0xEA77,//  small w with ring above
                0x1E99 => 0xEA79,//  small y with ring above
                0x1EA0 => 0xF241,//  capital a with dot below
                0x1EA1 => 0xF261,//  small a with dot below
                0x1EA2 => 0xE041,//  capital a with hook above
                0x1EA3 => 0xE061,//  small a with hook above
                0x1EB8 => 0xF245,//  capital e with dot below
                0x1EB9 => 0xF265,//  small e with dot below
                0x1EBA => 0xE045,//  capital e with hook above
                0x1EBB => 0xE065,//  small e with hook above
                0x1EBC => 0xE445,//  capital e with tilde
                0x1EBD => 0xE465,//  small e with tilde
                0x1EC8 => 0xE049,//  capital i with hook above
                0x1EC9 => 0xE069,//  small i with hook above
                0x1ECA => 0xF249,//  capital i with dot below
                0x1ECB => 0xF269,//  small i with dot below
                0x1ECC => 0xF24F,//  capital o with dot below
                0x1ECD => 0xF26F,//  small o with dot below
                0x1ECE => 0xE04F,//  capital o with hook above
                0x1ECF => 0xE06F,//  small o with hook above
                0x1EE4 => 0xF255,//  capital u with dot below
                0x1EE5 => 0xF275,//  small u with dot below
                0x1EE6 => 0xE055,//  capital u with hook above
                0x1EE7 => 0xE075,//  small u with hook above
                0x1EF2 => 0xE159,//  capital y with grave
                0x1EF3 => 0xE179,//  small y with grave
                0x1EF4 => 0xF259,//  capital y with dot below
                0x1EF5 => 0xF279,//  small y with dot below
                0x1EF6 => 0xE059,//  capital y with hook above
                0x1EF7 => 0xE079,//  small y with hook above
                0x1EF8 => 0xE459,//  capital y with tilde
                0x1EF9 => 0xE479,//  small y with tilde
                0x200C => 0x8E,//  zero width non-joiner
                0x200D => 0x8D,//  zero width joiner
                0x2113 => 0xC1,//  script small l
                0x2117 => 0xC2,//  sound recording copyright
                0x266D => 0xA9,//  music flat sign
                0x266F => 0xC4,//  music sharp sign
                0xFE20 => 0xEB,//  ligature left half
                0xFE21 => 0xEC,//  ligature right half
                0xFE22 => 0xFA,//  double tilde left half
                0xFE23 => 0xFB,//  double tilde right half
                _ => 0xC5,// if no match, use inverted '?'
            };

            /* Note: this conversion table is currently the exact inverse of that used in
            * ANSELInputStreamReader. Ideally it should also provide fallback conversion for
            * UNICODE characters that are never generated by ANSELInputStreamReader, e.g.
            * free-standing accents. For future work.
            */
        }

        /*
        * Conversion table for ANSEL characters coded in one byte
        */

        static int ConvertOneByteToUnicode(int ansel)
        {
            return ansel switch
            {
                0x8D => 0x200D,//  zero width joiner
                0x8E => 0x200C,//  zero width non-joiner
                0xA1 => 0x0141,//  capital L with stroke
                0xA2 => 0x00D8,//  capital O with oblique stroke
                               // case 0xA3: return 0x0110;   capital D with stroke
                0xA3 => 0x00D0,//  capital icelandic letter Eth
                0xA4 => 0x00DE,//  capital Icelandic letter Thorn
                0xA5 => 0x00C6,//  capital diphthong A with E
                0xA6 => 0x0152,//  capital ligature OE
                0xA7 => 0x02B9,//  modified letter prime
                0xA8 => 0x00B7,//  middle dot
                0xA9 => 0x266D,//  music flat sign
                0xAA => 0x00AE,//  registered trade mark sign
                0xAB => 0x00B1,//  plus-minus sign
                0xAC => 0x01A0,//  capital O with horn
                0xAD => 0x01AF,//  capital U with horn
                0xAE => 0x02BE,//  modifier letter right half ring
                0xB0 => 0x02BF,//  modifier letter left half ring
                0xB1 => 0x0142,//  small l with stroke
                0xB2 => 0x00F8,//  small o with oblique stroke
                0xB3 => 0x0111,//  small D with stroke
                0xB4 => 0x00FE,//  small Icelandic letter Thorn
                0xB5 => 0x00E6,//  small diphthong a with e
                0xB6 => 0x0153,//  small ligature OE
                0xB7 => 0x02BA,//  modified letter double prime
                0xB8 => 0x0131,//  small dotless i
                0xB9 => 0x00A3,//  pound sign
                0xBA => 0x00F0,//  small Icelandic letter Eth
                0xBC => 0x01A1,//  small o with horn
                0xBD => 0x01B0,//  small u with horn
                0xC0 => 0x00B0,//  degree sign, ring above
                0xC1 => 0x2113,//  script small l
                0xC2 => 0x2117,//  sound recording copyright
                0xC3 => 0x00A9,//  copyright sign
                0xC4 => 0x266F,//  music sharp sign
                0xC5 => 0x00BF,//  inverted question mark
                0xC6 => 0x00A1,//  inverted exclamation mark
                0xCF => 0x00DF,//  small German letter sharp s
                0xE0 => 0x0309,//  hook above
                0xE1 => 0x0300,//  grave accent
                0xE2 => 0x0301,//  acute accent
                0xE3 => 0x0302,//  circumflex accent
                0xE4 => 0x0303,//  tilde
                0xE5 => 0x0304,//  combining macron
                0xE6 => 0x0306,//  breve
                0xE7 => 0x0307,//  dot above
                0xE9 => 0x030C,//  caron
                0xEA => 0x030A,//  ring above
                0xEB => 0xFE20,//  ligature left half
                0xEC => 0xFE21,//  ligature right half
                0xED => 0x0315,//  comma above right
                0xEE => 0x030B,//  double acute accent
                0xEF => 0x0310,//  candrabindu
                0xF0 => 0x0327,//  combining cedilla
                0xF1 => 0x0328,//  ogonek
                0xF2 => 0x0323,//  dot below
                0xF3 => 0x0324,//  diaeresis below
                0xF4 => 0x0325,//  ring below
                0xF5 => 0x0333,//  double low line
                0xF6 => 0x0332,//  low line (= line below?)
                0xF7 => 0x0326,//  comma below
                0xF8 => 0x031C,//  combining half ring below
                0xF9 => 0x032E,//  breve below
                0xFA => 0xFE22,//  double tilde left half
                0xFB => 0xFE23,//  double tilde right half
                0xFE => 0x0313,//  comma above
                _ => 0xFFFD,// if no match, use Unicode REPLACEMENT CHARACTER
            };
        }

        /*
        * Conversion table for ANSEL characters coded in two bytes
        */

        static int ConvertTwoBytesToUnicode(int ansel)
        {
            return ansel switch
            {
                0xE041 => 0x1EA2,//  capital a with hook above
                0xE045 => 0x1EBA,//  capital e with hook above
                0xE049 => 0x1EC8,//  capital i with hook above
                0xE04F => 0x1ECE,//  capital o with hook above
                0xE055 => 0x1EE6,//  capital u with hook above
                0xE059 => 0x1EF6,//  capital y with hook above
                0xE061 => 0x1EA3,//  small a with hook above
                0xE065 => 0x1EBB,//  small e with hook above
                0xE069 => 0x1EC9,//  small i with hook above
                0xE06F => 0x1ECF,//  small o with hook above
                0xE075 => 0x1EE7,//  small u with hook above
                0xE079 => 0x1EF7,//  small y with hook above
                0xE141 => 0x00C0,//  capital A with grave accent
                0xE145 => 0x00C8,//  capital E with grave accent
                0xE149 => 0x00CC,//  capital I with grave accent
                0xE14F => 0x00D2,//  capital O with grave accent
                0xE155 => 0x00D9,//  capital U with grave accent
                0xE157 => 0x1E80,//  capital w with grave
                0xE159 => 0x1EF2,//  capital y with grave
                0xE161 => 0x00E0,//  small a with grave accent
                0xE165 => 0x00E8,//  small e with grave accent
                0xE169 => 0x00EC,//  small i with grave accent
                0xE16F => 0x00F2,//  small o with grave accent
                0xE175 => 0x00F9,//  small u with grave accent
                0xE177 => 0x1E81,//  small w with grave
                0xE179 => 0x1EF3,//  small y with grave
                0xE241 => 0x00C1,//  capital A with acute accent
                0xE243 => 0x0106,//  capital C with acute accent
                0xE245 => 0x00C9,//  capital E with acute accent
                0xE247 => 0x01F4,//  capital g with acute
                0xE249 => 0x00CD,//  capital I with acute accent
                0xE24B => 0x1E30,//  capital k with acute
                0xE24C => 0x0139,//  capital L with acute accent
                0xE24D => 0x1E3E,//  capital m with acute
                0xE24E => 0x0143,//  capital N with acute accent
                0xE24F => 0x00D3,//  capital O with acute accent
                0xE250 => 0x1E54,//  capital p with acute
                0xE252 => 0x0154,//  capital R with acute accent
                0xE253 => 0x015A,//  capital S with acute accent
                0xE255 => 0x00DA,//  capital U with acute accent
                0xE257 => 0x1E82,//  capital w with acute
                0xE259 => 0x00DD,//  capital Y with acute accent
                0xE25A => 0x0179,//  capital Z with acute accent
                0xE261 => 0x00E1,//  small a with acute accent
                0xE263 => 0x0107,//  small c with acute accent
                0xE265 => 0x00E9,//  small e with acute accent
                0xE267 => 0x01F5,//  small g with acute
                0xE269 => 0x00ED,//  small i with acute accent
                0xE26B => 0x1E31,//  small k with acute
                0xE26C => 0x013A,//  small l with acute accent
                0xE26D => 0x1E3F,//  small m with acute
                0xE26E => 0x0144,//  small n with acute accent
                0xE26F => 0x00F3,//  small o with acute accent
                0xE270 => 0x1E55,//  small p with acute
                0xE272 => 0x0155,//  small r with acute accent
                0xE273 => 0x015B,//  small s with acute accent
                0xE275 => 0x00FA,//  small u with acute accent
                0xE277 => 0x1E83,//  small w with acute
                0xE279 => 0x00FD,//  small y with acute accent
                0xE27A => 0x017A,//  small z with acute accent
                0xE2A5 => 0x01FC,//  capital ae with acute
                0xE2B5 => 0x01FD,//  small ae with acute
                0xE341 => 0x00C2,//  capital A with circumflex accent
                0xE343 => 0x0108,//  capital c with circumflex
                0xE345 => 0x00CA,//  capital E with circumflex accent
                0xE347 => 0x011C,//  capital g with circumflex
                0xE348 => 0x0124,//  capital h with circumflex
                0xE349 => 0x00CE,//  capital I with circumflex accent
                0xE34A => 0x0134,//  capital j with circumflex
                0xE34F => 0x00D4,//  capital O with circumflex accent
                0xE353 => 0x015C,//  capital s with circumflex
                0xE355 => 0x00DB,//  capital U with circumflex
                0xE357 => 0x0174,//  capital w with circumflex
                0xE359 => 0x0176,//  capital y with circumflex
                0xE35A => 0x1E90,//  capital z with circumflex
                0xE361 => 0x00E2,//  small a with circumflex accent
                0xE363 => 0x0109,//  small c with circumflex
                0xE365 => 0x00EA,//  small e with circumflex accent
                0xE367 => 0x011D,//  small g with circumflex
                0xE368 => 0x0125,//  small h with circumflex
                0xE369 => 0x00EE,//  small i with circumflex accent
                0xE36A => 0x0135,//  small j with circumflex
                0xE36F => 0x00F4,//  small o with circumflex accent
                0xE373 => 0x015D,//  small s with circumflex
                0xE375 => 0x00FB,//  small u with circumflex
                0xE377 => 0x0175,//  small w with circumflex
                0xE379 => 0x0177,//  small y with circumflex
                0xE37A => 0x1E91,//  small z with circumflex
                0xE441 => 0x00C3,//  capital A with tilde
                0xE445 => 0x1EBC,//  capital e with tilde
                0xE449 => 0x0128,//  capital i with tilde
                0xE44E => 0x00D1,//  capital N with tilde
                0xE44F => 0x00D5,//  capital O with tilde
                0xE455 => 0x0168,//  capital u with tilde
                0xE456 => 0x1E7C,//  capital v with tilde
                0xE459 => 0x1EF8,//  capital y with tilde
                0xE461 => 0x00E3,//  small a with tilde
                0xE465 => 0x1EBD,//  small e with tilde
                0xE469 => 0x0129,//  small i with tilde
                0xE46E => 0x00F1,//  small n with tilde
                0xE46F => 0x00F5,//  small o with tilde
                0xE475 => 0x0169,//  small u with tilde
                0xE476 => 0x1E7D,//  small v with tilde
                0xE479 => 0x1EF9,//  small y with tilde
                0xE541 => 0x0100,//  capital a with macron
                0xE545 => 0x0112,//  capital e with macron
                0xE547 => 0x1E20,//  capital g with macron
                0xE549 => 0x012A,//  capital i with macron
                0xE54F => 0x014C,//  capital o with macron
                0xE555 => 0x016A,//  capital u with macron
                0xE561 => 0x0101,//  small a with macron
                0xE565 => 0x0113,//  small e with macron
                0xE567 => 0x1E21,//  small g with macron
                0xE569 => 0x012B,//  small i with macron
                0xE56F => 0x014D,//  small o with macron
                0xE575 => 0x016B,//  small u with macron
                0xE5A5 => 0x01E2,//  capital ae with macron
                0xE5B5 => 0x01E3,//  small ae with macron
                0xE641 => 0x0102,//  capital A with breve
                0xE645 => 0x0114,//  capital e with breve
                0xE647 => 0x011E,//  capital g with breve
                0xE649 => 0x012C,//  capital i with breve
                0xE64F => 0x014E,//  capital o with breve
                0xE655 => 0x016C,//  capital u with breve
                0xE661 => 0x0103,//  small a with breve
                0xE665 => 0x0115,//  small e with breve
                0xE667 => 0x011F,//  small g with breve
                0xE669 => 0x012D,//  small i with breve
                0xE66F => 0x014F,//  small o with breve
                0xE675 => 0x016D,//  small u with breve
                0xE742 => 0x1E02,//  capital b with dot above
                0xE743 => 0x010A,//  capital c with dot above
                0xE744 => 0x1E0A,//  capital d with dot above
                0xE745 => 0x0116,//  capital e with dot above
                0xE746 => 0x1E1E,//  capital f with dot above
                0xE747 => 0x0120,//  capital g with dot above
                0xE748 => 0x1E22,//  capital h with dot above
                0xE749 => 0x0130,//  capital i with dot above
                0xE74D => 0x1E40,//  capital m with dot above
                0xE74E => 0x1E44,//  capital n with dot above
                0xE750 => 0x1E56,//  capital p with dot above
                0xE752 => 0x1E58,//  capital r with dot above
                0xE753 => 0x1E60,//  capital s with dot above
                0xE754 => 0x1E6A,//  capital t with dot above
                0xE757 => 0x1E86,//  capital w with dot above
                0xE758 => 0x1E8A,//  capital x with dot above
                0xE759 => 0x1E8E,//  capital y with dot above
                0xE75A => 0x017B,//  capital Z with dot above
                0xE762 => 0x1E03,//  small b with dot above
                0xE763 => 0x010B,//  small c with dot above
                0xE764 => 0x1E0B,//  small d with dot above
                0xE765 => 0x0117,//  small e with dot above
                0xE766 => 0x1E1F,//  small f with dot above
                0xE767 => 0x0121,//  small g with dot above
                0xE768 => 0x1E23,//  small h with dot above
                0xE76D => 0x1E41,//  small m with dot above
                0xE76E => 0x1E45,//  small n with dot above
                0xE770 => 0x1E57,//  small p with dot above
                0xE772 => 0x1E59,//  small r with dot above
                0xE773 => 0x1E61,//  small s with dot above
                0xE774 => 0x1E6B,//  small t with dot above
                0xE777 => 0x1E87,//  small w with dot above
                0xE778 => 0x1E8B,//  small x with dot above
                0xE779 => 0x1E8F,//  small y with dot above
                0xE77A => 0x017C,//  small z with dot above
                0xE841 => 0x00C4,//  capital A with diaeresis
                0xE845 => 0x00CB,//  capital E with diaeresis
                0xE848 => 0x1E26,//  capital h with diaeresis
                0xE849 => 0x00CF,//  capital I with diaeresis
                0xE84F => 0x00D6,//  capital O with diaeresis
                0xE855 => 0x00DC,//  capital U with diaeresis
                0xE857 => 0x1E84,//  capital w with diaeresis
                0xE858 => 0x1E8C,//  capital x with diaeresis
                0xE859 => 0x0178,//  capital y with diaeresis
                0xE861 => 0x00E4,//  small a with diaeresis
                0xE865 => 0x00EB,//  small e with diaeresis
                0xE868 => 0x1E27,//  small h with diaeresis
                0xE869 => 0x00EF,//  small i with diaeresis
                0xE86F => 0x00F6,//  small o with diaeresis
                0xE874 => 0x1E97,//  small t with diaeresis
                0xE875 => 0x00FC,//  small u with diaeresis
                0xE877 => 0x1E85,//  small w with diaeresis
                0xE878 => 0x1E8D,//  small x with diaeresis
                0xE879 => 0x00FF,//  small y with diaeresis
                0xE941 => 0x01CD,//  capital a with caron
                0xE943 => 0x010C,//  capital C with caron
                0xE944 => 0x010E,//  capital D with caron
                0xE945 => 0x011A,//  capital E with caron
                0xE947 => 0x01E6,//  capital g with caron
                0xE949 => 0x01CF,//  capital i with caron
                0xE94B => 0x01E8,//  capital k with caron
                0xE94C => 0x013D,//  capital L with caron
                0xE94E => 0x0147,//  capital N with caron
                0xE94F => 0x01D1,//  capital o with caron
                0xE952 => 0x0158,//  capital R with caron
                0xE953 => 0x0160,//  capital S with caron
                0xE954 => 0x0164,//  capital T with caron
                0xE955 => 0x01D3,//  capital u with caron
                0xE95A => 0x017D,//  capital Z with caron
                0xE961 => 0x01CE,//  small a with caron
                0xE963 => 0x010D,//  small c with caron
                0xE964 => 0x010F,//  small d with caron
                0xE965 => 0x011B,//  small e with caron
                0xE967 => 0x01E7,//  small g with caron
                0xE969 => 0x01D0,//  small i with caron
                0xE96A => 0x01F0,//  small j with caron
                0xE96B => 0x01E9,//  small k with caron
                0xE96C => 0x013E,//  small l with caron
                0xE96E => 0x0148,//  small n with caron
                0xE96F => 0x01D2,//  small o with caron
                0xE972 => 0x0159,//  small r with caron
                0xE973 => 0x0161,//  small s with caron
                0xE974 => 0x0165,//  small t with caron
                0xE975 => 0x01D4,//  small u with caron
                0xE97A => 0x017E,//  small z with caron
                0xEA41 => 0x00C5,//  capital A with ring above
                0xEA61 => 0x00E5,//  small a with ring above
                0xEA75 => 0x016F,//  small u with ring above
                0xEA77 => 0x1E98,//  small w with ring above
                0xEA79 => 0x1E99,//  small y with ring above
                0xEAAD => 0x016E,//  capital U with ring above
                0xEE4F => 0x0150,//  capital O with double acute
                0xEE55 => 0x0170,//  capital U with double acute
                0xEE6F => 0x0151,//  small o with double acute
                0xEE75 => 0x0171,//  small u with double acute
                0xF020 => 0x00B8,//  cedilla
                0xF043 => 0x00C7,//  capital C with cedilla
                0xF044 => 0x1E10,//  capital d with cedilla
                0xF047 => 0x0122,//  capital g with cedilla
                0xF048 => 0x1E28,//  capital h with cedilla
                0xF04B => 0x0136,//  capital k with cedilla
                0xF04C => 0x013B,//  capital l with cedilla
                0xF04E => 0x0145,//  capital n with cedilla
                0xF052 => 0x0156,//  capital r with cedilla
                0xF053 => 0x015E,//  capital S with cedilla
                0xF054 => 0x0162,//  capital T with cedilla
                0xF063 => 0x00E7,//  small c with cedilla
                0xF064 => 0x1E11,//  small d with cedilla
                0xF067 => 0x0123,//  small g with cedilla
                0xF068 => 0x1E29,//  small h with cedilla
                0xF06B => 0x0137,//  small k with cedilla
                0xF06C => 0x013C,//  small l with cedilla
                0xF06E => 0x0146,//  small n with cedilla
                0xF072 => 0x0157,//  small r with cedilla
                0xF073 => 0x015F,//  small s with cedilla
                0xF074 => 0x0163,//  small t with cedilla
                0xF141 => 0x0104,//  capital A with ogonek
                0xF145 => 0x0118,//  capital E with ogonek
                0xF149 => 0x012E,//  capital i with ogonek
                0xF14F => 0x01EA,//  capital o with ogonek
                0xF155 => 0x0172,//  capital u with ogonek
                0xF161 => 0x0105,//  small a with ogonek
                0xF165 => 0x0119,//  small e with ogonek
                0xF169 => 0x012F,//  small i with ogonek
                0xF16F => 0x01EB,//  small o with ogonek
                0xF175 => 0x0173,//  small u with ogonek
                0xF241 => 0x1EA0,//  capital a with dot below
                0xF242 => 0x1E04,//  capital b with dot below
                0xF244 => 0x1E0C,//  capital d with dot below
                0xF245 => 0x1EB8,//  capital e with dot below
                0xF248 => 0x1E24,//  capital h with dot below
                0xF249 => 0x1ECA,//  capital i with dot below
                0xF24B => 0x1E32,//  capital k with dot below
                0xF24C => 0x1E36,//  capital l with dot below
                0xF24D => 0x1E42,//  capital m with dot below
                0xF24E => 0x1E46,//  capital n with dot below
                0xF24F => 0x1ECC,//  capital o with dot below
                0xF252 => 0x1E5A,//  capital r with dot below
                0xF253 => 0x1E62,//  capital s with dot below
                0xF254 => 0x1E6C,//  capital t with dot below
                0xF255 => 0x1EE4,//  capital u with dot below
                0xF256 => 0x1E7E,//  capital v with dot below
                0xF257 => 0x1E88,//  capital w with dot below
                0xF259 => 0x1EF4,//  capital y with dot below
                0xF25A => 0x1E92,//  capital z with dot below
                0xF261 => 0x1EA1,//  small a with dot below
                0xF262 => 0x1E05,//  small b with dot below
                0xF264 => 0x1E0D,//  small d with dot below
                0xF265 => 0x1EB9,//  small e with dot below
                0xF268 => 0x1E25,//  small h with dot below
                0xF269 => 0x1ECB,//  small i with dot below
                0xF26B => 0x1E33,//  small k with dot below
                0xF26C => 0x1E37,//  small l with dot below
                0xF26D => 0x1E43,//  small m with dot below
                0xF26E => 0x1E47,//  small n with dot below
                0xF26F => 0x1ECD,//  small o with dot below
                0xF272 => 0x1E5B,//  small r with dot below
                0xF273 => 0x1E63,//  small s with dot below
                0xF274 => 0x1E6D,//  small t with dot below
                0xF275 => 0x1EE5,//  small u with dot below
                0xF276 => 0x1E7F,//  small v with dot below
                0xF277 => 0x1E89,//  small w with dot below
                0xF279 => 0x1EF5,//  small y with dot below
                0xF27A => 0x1E93,//  small z with dot below
                0xF355 => 0x1E72,//  capital u with diaeresis below
                0xF375 => 0x1E73,//  small u with diaeresis below
                0xF441 => 0x1E00,//  capital a with ring below
                0xF461 => 0x1E01,//  small a with ring below
                0xF948 => 0x1E2A,//  capital h with breve below
                0xF968 => 0x1E2B,//  small h with breve below
                _ => 0xFFFD,// if no match, use Unicode REPLACEMENT CHARACTER
            };
        }
    }   
}
