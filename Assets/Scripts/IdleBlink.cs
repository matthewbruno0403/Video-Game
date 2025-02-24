using System.Collections;
using UnityEngine;

public class IdleBlink : MonoBehaviour
{
    Animator animator;
    [SerializeField] float minBlinkInterval = 3f;
    [SerializeField] float maxBlinkInterval = 10f;

    void Start()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(BlinkRoutine());
    }

    IEnumerator BlinkRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minBlinkInterval, maxBlinkInterval);
            yield return new WaitForSeconds(waitTime);
            animator.SetTrigger("blinkTrigger");
        }
    }
}
