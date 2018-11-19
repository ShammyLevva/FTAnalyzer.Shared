using System;

namespace FTAnalyzer
{
    public class ExportFact
    {
        public Individual Ind { get; set; }
        public Fact F { get; set; }

        public ExportFact(Individual ind, Fact f)
        {
            Ind = ind;
            F = f;
        }

        public string ID { get { return Ind.IndividualID; } }
        public string Forenames { get { return Ind.Forenames; } }
        public string Surname { get { return Ind.Surname; } }
        public string Gender { get { return Ind.Gender; } }
        public string FactType { get { return F.FactTypeDescription; } }
        public string FactDate { get { return F.FactDate.ToString(); } }
        public string FactLocation { get { return F.Location.ToString(); } }
        public string FactComment { get { return F.Comment; } }
        public string SortableLocation { get { return F.Location.SortableLocation; } }
        public DateTime StartDate { get { return F.FactDate.StartDate; } }
        public DateTime EndDate { get { return F.FactDate.EndDate; } }
        public int RelationType { get { return Ind.RelationType; } }
    }
}
