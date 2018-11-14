using FTAnalyzer.Utilities;

namespace FTAnalyzer
{
    public interface IDisplayFamily
    {
        [ColumnDetail("Family Ref",60)]
        string FamilyID { get; }
        [ColumnDetail("Husband ID",70)]
        string HusbandID { get; }
        [ColumnDetail("Husband Name",250)]
        string Husband { get; }
        [ColumnDetail("Wife ID", 70)]
        string WifeID { get; }
        [ColumnDetail("Wife Name", 250)]
        string Wife { get; }
        [ColumnDetail("Marriage Detail", 300)]
        string Marriage { get; }
        [ColumnDetail("Location",250)]
        FactLocation Location { get; }
        [ColumnDetail("Childrens Details", 350)]
        string Children { get; }
        [ColumnDetail("Family Size", 80, ColumnDetail.ColumnAlignment.Right)]
        int FamilySize { get; }
    }
}
