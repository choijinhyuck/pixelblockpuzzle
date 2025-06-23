using System.Collections;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private Coroutine shakeRoutine = null;
    private Vector3 origin; // 원래 위치 저장

    void Start()
    {
        origin = transform.position;
    }

    void Update()
    {
    }

    // 이미 실행 중인 흔들림 코루틴이 있으면 중단하고 새롭게 시작
    public void ShakeCamera(float duration = 0.2f, float shakeAmount = 0.1f, float frequency = 20f)
    {
        if(shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            transform.position = origin; // 초기 위치로 복귀
        }
        origin = transform.position;  // 새롭게 시작하기 전에 현재 위치를 origin으로 저장
        shakeRoutine = StartCoroutine(ShakeCameraCoroutine(duration, shakeAmount, frequency));
    }

    // 좌우로 아주 살짝 흔들리는 코루틴
    public IEnumerator ShakeCameraCoroutine(float duration = 0.2f, float shakeAmount = 0.01f, float frequency = 20f)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float offsetX = Mathf.Sin(elapsed * frequency * Mathf.PI * 2) * shakeAmount;
            transform.position = new Vector3(origin.x + offsetX, origin.y, origin.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = origin;
        shakeRoutine = null;
    }
}