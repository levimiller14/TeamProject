using System.Collections;
using UnityEngine;

public class GunIdleAnimations : MonoBehaviour
{
    [Header("----- Idle Detection -----")]
    [SerializeField] float idleThreshold = 15f;
    [SerializeField] float animationInterval = 5f;

    [Header("----- Animation Settings -----")]
    [SerializeField] float returnDuration = 0.25f;
    [SerializeField] float spinSpeed = 360f;
    [SerializeField] float flipDuration = 0.6f;
    [SerializeField] float tossHeight = 0.15f;
    [SerializeField] float tossDuration = 0.8f;
    [SerializeField] float twiddleDuration = 1.2f;
    [SerializeField] float inspectDuration = 2f;
    [SerializeField] float juggleDuration = 1.4f;
    [SerializeField] float flourishDuration = 1.8f;

    float timeSinceLastShot;
    float timeSinceLastAnimation;
    bool isIdle;
    bool isAnimating;

    Quaternion originalLocalRot;
    Vector3 originalLocalPos;
    Coroutine currentAnimation;

    void Start()
    {
        originalLocalRot = transform.localRotation;
        originalLocalPos = transform.localPosition;
    }

    void Update()
    {
        if (gameManager.instance != null && gameManager.instance.isPaused)
            return;

        if (Input.GetButton("Fire1"))
        {
            OnShoot();
            return;
        }

        timeSinceLastShot += Time.deltaTime;

        bool wasIdle = isIdle;
        isIdle = timeSinceLastShot >= idleThreshold;

        if (isIdle && !isAnimating)
        {
            timeSinceLastAnimation += Time.deltaTime;

            if (!wasIdle || timeSinceLastAnimation >= animationInterval)
            {
                PlayRandomAnimation();
                timeSinceLastAnimation = 0f;
            }
        }
    }

    public void OnShoot()
    {
        timeSinceLastShot = 0f;
        timeSinceLastAnimation = 0f;
        isIdle = false;

        if (isAnimating)
        {
            CancelAnimation();
        }
    }

