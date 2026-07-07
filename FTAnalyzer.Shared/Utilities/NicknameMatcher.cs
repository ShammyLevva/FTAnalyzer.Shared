namespace FTAnalyzer
{
    /// <summary>
    /// Recognises common English-language nickname/diminutive pairs (e.g. Margaret/Maggie,
    /// William/Bill) that DoubleMetaphone cannot catch, since a nickname is a different word
    /// rather than a phonetic spelling variant. Used to disambiguate household members who share
    /// a census reference when reconciling Lost Cousins My Ancestors entries against tree
    /// individuals. Deliberately conservative: names with genuinely ambiguous historical usage
    /// (e.g. Elsie for both Elizabeth and Alice) are left out rather than risking a false match.
    /// </summary>
    public static class NicknameMatcher
    {
        static readonly string[][] Groups =
        [
            // ── Female ──────────────────────────────────────────────
            ["MARGARET", "MAGGIE", "MEG", "MEGGIE", "PEGGY", "PEG", "MADGE", "MARGE", "GRETA"],
            ["ELIZABETH", "ELIZA", "LIZA", "LIZZIE", "BESS", "BESSIE", "BETTY", "BETSY", "LIBBY"],
            ["MARY", "MOLLY", "POLLY", "MAMIE", "MAY"],
            ["CATHERINE", "KATHERINE", "CATHARINE", "KATHARINE", "KATE", "KATIE", "CATHY", "KATHY", "KITTY", "CASSIE"],
            ["ANN", "ANNE", "ANNIE", "NAN", "NANCY", "NANNY"],
            ["SARAH", "SARA", "SALLY", "SADIE"],
            ["JANE", "JEAN", "JEANNIE", "JENNY", "JINNY"],
            ["SUSAN", "SUSANNAH", "SUSANNA", "SUSIE", "SUKY"],
            ["DOROTHY", "DOT", "DOTTIE", "DOLLY"],
            ["FRANCES", "FANNY", "FRAN", "FRANKIE"],
            ["HARRIET", "HATTIE", "HETTY"],
            ["CHARLOTTE", "LOTTIE", "CARLOTTA"],
            ["ALICE", "ALLIE", "ALICIA"],
            ["REBECCA", "BECKY", "BECCA"],
            ["MARTHA", "PATTY", "MATTIE", "MATTY"],
            ["WINIFRED", "WINNIE", "FREDA"],
            ["AGNES", "NESSIE", "AGGIE"],
            ["ISABELLA", "ISABEL", "ISOBEL", "BELLA", "BELL", "TIBBIE"],
            ["CHRISTINA", "CHRISTINE", "CHRISSIE", "TEENIE", "TINA"],
            ["JANET", "JESSIE", "NETTIE"],
            ["HELEN", "ELLEN", "NELL", "NELLIE", "ELLA", "HELENA", "LENA"],
            ["GEORGINA", "GEORGIE"],
            ["FLORENCE", "FLO", "FLOSSIE"],
            ["AMELIA", "MILLIE", "AMY"],

            // ── Male ────────────────────────────────────────────────
            ["WILLIAM", "WILL", "WILLIE", "BILL", "BILLY"],
            ["JOHN", "JACK", "JACKIE", "JOCK", "JOHNNY"],
            ["ROBERT", "BOB", "BOBBY", "ROB", "ROBBIE"],
            ["RICHARD", "DICK", "DICKIE", "RICK", "RICKY", "RICHIE"],
            ["CHARLES", "CHARLIE", "CHUCK", "CHAS"],
            ["THOMAS", "TOM", "TOMMY"],
            ["JAMES", "JIM", "JIMMY", "JAMIE", "JEM"],
            ["EDWARD", "TED", "TEDDY", "ED", "EDDIE", "NED"],
            ["HENRY", "HARRY", "HAL", "HANK"],
            ["ALEXANDER", "ALEX", "ALEC", "SANDY"],
            ["FREDERICK", "FRED", "FREDDIE", "FRITZ"],
            ["GEORGE", "GEORGIE"],
            ["SAMUEL", "SAM", "SAMMY"],
            ["JOSEPH", "JOE", "JOEY"],
            ["DAVID", "DAVE", "DAVY"],
            ["DANIEL", "DAN", "DANNY"],
            ["BENJAMIN", "BEN", "BENNY"],
            ["NATHANIEL", "NATHAN", "NAT"],
            ["PETER", "PETE"],
            ["ANDREW", "ANDY", "DREW"],
            ["ANTHONY", "TONY"],
            ["CHRISTOPHER", "CHRIS", "KIT"],
            ["MATTHEW", "MATT"],
            ["MICHAEL", "MIKE", "MICKEY"],
            ["PATRICK", "PAT", "PADDY"],
            ["FRANCIS", "FRANK", "FRANKIE"],
            ["TIMOTHY", "TIM", "TIMMY"],
            ["WALTER", "WALLY", "WALT"],
            ["HERBERT", "HERBIE"],
            ["ALBERT", "BERT", "BERTIE", "AL"],
            ["ERNEST", "ERNIE"],
            ["ARTHUR", "ART", "ARTIE"],
            ["STEPHEN", "STEVEN", "STEVE", "STEVIE"],
            ["VINCENT", "VINCE"],
            ["LAWRENCE", "LARRY", "LAURIE"],
            ["GREGORY", "GREG"],
            ["NICHOLAS", "NICK", "NICKY"],
            ["ISAAC", "IKE"],
            ["JACOB", "JAKE"],
            ["ABRAHAM", "ABE"],
            ["REGINALD", "REG", "REGGIE"],
            ["CECIL", "CES"],
            ["PERCIVAL", "PERCY"],
            ["SIDNEY", "SID"],
            ["LEONARD", "LEN", "LENNY"],
            ["WILFRED", "WILF"],
            ["KENNETH", "KEN", "KENNY"],
            ["RONALD", "RON", "RONNIE"],
            ["DONALD", "DON", "DONNIE"],
            ["DOUGLAS", "DOUG"],
            ["NORMAN", "NORM"],
        ];

        static readonly Dictionary<string, string> CanonicalOf = BuildLookup();

        static Dictionary<string, string> BuildLookup()
        {
            Dictionary<string, string> lookup = [];
            foreach (var group in Groups)
                foreach (var name in group)
                    lookup[name] = group[0];
            return lookup;
        }

        /// <summary>
        /// True if the two forenames are the same name, or known nickname/formal-name variants
        /// of each other (e.g. "Maggie" and "Margaret"). Case-insensitive; does not attempt
        /// phonetic matching — pair with DoubleMetaphone for spelling-variant matches.
        /// </summary>
        public static bool AreEquivalent(string? name1, string? name2)
        {
            if (string.IsNullOrWhiteSpace(name1) || string.IsNullOrWhiteSpace(name2))
                return false;
            string n1 = name1.Trim().ToUpperInvariant();
            string n2 = name2.Trim().ToUpperInvariant();
            if (n1 == n2)
                return true;
            return CanonicalOf.TryGetValue(n1, out var c1) && CanonicalOf.TryGetValue(n2, out var c2) && c1 == c2;
        }
    }
}
