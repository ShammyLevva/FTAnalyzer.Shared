using System;
using System.Collections.Generic;
using System.Text;

namespace FTAnalyzer
{
    public interface IDisplayLostCousin
    {
        string Name { get; }
        int BirthYear { get; }
        string Reference { get; }
        CensusDate CensusDate { get; }
        bool FTAnalyzerFact { get; }
        bool Verified { get; }
    }
}
