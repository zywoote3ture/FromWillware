using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadVibration : MonoBehaviour
{
    public static GamepadVibration Instance;

    private Coroutine vibrationCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Vibrate(float lowFrequency, float highFrequency, float duration)
    {
        Gamepad pad = Gamepad.current;

        if (pad == null)
        {
            Debug.LogWarning("没有检测到手柄");
            return;
        }

        if (vibrationCoroutine != null)
        {
            StopCoroutine(vibrationCoroutine);

            pad.SetMotorSpeeds(0, 0);
        }

        vibrationCoroutine = StartCoroutine(
            VibrateCoroutine(pad, lowFrequency, highFrequency, duration)
        );
    }

    IEnumerator VibrateCoroutine(Gamepad pad,float low, float high, float duration)
    {
        pad.SetMotorSpeeds(low, high);

        yield return new WaitForSeconds(duration);

        pad.SetMotorSpeeds(0, 0);

        vibrationCoroutine = null;
    }

    private void StopVibration()
    {
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(0, 0);
        }
    }

    private void OnDisable()
    {
        StopVibration();
    }

    private void OnApplicationQuit()
    {
        StopVibration();
    }
}