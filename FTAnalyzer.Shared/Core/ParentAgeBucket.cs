namespace FTAnalyzer
{
    public record ParentAgeBucket(int MinAge, int MaxAge,
        int FatherSons, int FatherDaughters, int FatherUnknown,
        int MotherSons, int MotherDaughters, int MotherUnknown)
    {
        public string AgeLabel => $"{MinAge}-{MaxAge}";
        public int FatherTotal => FatherSons + FatherDaughters + FatherUnknown;
        public int MotherTotal => MotherSons + MotherDaughters + MotherUnknown;
    }
}
