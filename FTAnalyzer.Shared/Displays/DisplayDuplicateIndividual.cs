namespace FTAnalyzer
{
    public class DisplayDuplicateIndividual : IDisplayDuplicateIndividual
    {
        Individual IndA { get; }
        Individual IndB { get; }
        public int Score { get; private set; }
        public bool IgnoreNonDuplicate { get; set; }

        public DisplayDuplicateIndividual(DuplicateIndividual dup)
        {
            IndA = dup.IndividualA;
            IndB = dup.IndividualB;
            Score = dup.Score;
        }

        public string IndividualID => IndA.IndividualID;
        public string Name => IndA.Name;
        public string Forenames => IndA.Forenames;
        public string Surname => IndA.Surname;
        public FactDate BirthDate => IndA.BirthDate;
        public FactLocation BirthLocation => IndA.BirthLocation;
        public string Gender => IndA.Gender;
        public string MatchIndividualID => IndB.IndividualID;
        public string MatchName => IndB.Name;
        public FactDate MatchBirthDate => IndB.BirthDate;
        public FactLocation MatchBirthLocation => IndB.BirthLocation;
        public string MatchGender => IndB.Gender;
    }
}
