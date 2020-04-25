using System;
using System.Text;

namespace FTAnalyzer
{
    public class ComboBoxFamily : Family
    {
        public ComboBoxFamily(Family family)
            : base(family)
        { }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            if (Husband != null)
                result.Append(Husband.Name);
            if(Wife != null)
                result.Append(Husband == null ? Wife.Name : " and " + Wife.Name);
            if (result.Length > 0)
                return $"{FamilyID}: {result.ToString()} {base.ToString()}";
            return $"{FamilyID}: {base.ToString()}";
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(ComboBoxFamily))
                return false;
            Family that = obj as Family;
            return FamilyID.Equals(that.FamilyID, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode() => base.GetHashCode();
    }
}
