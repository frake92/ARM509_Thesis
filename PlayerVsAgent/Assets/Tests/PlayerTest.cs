using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DummyAgent : MonoBehaviour
{
    public static DummyAgent Instance;
    public bool Ended = false;
    void Awake() => Instance = this;
    public void EndEpisode() => Ended = true;
}

public class DummyFlash : SimpleFlash
{
    public override void Flash() { }
}

public class DummyPlayerHP : PlayerHP
{
    public override IEnumerator startGameOver() { yield break; }
}


public class PlayerTest
{
    private GameObject playerObject;
    private PlayerHP playerHP;
    private Slider hpBar;

    [SetUp]
    public void Setup()
    {
        playerObject = new GameObject("Player");
        playerHP = playerObject.AddComponent<DummyPlayerHP>();

        hpBar = playerObject.AddComponent<Slider>();
        playerHP.hpBar = hpBar;
        playerHP.maxHP = 150;
        playerHP.currentHP = 150;


        playerHP.simpleFlash = playerObject.AddComponent<DummyFlash>();
        PlayerHP.Instance = playerHP;

        var agentObj = new GameObject("Agent");
        agentObj.AddComponent<DummyAgent>();
    }

    [Test]
    public void TakeDamage_ReducesCurrentHP()
    {
        int initialHP = playerHP.currentHP;
        int damage = 10;

        playerHP.TakeDamage(damage);

        Assert.AreEqual(initialHP - damage, playerHP.currentHP);
        Assert.AreEqual((initialHP - damage) / (float)playerHP.maxHP, hpBar.value);
    }

    [Test]
    public void TakeDamage_DoesNotReduceHP_WhenCanTakeDamageIsFalse()
    {
        playerHP.canTakeDamage = false;
        int initialHP = playerHP.currentHP;

        playerHP.TakeDamage(10);

        Assert.AreEqual(initialHP, playerHP.currentHP);
    }

    [Test]
    public void TakeDamage_SetsCurrentHPToZero_WhenDamageExceedsHP()
    {
        playerHP.currentHP = 5;
        playerHP.TakeDamage(10);

        Assert.AreEqual(0, playerHP.currentHP);
        Assert.AreEqual(0, hpBar.value);
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(playerObject);
        var agent = GameObject.Find("Agent");
        if (agent != null) Object.DestroyImmediate(agent);
    }
}