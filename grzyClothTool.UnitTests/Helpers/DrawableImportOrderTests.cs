using grzyClothTool.Helpers;

namespace grzyClothTool.UnitTests.Helpers;

public class DrawableImportOrderTests
{
    [Theory]
    [InlineData("mp_f_freemode_01_talecloth01^jbib_002_u.ydd", "talecloth01")]
    [InlineData("mp_m_freemode_01_p_talecloth01^p_head_000.ydd", "talecloth01")]
    [InlineData("talecloth02^jbib_000_u.ydd", "talecloth02")]
    [InlineData("jbib_000_u.ydd", "")]
    public void GetDlcName_RemovesPedPrefix(string path, string expected)
    {
        Assert.Equal(expected, DrawableImportOrder.GetDlcName(path));
    }

    [Fact]
    public void OrderFiles_CompletesDlcBeforeNext_AndSortsByFilenameInsteadOfDrawableNumber()
    {
        string[] expected =
        [
            "mp_f_freemode_01_talecloth2^accs_010_u.ydd",
            "mp_f_freemode_01_talecloth2^jbib_002_u.ydd",
            "mp_m_freemode_01_talecloth2^jbib_000_u.ydd",
            "mp_f_freemode_01_talecloth10^accs_000_u.ydd",
            "jbib_000_u.ydd"
        ];
        Assert.Equal(expected, DrawableImportOrder.OrderFiles(expected.Reverse()));
    }

    [Fact]
    public void OrderFiles_DeduplicatesPathsIgnoringCase()
    {
        Assert.Single(DrawableImportOrder.OrderFiles(["pack^jbib_000_u.ydd", "PACK^JBIB_000_U.YDD"]));
    }

    [Fact]
    public void OrderFiles_GroupsGendersAndPropsUnderTheSameDlc()
    {
        string[] paths =
        [
            "mp_m_freemode_01_talecloth02^jbib_000_u.ydd",
            "mp_f_freemode_01_talecloth01^jbib_001_u.ydd",
            "mp_m_freemode_01_p_TALECLOTH01^p_head_000.ydd",
            "mp_f_freemode_01_talecloth02^jbib_000_u.ydd"
        ];
        var groups = DrawableImportOrder.OrderFiles(paths)
            .GroupBy(DrawableImportOrder.GetDlcName, StringComparer.OrdinalIgnoreCase).ToArray();
        Assert.Equal(2, groups.Length);
        Assert.Equal("talecloth01", groups[0].Key, ignoreCase: true);
        Assert.Equal(2, groups[0].Count());
        Assert.Equal("talecloth02", groups[1].Key, ignoreCase: true);
        Assert.Equal(2, groups[1].Count());
    }
}
