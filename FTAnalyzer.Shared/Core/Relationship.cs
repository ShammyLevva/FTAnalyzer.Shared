using System;
using System.Linq;
using System.Numerics;
using System.Text;

namespace FTAnalyzer
{
    public static class Relationship
    {
        public static string CalculateRelationship(Individual rootPerson, Individual indToFind)
        {
            if (rootPerson.Equals(indToFind))
                return "root person";
            CommonAncestor commonAncestor = indToFind.CommonAncestor;
            long rootDistance = (long)(BigInteger.Log(commonAncestor.Ind.Ahnentafel) / Math.Log(2.0));
            long toFindDistance = commonAncestor.Distance;

            // DIRECT DESCENDANT - PARENT
            if (toFindDistance == 0)
            {
                string relation = indToFind.IsMale ? "father" : "mother";
                indToFind.RelationSort = rootDistance;
                return (commonAncestor.Step ? "step " : string.Empty) + AggrandiseRelationship(relation, rootDistance, 0);
            }
            // DIRECT DESCENDANT - CHILD
            if (rootDistance == 0)
            {
                string relation = indToFind.IsMale ? "son" : "daughter";
                indToFind.RelationSort = -toFindDistance;
                indToFind.RelationType = Individual.DESCENDANT;
                return (commonAncestor.Step ? "step " : string.Empty) + AggrandiseRelationship(relation, toFindDistance, 0);
            }
            // EQUAL DISTANCE - SIBLINGS / PERFECT COUSINS
            if (rootDistance == toFindDistance)
            {
                switch (toFindDistance)
                {
                    case 1:
                        return (commonAncestor.Step ? "half " : string.Empty) + (indToFind.IsMale ? "brother" : "sister");
                    case 2:
                        return "cousin";
                    default:
                        return $"{OrdinalSuffix(toFindDistance - 1)} cousin";
                }
            }
            // AUNT / UNCLE
            if (toFindDistance == 1)
            {
                string relation = indToFind.IsMale ? "uncle" : "aunt";
                return AggrandiseRelationship(relation, rootDistance, 1);
            }
            // NEPHEW / NIECE
            if (rootDistance == 1)
            {
                string relation = indToFind.IsMale ? "nephew" : "niece";
                return AggrandiseRelationship(relation, toFindDistance, 1);
            }
            // COUSINS, GENERATIONALLY REMOVED
            long cousinOrdinal = Math.Min(rootDistance, toFindDistance) - 1;
            long cousinGenerations = Math.Abs(rootDistance - toFindDistance);
            return $"{OrdinalSuffix(cousinOrdinal)} cousin {FormatPlural(cousinGenerations)} removed";
        }

        static string FormatPlural(long count)
        {
            if (Math.Abs(count) == 1)
                return "once";
            if (Math.Abs(count) == 2)
                return "twice";
            return count + " times";
        }

        static string AggrandiseRelationship(string relation, long distance, int offset)
        {
            distance -= offset;
            switch (distance)
            {
                case 1:
                    return relation;
                case 2:
                    return "grand" + relation;
                case 3:
                    return "great grand" + relation;
                default:
                    return OrdinalSuffix(distance - 2) + " great grand" + relation;
            }
        }

        static string OrdinalSuffix(long number)
        {
            string os = string.Empty;
            if (number % 100 > 10 && number % 100 < 14)
                os = "th";
            else if (number == 0)
                os = "";
            else
            {
                decimal last = number % 10;
                switch (last)
                {
                    case 1:
                        os = "st";
                        break;
                    case 2:
                        os = "nd";
                        break;
                    case 3:
                        os = "rd";
                        break;
                    default:
                        os = "th";
                        break;
                }
            }
            return number + os;
        }

        public static string AhnentafelToString(BigInteger ahnentafel)
        {
            StringBuilder output = new StringBuilder();
            StringBuilder relations = new StringBuilder();
            output.Append(FamilyTree.Instance.RootPerson.Name);
            if(ahnentafel !=1) output.Append("'s ");
            while (ahnentafel != 1)
            {
                if (ahnentafel % 2 == 0)
                    relations.Append("father's ");
                else
                {
                    ahnentafel -= 1;
                    relations.Append("mother's ");
                }
                ahnentafel /= 2;
            }
            output.Append(string.Join(" ", relations.ToString().Split(' ').Reverse()));
            output.Replace("  ", " ");
            //remove last 's
            return output.ToString().Substring(0,output.Length -2);
        }
    }
}
