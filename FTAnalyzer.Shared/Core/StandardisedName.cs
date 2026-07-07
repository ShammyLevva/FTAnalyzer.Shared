namespace FTAnalyzer
{
    public class StandardisedName : IEquatable<StandardisedName>
    {
        public bool IsMale { get; private set; }
        public string Name { get; private set; }

        public StandardisedName(int sex, string name)
        {
            IsMale = sex != 1;  // 1 female, 2 male, anything else male
            Name = name;
        }

        public StandardisedName(bool male, string name)
        {
            IsMale = male;
            Name = name;
        }

        public override string ToString()
        {
            return (IsMale ? "Male :" : "Female :") + Name;
        }

        // Used as a Dictionary key (FamilyTree.names) — without value equality every lookup
        // missed and GetStandardisedName silently returned the input name unchanged.
        public bool Equals(StandardisedName? other) =>
            other is not null && IsMale == other.IsMale && string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object? obj) => Equals(obj as StandardisedName);

        public override int GetHashCode() => HashCode.Combine(IsMale, Name.ToUpperInvariant());
    }
}
