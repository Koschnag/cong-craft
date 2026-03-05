using CongCraft.Engine.Combat;
using CongCraft.Engine.ECS;

namespace CongCraft.Engine.Tests.Combat;

public class EnemySpawnerTests
{
    [Theory]
    [InlineData(EnemyType.Wolf, 35f, 6f)]
    [InlineData(EnemyType.Bandit, 60f, 10f)]
    [InlineData(EnemyType.Skeleton, 80f, 15f)]
    [InlineData(EnemyType.Troll, 200f, 25f)]
    public void GetEnemyStats_ReturnsCorrectHpAndDamage(EnemyType type, float expectedHp, float expectedDamage)
    {
        var (hp, damage, _, _, _, _, _, _) = EnemySpawner.GetEnemyStats(type);
        Assert.Equal(expectedHp, hp);
        Assert.Equal(expectedDamage, damage);
    }

    [Fact]
    public void GetEnemyStats_WolfIsFastest()
    {
        var (_, _, _, _, wolfChase, _, _, _) = EnemySpawner.GetEnemyStats(EnemyType.Wolf);
        var (_, _, _, _, banditChase, _, _, _) = EnemySpawner.GetEnemyStats(EnemyType.Bandit);
        var (_, _, _, _, skeletonChase, _, _, _) = EnemySpawner.GetEnemyStats(EnemyType.Skeleton);
        var (_, _, _, _, trollChase, _, _, _) = EnemySpawner.GetEnemyStats(EnemyType.Troll);

        Assert.True(wolfChase > banditChase);
        Assert.True(banditChase > skeletonChase);
        Assert.True(skeletonChase > trollChase);
    }

    [Fact]
    public void GetEnemyStats_TrollHasHighestHp()
    {
        var (trollHp, _, _, _, _, _, _, _) = EnemySpawner.GetEnemyStats(EnemyType.Troll);
        var (wolfHp, _, _, _, _, _, _, _) = EnemySpawner.GetEnemyStats(EnemyType.Wolf);
        var (banditHp, _, _, _, _, _, _, _) = EnemySpawner.GetEnemyStats(EnemyType.Bandit);
        var (skeletonHp, _, _, _, _, _, _, _) = EnemySpawner.GetEnemyStats(EnemyType.Skeleton);

        Assert.True(trollHp > skeletonHp);
        Assert.True(skeletonHp > banditHp);
        Assert.True(banditHp > wolfHp);
    }

    [Fact]
    public void GetEnemyStats_AllTypesHavePositiveStats()
    {
        foreach (var type in Enum.GetValues<EnemyType>())
        {
            var (hp, damage, range, moveSpeed, chaseSpeed, detectRange, attackCooldown, patrolRadius) =
                EnemySpawner.GetEnemyStats(type);

            Assert.True(hp > 0, $"{type} should have positive HP");
            Assert.True(damage > 0, $"{type} should have positive damage");
            Assert.True(range > 0, $"{type} should have positive range");
            Assert.True(moveSpeed > 0, $"{type} should have positive move speed");
            Assert.True(chaseSpeed > 0, $"{type} should have positive chase speed");
            Assert.True(detectRange > 0, $"{type} should have positive detect range");
            Assert.True(attackCooldown > 0, $"{type} should have positive attack cooldown");
            Assert.True(patrolRadius > 0, $"{type} should have positive patrol radius");
        }
    }
}
