using System;

namespace FTAnalyzer
{
    [Serializable]
    public class NonDuplicate
    {
        public NonDuplicate()
        { }

        public NonDuplicate(DisplayDuplicateIndividual dup)
        {
            IndividualA_ID = dup.IndividualID;
            IndividualA_Name = dup.Name;
            IndividualA_BirthDate = dup.BirthDate.ToString();
            IndividualB_ID = dup.MatchIndividualID;
            IndividualB_Name = dup.MatchName;
            IndividualB_BirthDate = dup.MatchBirthDate.ToString();
        }

        public string IndividualA_ID { get; set; }
        public string IndividualA_Name { get; set; }
        public string IndividualA_BirthDate { get; set; }
        public string IndividualB_ID { get; set; }
        public string IndividualB_Name { get; set; }
        public string IndividualB_BirthDate { get; set; }

        public override bool Equals(object obj)
        {
            NonDuplicate that = (NonDuplicate)obj;
            bool result = (IndividualA_ID == that.IndividualA_ID &&
                   IndividualA_Name == that.IndividualA_Name &&
                   IndividualA_BirthDate == that.IndividualA_BirthDate &&
                   IndividualB_ID == that.IndividualB_ID &&
                   IndividualB_Name == that.IndividualB_Name &&
                   IndividualB_BirthDate == that.IndividualB_BirthDate)
                ||
                (IndividualA_ID == that.IndividualB_ID &&
                   IndividualA_Name == that.IndividualB_Name &&
                   IndividualA_BirthDate == that.IndividualB_BirthDate &&
                   IndividualB_ID == that.IndividualA_ID &&
                   IndividualB_Name == that.IndividualA_Name &&
                   IndividualB_BirthDate == that.IndividualA_BirthDate);
            return result;
        }

        public override int GetHashCode() => base.GetHashCode();
    }
}
