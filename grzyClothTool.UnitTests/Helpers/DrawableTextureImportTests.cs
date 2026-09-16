using grzyClothTool.Helpers;

namespace grzyClothTool.UnitTests.Helpers;

public class DrawableTextureImportTests
{
    [Fact]
    public void MatchingTextures_UsesSameDlcAndExactNumber_AndSortsVariants()
    {
        using var temp = new TestTempDirectory();
        var source = temp.FilePath("mp_f_freemode_01_talecloth01^jbib_005_u.ydd");
        var first = temp.FilePath("mp_f_freemode_01_talecloth01^jbib_diff_005_a_uni.ytd");
        var second = temp.FilePath("mp_f_freemode_01_talecloth01^jbib_diff_005_b_uni.YTD");
        File.WriteAllText(second, "");
        File.WriteAllText(first, "");
        File.WriteAllText(temp.FilePath("mp_f_freemode_01_talecloth02^jbib_diff_005_a_uni.ytd"), "");
        File.WriteAllText(temp.FilePath("mp_f_freemode_01_talecloth01^jbib_diff_0050_a_uni.ytd"), "");
        Assert.Equal([first, second], FileHelper.FindMatchingTextures(source, "p_head", true));
    }

    [Theory]
    [InlineData("custom_shirt", "custom_shirt")]
    [InlineData("custom_shirt", "custom_shirt_a")]
    [InlineData("p_head_002", "p_head_diff_002_a")]
    [InlineData("jbib_005_u", "jbib_005_u")]
    public void MatchingTextures_HandlesCustomNamesAndProps_WhenOutputTypeIsOverridden(string drawableName, string textureName)
    {
        using var temp = new TestTempDirectory();
        var texturePath = temp.FilePath(textureName + ".ytd");
        File.WriteAllText(texturePath, "");
        File.WriteAllText(temp.FilePath("unrelated.ytd"), "");
        Assert.Equal([texturePath], FileHelper.FindMatchingTextures(temp.FilePath(drawableName + ".ydd"), "jbib", false));
    }

    [Fact]
    public void MatchingTextures_CachedAndUncachedResultsAgree()
    {
        using var temp = new TestTempDirectory();
        var source = temp.FilePath("jbib_000_u.ydd");
        File.WriteAllText(temp.FilePath("jbib_diff_000_b_uni.ytd"), "");
        File.WriteAllText(temp.FilePath("jbib_diff_000_a_uni.ytd"), "");
        try
        {
            Assert.Equal(FileHelper.FindMatchingTextures(source, "jbib", false),
                FileHelper.FindMatchingTextures(source, "jbib", false, useFolderCache: true));
        }
        finally { FileHelper.ClearTextureSearchCache(); }
    }
}
