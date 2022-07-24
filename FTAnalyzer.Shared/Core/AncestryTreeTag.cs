namespace FTAnalyzer
{
    public class AncestryTreeTag
    {
        public string TagID { get; }
        public string Name { get; }
        public string RIN { get; }

        public AncestryTreeTag(string tag, string name, string rin)
        {
            TagID = tag;
            Name = name;
            RIN = rin;
        }

        public override string ToString() => string.IsNullOrEmpty(RIN) ? $"{TagID} : {Name}" : $"{TagID} : {Name} ({RIN})";

        enum AncestryType
        {
            Adppted_into_Family = 1,
            Adopted_out_of_Family = 2,
            Actively_Researching = 3,
            Brick_Wall = 4,
            Common_DNA_Ancestor = 5,
            Complete = 6,
            Died_Young = 7,
            Direct_Ancestor = 8,
            DNA_Connection = 9,
            DNA_Match = 10,
            Enslaved = 11,
            Free_Person_of_Colour = 12,
            Hypothesis = 13,
            Immigrant = 14,
            Indentured_Servant = 15,
            Military_Service = 16,
            Multiple_Spouses = 17,
            Never_Married = 18,
            No_Children = 19,
            Orphan = 20,
            Royalty_Nobility = 21,
            Slave_Owner = 22,
            Unverified = 23,
            Verified = 24
        }
    }
}
