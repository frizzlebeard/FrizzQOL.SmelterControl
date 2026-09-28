using SmelterControl;
using Xunit;

public class SmelterRulesTests
{
    [Fact]
    public void Zero_or_negative_keeps_the_game_value()
    {
        Assert.Equal(30f, SmelterRules.ResolveSeconds(0f, 30f));
        Assert.Equal(30f, SmelterRules.ResolveSeconds(-1f, 30f));
        Assert.Equal(10, SmelterRules.ResolveCount(0, 10));
        Assert.Equal(20, SmelterRules.ResolveCount(-4, 20));
    }

    [Fact]
    public void Positive_numbers_replace_the_game_value()
    {
        Assert.Equal(10f, SmelterRules.ResolveSeconds(10f, 30f));
        Assert.Equal(20, SmelterRules.ResolveCount(20, 10));
        Assert.Equal(50, SmelterRules.ResolveCount(50, 25));
    }

    [Theory]
    [InlineData("piece_smelter", true)]
    [InlineData("piece_blastfurnace", true)]
    [InlineData("piece_charcoalkiln", true)]
    [InlineData("piece_windmill", false)]
    [InlineData("piece_eitrrefinery", false)]
    [InlineData("piece_spinningwheel", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void Only_three_prefabs_are_controlled(string prefabName, bool controlled)
    {
        Assert.Equal(controlled, SmelterRules.IsControlled(prefabName));
    }

    [Fact]
    public void Fuel_is_written_only_for_smelter_and_blast_furnace_with_a_fuel_item()
    {
        Assert.True(SmelterRules.WritesFuel(SmelterRules.SmelterPrefab, true));
        Assert.True(SmelterRules.WritesFuel(SmelterRules.BlastFurnacePrefab, true));
        Assert.False(SmelterRules.WritesFuel(SmelterRules.SmelterPrefab, false));
        Assert.False(SmelterRules.WritesFuel(SmelterRules.BlastFurnacePrefab, false));
        Assert.False(SmelterRules.WritesFuel(SmelterRules.CharcoalKilnPrefab, true));
        Assert.False(SmelterRules.WritesFuel(SmelterRules.CharcoalKilnPrefab, false));
        Assert.False(SmelterRules.WritesFuel("piece_windmill", true));
    }

    [Fact]
    public void Ore_is_full_when_the_queue_reaches_the_cap()
    {
        Assert.False(SmelterRules.OreIsFull(9, 10));
        Assert.True(SmelterRules.OreIsFull(10, 10));
        Assert.True(SmelterRules.OreIsFull(11, 10));
    }

    [Fact]
    public void Fuel_is_full_when_it_passes_max_minus_one()
    {
        Assert.False(SmelterRules.FuelIsFull(19f, 20));
        Assert.True(SmelterRules.FuelIsFull(19.1f, 20));
        Assert.True(SmelterRules.FuelIsFull(20f, 20));
    }

    [Theory]
    [InlineData("piece_smelter", "piece_smelter")]
    [InlineData("piece_smelter(Clone)", "piece_smelter")]
    [InlineData("piece_charcoalkiln (Clone)", "piece_charcoalkiln")]
    [InlineData(null, "")]
    [InlineData("", "")]
    public void Clone_suffix_is_removed_from_the_object_name(string objectName, string expected)
    {
        Assert.Equal(expected, SmelterRules.PrefabName(objectName));
    }
}
