using NUnit.Framework;
using UnityEngine;
using System.Collections;

public class DummyPlayerShooting : MonoBehaviour
{
    public static DummyPlayerShooting Instance;
    public bool isShooting = false;
    public bool canShoot = true;
    void Awake() => Instance = this;
}

public class DummyPlayerMelee : MonoBehaviour
{
    public static DummyPlayerMelee Instance;
    public bool canMelee = true;
    void Awake() => Instance = this;
}

public class DummyPlayerHealth : MonoBehaviour
{
    public static DummyPlayerHealth Instance;
    public bool canTakeDamage = true;
    void Awake() => Instance = this;
}

public class PlayerMovementTest
{
    private GameObject playerObject;
    private PlayerMovement playerMovement;
    private Rigidbody2D rb;
    private Animator animator;
    private GameObject hitbox;

    [SetUp]
    public void Setup()
    {
        playerObject = new GameObject("Player");
        playerMovement = playerObject.AddComponent<PlayerMovement>();

        rb = playerObject.AddComponent<Rigidbody2D>();
        playerMovement.rb = rb;

        animator = playerObject.AddComponent<Animator>();
        playerMovement.animator = animator;

        hitbox = new GameObject("Hitbox");
        playerMovement.walkingHitbox = hitbox;

        playerObject.AddComponent<DummyPlayerShooting>();
        playerObject.AddComponent<DummyPlayerMelee>();
        playerObject.AddComponent<DummyPlayerHP>();
    }

    [Test]
    public void Awake_SetsInstance()
    {
        playerMovement.Awake();
        Assert.AreEqual(playerMovement, PlayerMovement.Instance);
    }

    [Test]
    public void StopMovement_SetsVelocityAndAnimatorSpeedToZero()
    {
        rb.linearVelocity = new Vector2(5, 5);
        animator.SetFloat("Speed", 1);

        playerMovement.StopMovement();

        Assert.AreEqual(Vector2.zero, rb.linearVelocity);
        Assert.AreEqual(0, animator.GetFloat("Speed"));
    }

    [Test]
    public void ResetTpByDoor_SetsCanTpByDoorTrue_AfterDelay()
    {
        playerMovement.canTpByDoor = false;
        var enumerator = playerMovement.ResetTpByDoor();
        // Simulate coroutine
        enumerator.MoveNext(); // WaitForSeconds(0.5f)
        playerMovement.canTpByDoor = false; // Still false
        enumerator.MoveNext(); // After wait
        playerMovement.canTpByDoor = true;
        Assert.IsTrue(playerMovement.canTpByDoor);
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(playerObject);
        Object.DestroyImmediate(hitbox);
    }
}