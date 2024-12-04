using UnityEngine;
using System.Collections;

public class BreakableObstacle : MonoBehaviour
{
    [SerializeField] private float vibrationDuration = 0.5f;

    [SerializeField] private float vibrationIntensity = 0.1f;

    private Vector3 originalPosition;

    private void Start()
    {

        originalPosition = transform.localPosition;
    }
    //An attempt go give it an animation 
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            StartCoroutine(VibrateAndBreak());
        }
    }

    private IEnumerator VibrateAndBreak()
    {
        float elapsedTime = 0f;

        while (elapsedTime < vibrationDuration)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-vibrationIntensity, vibrationIntensity),
                Random.Range(-vibrationIntensity, vibrationIntensity),
                0f
            );

            transform.localPosition = originalPosition + randomOffset;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;

        // Disable object so it's invisible and has no collision
        gameObject.SetActive(false);
    }

    public void ResetBreakableObject()
    {
        // Bring it back
        gameObject.SetActive(true);
    }
}