    void CancelAnimation()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }
        isAnimating = false;
        transform.localRotation = originalLocalRot;
        transform.localPosition = originalLocalPos;
    }

    void PlayRandomAnimation()
    {
        if (isAnimating) return;

        int choice = Random.Range(0, 12);
        switch (choice)
        {
            case 0:
                currentAnimation = StartCoroutine(SpinAnimation());
                break;
            case 1:
                currentAnimation = StartCoroutine(FlipAnimation());
                break;
            case 2:
                currentAnimation = StartCoroutine(TossAndCatchAnimation());
                break;
            case 3:
                currentAnimation = StartCoroutine(TwiddleAnimation());
                break;
            case 4:
                currentAnimation = StartCoroutine(FingerSpinAnimation());
                break;
            case 5:
                currentAnimation = StartCoroutine(InspectAnimation());
                break;
            case 6:
                currentAnimation = StartCoroutine(JuggleAnimation());
                break;
            case 7:
                currentAnimation = StartCoroutine(BlowBarrelAnimation());
                break;
            case 8:
                currentAnimation = StartCoroutine(QuickDrawPracticeAnimation());
                break;
            case 9:
                currentAnimation = StartCoroutine(BalanceAnimation());
                break;
            case 10:
                currentAnimation = StartCoroutine(BoredDrumAnimation());
                break;
            case 11:
                currentAnimation = StartCoroutine(FlourishAnimation());
                break;
        }
    }

    IEnumerator SmoothReturn()
    {
        float elapsed = 0f;
        Quaternion startRot = transform.localRotation;
        Vector3 startPos = transform.localPosition;

        while (elapsed < returnDuration)
        {
            if (!isIdle) break;

            elapsed += Time.deltaTime;
            float t = elapsed / returnDuration;
            float smooth = t * t * (3f - 2f * t);

            transform.localRotation = Quaternion.Slerp(startRot, originalLocalRot, smooth);
            transform.localPosition = Vector3.Lerp(startPos, originalLocalPos, smooth);
            yield return null;
        }

        transform.localRotation = originalLocalRot;
        transform.localPosition = originalLocalPos;
    }

    void EndAnimation()
    {
        isAnimating = false;
        currentAnimation = null;
    }

    IEnumerator SpinAnimation()
    {
        isAnimating = true;
        float elapsed = 0f;
        float duration = 360f / spinSpeed;

        while (elapsed < duration)
        {
            if (!isIdle) break;

            elapsed += Time.deltaTime;
            float angle = elapsed * spinSpeed;
            transform.localRotation = originalLocalRot * Quaternion.Euler(0, angle, 0);
            yield return null;
        }

        yield return SmoothReturn();
        EndAnimation();
    }

    IEnumerator FlipAnimation()
    {
        isAnimating = true;
        float elapsed = 0f;

        while (elapsed < flipDuration)
        {
            if (!isIdle) break;

            elapsed += Time.deltaTime;
            float t = elapsed / flipDuration;
            float eased = Mathf.Sin(t * Mathf.PI);
            float angle = t * 360f;

            transform.localRotation = originalLocalRot * Quaternion.Euler(angle, 0, 0);
            transform.localPosition = originalLocalPos + Vector3.up * (eased * tossHeight * 0.5f);
            yield return null;
        }

        yield return SmoothReturn();
        EndAnimation();
    }

    IEnumerator TossAndCatchAnimation()
    {
        isAnimating = true;
        float elapsed = 0f;
        float spinAngle = 0f;
        float randomSpin = Random.Range(540f, 720f);

        while (elapsed < tossDuration)
        {
            if (!isIdle) break;

            elapsed += Time.deltaTime;
            float t = elapsed / tossDuration;

            float height = 4f * tossHeight * t * (1f - t);
            spinAngle = t * randomSpin;

            transform.localPosition = originalLocalPos + Vector3.up * height;
            transform.localRotation = originalLocalRot * Quaternion.Euler(spinAngle, spinAngle * 0.3f, 0);
            yield return null;
        }

        yield return SmoothReturn();
        EndAnimation();
    }

    IEnumerator TwiddleAnimation()
    {
        isAnimating = true;
        float elapsed = 0f;
        int direction = Random.Range(0, 2) == 0 ? 1 : -1;

        while (elapsed < twiddleDuration)
        {
            if (!isIdle) break;

            elapsed += Time.deltaTime;
            float t = elapsed / twiddleDuration;

            float wave = Mathf.Sin(t * Mathf.PI * 4f) * (1f - t);
            float zRot = wave * 25f * direction;
            float yOffset = Mathf.Abs(wave) * 0.02f;

            transform.localRotation = originalLocalRot * Quaternion.Euler(0, 0, zRot);
            transform.localPosition = originalLocalPos + new Vector3(wave * 0.03f * direction, yOffset, 0);
            yield return null;
        }

        yield return SmoothReturn();
        EndAnimation();
    }

    IEnumerator FingerSpinAnimation()
    {
        isAnimating = true;
        float elapsed = 0f;
        float spinDuration = 1.5f;
        float spins = 2f;

        while (elapsed < spinDuration)
        {
            if (!isIdle) break;

            elapsed += Time.deltaTime;
            float t = elapsed / spinDuration;

            float easeOut = 1f - Mathf.Pow(1f - t, 3f);
            float angle = easeOut * 360f * spins;

            float wobble = Mathf.Sin(t * Mathf.PI * 6f) * (1f - t) * 8f;

            transform.localRotation = originalLocalRot * Quaternion.Euler(wobble, 0, angle);
            yield return null;
        }

        yield return SmoothReturn();
        EndAnimation();
    }

    IEnumerator InspectAnimation()
    {
        isAnimating = true;
        float elapsed = 0f;

        while (elapsed < inspectDuration)
        {
            if (!isIdle) break;

            elapsed += Time.deltaTime;
            float t = elapsed / inspectDuration;

            float xRot = Mathf.Sin(t * Mathf.PI * 2f) * 20f;
            float yRot = Mathf.Sin(t * Mathf.PI) * 40f - 20f;
            float zRot = Mathf.Cos(t * Mathf.PI * 1.5f) * 15f;

            float pull = Mathf.Sin(t * Mathf.PI) * 0.08f;

            transform.localRotation = originalLocalRot * Quaternion.Euler(xRot, yRot, zRot);
            transform.localPosition = originalLocalPos + new Vector3(0, 0, -pull);
            yield return null;
        }

        yield return SmoothReturn();
        EndAnimation();
    }

    IEnumerator JuggleAnimation()
    {
        isAnimating = true;
        float elapsed = 0f;
        int tosses = 3;

        while (elapsed < juggleDuration)
        {
            if (!isIdle) break;

            elapsed += Time.deltaTime;
            float t = elapsed / juggleDuration;

            float tossPhase = (t * tosses) % 1f;
            float height = 4f * tossHeight * 0.7f * tossPhase * (1f - tossPhase);
            float xOffset = Mathf.Sin(t * Mathf.PI * tosses) * 0.12f;
            float spin = t * 360f * tosses;

            transform.localPosition = originalLocalPos + new Vector3(xOffset, height, 0);
            transform.localRotation = originalLocalRot * Quaternion.Euler(0, 0, spin);
            yield return null;
        }

        yield return SmoothReturn();
        EndAnimation();
    }

    IEnumerator BlowBarrelAnimation()
    {
        isAnimating = true;
        float duration = 1.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (!isIdle) break;

            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float tiltUp = Mathf.Sin(t * Mathf.PI) * -35f;
            float pullBack = Mathf.Sin(t * Mathf.PI) * 0.06f;
            float bob = Mathf.Sin(t * Mathf.PI * 3f) * 0.01f * (t < 0.7f ? 1f : 0f);

            transform.localRotation = originalLocalRot * Quaternion.Euler(tiltUp, 0, 0);
            transform.localPosition = originalLocalPos + new Vector3(0, bob, -pullBack);
            yield return null;
        }

        yield return SmoothReturn();
        EndAnimation();
    }

    IEnumerator QuickDrawPracticeAnimation()
    {
        isAnimating = true;
        float duration = 0.8f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (!isIdle) break;

            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float drop, snapUp;
            if (t < 0.4f)
            {
                drop = Mathf.Pow(t / 0.4f, 2f);
                snapUp = 0f;
            }
            else
            {
                drop = 1f;
                float snapT = (t - 0.4f) / 0.6f;
                snapUp = 1f - Mathf.Pow(1f - snapT, 3f);
            }

            float yOffset = -0.15f * drop + 0.15f * snapUp;
            float xRot = 30f * drop - 30f * snapUp;

            transform.localPosition = originalLocalPos + new Vector3(0, yOffset, 0);
            transform.localRotation = originalLocalRot * Quaternion.Euler(xRot, 0, 0);
            yield return null;
        }

        yield return SmoothReturn();
        EndAnimation();
    }

    IEnumerator BalanceAnimation()
    {
        isAnimating = true;
        float duration = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (!isIdle) break;

            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float wobbleX = Mathf.Sin(t * Mathf.PI * 8f) * 12f * (1f - t * 0.5f);
            float wobbleZ = Mathf.Cos(t * Mathf.PI * 6f) * 8f * (1f - t * 0.5f);
            float correctX = Mathf.Sin(t * Mathf.PI * 2f) * -wobbleX * 0.3f;

            transform.localRotation = originalLocalRot * Quaternion.Euler(wobbleX + correctX, 0, wobbleZ);
            yield return null;
        }

        yield return SmoothReturn();
        EndAnimation();
    }

    IEnumerator BoredDrumAnimation()
    {
        isAnimating = true;
        float duration = 1.6f;
        float elapsed = 0f;
        int taps = 6;

        while (elapsed < duration)
        {
            if (!isIdle) break;

            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float tapPhase = (t * taps) % 1f;
            float tap = tapPhase < 0.3f ? Mathf.Sin(tapPhase / 0.3f * Mathf.PI) : 0f;

            float xRot = tap * 8f;
            float yBounce = tap * 0.015f;

            transform.localRotation = originalLocalRot * Quaternion.Euler(xRot, 0, 0);
            transform.localPosition = originalLocalPos + new Vector3(0, -yBounce, 0);
            yield return null;
        }

        yield return SmoothReturn();
        EndAnimation();
    }

    IEnumerator FlourishAnimation()
    {
        isAnimating = true;
        float elapsed = 0f;

        while (elapsed < flourishDuration)
        {
            if (!isIdle) break;

            elapsed += Time.deltaTime;
            float t = elapsed / flourishDuration;

            float phase1 = Mathf.Clamp01(t / 0.3f);
            float phase2 = Mathf.Clamp01((t - 0.3f) / 0.4f);
            float phase3 = Mathf.Clamp01((t - 0.7f) / 0.3f);

            float yRot = Mathf.Sin(phase1 * Mathf.PI) * 180f;
            yRot += phase2 * 360f;
            float zRot = Mathf.Sin(phase2 * Mathf.PI) * 45f;
            float xRot = Mathf.Sin(phase3 * Mathf.PI * 2f) * 20f;

            float toss = Mathf.Sin(phase2 * Mathf.PI) * tossHeight * 0.8f;

            transform.localRotation = originalLocalRot * Quaternion.Euler(xRot, yRot, zRot);
            transform.localPosition = originalLocalPos + new Vector3(0, toss, 0);
            yield return null;
        }

        yield return SmoothReturn();
        EndAnimation();
    }
}
