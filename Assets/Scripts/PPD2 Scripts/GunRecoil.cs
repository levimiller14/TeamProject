using UnityEngine;

public class GunRecoil : MonoBehaviour
{
    [Header("----- Recoil Settings -----")]
    [SerializeField] float kickbackAmount = 0.08f;
    [SerializeField] float kickUpAmount = 0.02f;
    [SerializeField] float rotationKick = 8f;
    [SerializeField] float sideKickRange = 2f;

    [Header("----- Recovery -----")]
    [SerializeField] float positionRecoverySpeed = 15f;
    [SerializeField] float rotationRecoverySpeed = 20f;

    [Header("----- Snappiness -----")]
    [SerializeField] float recoilSnappiness = 25f;

    Vector3 currentRecoilPosition;
    Vector3 targetRecoilPosition;
    Vector3 rotationRecoil;
    Vector3 targetRotationRecoil;

    Vector3 originalLocalPos;
    Quaternion originalLocalRot;

    GunIdleAnimations idleAnimations;

    void Start()
    {
        originalLocalPos = transform.localPosition;
        originalLocalRot = transform.localRotation;
        idleAnimations = GetComponent<GunIdleAnimations>();
    }

    void Update()
    {
        if (gameManager.instance != null && gameManager.instance.isPaused)
            return;

        // interpolate towards target recoil
        currentRecoilPosition = Vector3.Lerp(currentRecoilPosition, targetRecoilPosition, recoilSnappiness * Time.deltaTime);
        rotationRecoil = Vector3.Lerp(rotationRecoil, targetRotationRecoil, recoilSnappiness * Time.deltaTime);

        // decay target back to zero
        targetRecoilPosition = Vector3.Lerp(targetRecoilPosition, Vector3.zero, positionRecoverySpeed * Time.deltaTime);
        targetRotationRecoil = Vector3.Lerp(targetRotationRecoil, Vector3.zero, rotationRecoverySpeed * Time.deltaTime);

        // apply recoil offset
        transform.localPosition = originalLocalPos + currentRecoilPosition;
        transform.localRotation = originalLocalRot * Quaternion.Euler(rotationRecoil);
    }

    public void TriggerRecoil()
    {
        // notify idle animations that we shot (cancels any playing idle animation)
        if (idleAnimations != null)
        {
            idleAnimations.OnShoot();
        }

        // position kick: back and up
        targetRecoilPosition += new Vector3(0f, kickUpAmount, -kickbackAmount);

        // rotation kick: pitch up with slight random horizontal variation
        float sideKick = Random.Range(-sideKickRange, sideKickRange);
        targetRotationRecoil += new Vector3(-rotationKick, sideKick, sideKick * 0.5f);
    }

    public void TriggerRecoil(float multiplier)
    {
        if (idleAnimations != null)
        {
            idleAnimations.OnShoot();
        }

        targetRecoilPosition += new Vector3(0f, kickUpAmount * multiplier, -kickbackAmount * multiplier);

        float sideKick = Random.Range(-sideKickRange, sideKickRange) * multiplier;
        targetRotationRecoil += new Vector3(-rotationKick * multiplier, sideKick, sideKick * 0.5f);
    }

    // call this when switching guns to sync original transform
    public void ResetOrigin()
    {
        originalLocalPos = transform.localPosition;
        originalLocalRot = transform.localRotation;
        currentRecoilPosition = Vector3.zero;
        targetRecoilPosition = Vector3.zero;
        rotationRecoil = Vector3.zero;
        targetRotationRecoil = Vector3.zero;
    }
}
