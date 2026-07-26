using UnityEngine;

public class PlayerElementalManager : MonoBehaviour
{
    public enum ElementType
    {
        None,
        Poison,
        Dark,
        Lightning
    }

    [Header("Current Selected Element")]
    [SerializeField] private ElementType activeElement = ElementType.None;

    [Header("Elemental Settings")]
    [SerializeField] private bool isSecondBossDefeated = false;

    [Header("VFX Animator Controllers")]
    [SerializeField] private RuntimeAnimatorController poisonController;
    [SerializeField] private RuntimeAnimatorController darkController;
    [SerializeField] private RuntimeAnimatorController lightningController;

    private static PlayerElementalManager instance;
    public static PlayerElementalManager Instance => instance;

    public ElementType ActiveElement
    {
        get => activeElement;
        set => activeElement = value;
    }

    public bool IsSecondBossDefeated
    {
        get => isSecondBossDefeated;
        set => isSecondBossDefeated = value;
    }

    public float ActivationChance => isSecondBossDefeated ? 0.30f : 0.15f;
    public float TickDamage => isSecondBossDefeated ? 20f : 10f;

    public RuntimeAnimatorController PoisonController => poisonController;
    public RuntimeAnimatorController DarkController => darkController;
    public RuntimeAnimatorController LightningController => lightningController;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }
    }

    /// <summary>
    /// Try to apply elemental status to a hit target based on current activation chance.
    /// </summary>
    public void TryApplyElemental(GameObject target, bool isDirectHit = true)
    {
        if (activeElement == ElementType.None || target == null) return;

        // Only do chance roll on direct player attacks (spread spreads with 100% chance to radius)
        if (isDirectHit)
        {
            float roll = Random.value;
            if (roll > ActivationChance)
            {
                return;
            }
        }

        // Apply status effect
        ElementalEffect existingEffect = target.GetComponent<ElementalEffect>();
        if (existingEffect != null)
        {
            // Refresh duration
            existingEffect.RefreshDuration();
        }
        else
        {
            // Add status
            ElementalEffect newEffect = target.AddComponent<ElementalEffect>();
            newEffect.Configure(activeElement, TickDamage, 5f, isDirectHit); // lasts 5s
        }
    }
}
