using System;
using System.Collections.Generic;

namespace FTAnalyzer
{
    public class LostCousinsReferralComparer : Comparer<ExportReferrals>
    {
        public override int Compare(ExportReferrals x, ExportReferrals y)
        {
            if (x == null || y == null)
                return 0;
            if (x.ShortCode.Equals(y.ShortCode, StringComparison.OrdinalIgnoreCase))
            {
                if (x.Census.Equals(y.Census, StringComparison.OrdinalIgnoreCase))
                {
                    if (x.CensusReference.Equals(y.CensusReference, StringComparison.OrdinalIgnoreCase))
                    {
                        if (x.FamilyID.Equals(y.FamilyID, StringComparison.OrdinalIgnoreCase))
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
