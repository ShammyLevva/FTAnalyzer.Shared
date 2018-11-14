using FTAnalyzer.Utilities;

namespace FTAnalyzer
{
    public interface IDisplayChildrenStatus
    {
        [ColumnDetail("Family Ref", 60)]
        string FamilyID { get; }
        [ColumnDetail("Surname", 75)]
        string Surname { get; }
        [ColumnDetail("Husband ID", 70)]
        string HusbandID { get; }
        [ColumnDetail("Husband Name", 250)]
        string Husband { get; }
        [ColumnDetail("Wife ID", 70)]
        string WifeID { get; }
        [ColumnDetail("Wife Name", 250)]
        string Wife { get; }
        [ColumnDetail("Children Total", 75, ColumnDetail.ColumnAlignment.Right)]
        int ChildrenTotal { get; }
        [ColumnDetail("Children Alive", 75, ColumnDetail.ColumnAlignment.Right)]
        int ChildrenAlive { get; }
        [ColumnDetail("Children Dead", 75, ColumnDetail.ColumnAlignment.Right)]
        int ChildrenDead { get; }
        [ColumnDetail("Expected Total", 75, ColumnDetail.ColumnAlignment.Right)]
        int ExpectedTotal { get; }
        [ColumnDetail("Expected Alive", 75, ColumnDetail.ColumnAlignment.Right)]
        int ExpectedAlive { get; }
        [ColumnDetail("Expected Dead", 75, ColumnDetail.ColumnAlignment.Right)]
        int ExpectedDead { get; }
    }
}
