using FTAnalyzer.Utilities;

namespace FTAnalyzer
{
    public interface IDisplayFamily : IColumnComparer<IDisplayFamily>
    {
        [ColumnDetail("Family Ref", 60)]
        string FamilyID { get; }
        [ColumnDetail("Husband ID", 70)]
        string HusbandID { get; }
        [ColumnDetail("Husband Name", 250)]
        string Husband { get; }
        [ColumnDetail("Wife ID", 70)]
        string WifeID { get; }
        [ColumnDetail("Wife Name", 250)]
        string Wife { get; }
        [ColumnDetail("Marriage Detail", 300)]
        string Marriage { get; }
        [ColumnDetail("Location", 250)]
        FactLocation Location { get; }
        [ColumnDetail("Childrens Details", 350)]
        string Children { get; }
        [ColumnDetail("Family Size", 80, ColumnDetail.ColumnAlignment.Right)]
        int FamilySize { get; }
        [ColumnDetail("Husband Surname", 75)]
        string HusbandSurname { get; }
        [ColumnDetail("Husband Forenames", 100)]
        string HusbandForenames { get; }
        [ColumnDetail("Wife Surname", 75)]
        string WifeSurname { get; }
        [ColumnDetail("Wife Forenames", 100)]
        string WifeForenames { get; }
        [ColumnDetail("Marital Status", 100)]
        string MaritalStatus { get; }
    }
}
