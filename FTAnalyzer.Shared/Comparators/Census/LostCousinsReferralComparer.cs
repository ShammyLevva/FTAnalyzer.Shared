using System;
using System.Collections.Generic;

namespace FTAnalyzer
{
    public class LostCousinsReferralComparer : Comparer<ExportReferrals>
    {
        public override int Compare(ExportReferrals x, ExportReferrals y)
        {
            if (x.ShortCode.Equals(y.ShortCode))
            {
                if (x.Census.Equals(y.Census))
                {
                    if (x.CensusReference.Equals(y.CensusReference))
                    {
                        if (x.FamilyID.Equals(y.FamilyID))
                        {
                            if (x.Age.Equals(y.Age))
                            {
                                return x.Surname == y.Surname
                                    ? string.Compare(x.Forenames, y.Forenames, StringComparison.Ordinal)
                                    : string.Compare(x.Surname, y.Surname, StringComparison.Ordinal);
                            }
                            return y.Age.CompareTo(x.Age);
                        }
                        return string.Compare(x.FamilyID, y.FamilyID, StringComparison.Ordinal);
                    }
                    return string.Compare(x.CensusReference, y.CensusReference, StringComparison.Ordinal);
                }
                return string.Compare(x.Census, y.Census, StringComparison.Ordinal);
            }
            return string.Compare(x.ShortCode, y.ShortCode, StringComparison.Ordinal);
        }
    }
}
