using System;
using System.Collections.Generic;
using System.Text;

namespace FTAnalyzer
{
    public enum RelationshipType
    {
        // edefine relation type from direct ancestor to related by marriage and 
        // MARRIAGEDB ie: married to a direct or blood relation
        UNKNOWN = 1,
        DIRECT = 2,
        DESCENDANT = 4,
        BLOOD = 8,
        MARRIEDTODB = 16,
        MARRIAGE = 32,
        LINKED = 64,
        UNSET = 128
    }
}
