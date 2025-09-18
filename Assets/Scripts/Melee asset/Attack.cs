using StarterAssets;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;

public class Attack : WeaponBase
{

    [SerializeField] WeaponData weaponData; // Reference til ScriptableObject
    [SerializeField] StarterAssetsInputs starterAssetsInputs;
    [SerializeField] CharacterController characterController;
    [SerializeField] Camera cam;
    [SerializeField] GameObject hitEffect;
    [SerializeField] LayerMask attackLayer;
    [SerializeField] Animator animator;

    public const string IDLE = "Idle";
    public const string WALK = "Walk";
    public const string ATTACK1 = "Attack 1";
    public const string ATTACK2 = "Attack 2";

    //private Animator animator;
    private bool attacking = false;
    private bool readyToAttack = true;
    private int attackCount;
    private string currentAnimationState;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (starterAssetsInputs.attack)
        {
            AttackMethod();
            starterAssetsInputs.attack = false;
        }

        SetAnimations();
    }
    public override void Use()
    {
        AttackMethod();
    }
    public void AttackMethod()
    {
        if (!readyToAttack || attacking) return;

        readyToAttack = false;
        attacking = true;

        Invoke(nameof(ResetAttack), weaponData.attackSpeed);
        Invoke(nameof(AttackRaycast), 0.4f);  // Angrebsdelay (kan også lægges i WeaponData)

        if (attackCount == 0)
        {
            ChangeAnimationState(ATTACK1);
            attackCount++;
        }
        else
        {
            ChangeAnimationState(ATTACK2);
            attackCount = 0;
        }
    }

    void AttackRaycast()
    {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, weaponData.range, attackLayer))
        {
            Debug.Log("Hit: " + hit.point);
            HitTarget(hit.point);
        }
    }

    void HitTarget(Vector3 pos)
    {
        GameObject GO = Instantiate(hitEffect, pos, Quaternion.identity);
        Destroy(GO, 20);
    }

    void ResetAttack()
    {
        attacking = false;
        readyToAttack = true;
        ChangeAnimationState(IDLE);
    }

    void SetAnimations()
    {
        if (!attacking)
        {
            if (characterController.velocity.x == 0 && characterController.velocity.z == 0)
                ChangeAnimationState(IDLE);
            else
                ChangeAnimationState(WALK);
        }
    }

    void ChangeAnimationState(string newState)
    {
        if (currentAnimationState == newState) return;
        currentAnimationState = newState;
        animator.CrossFadeInFixedTime(currentAnimationState, 0.2f);
    }
}
