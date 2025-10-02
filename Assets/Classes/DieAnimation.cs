using System.Collections;
using UnityEngine;

/// <summary>
/// Animation that plays when piece is killed. Destroys itself after 10 seconds.
/// </summary>
public class DieAnimation : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SelfDestruct());
    }

    IEnumerator SelfDestruct()
    {
        yield return new WaitForSeconds(10f);
        Destroy(gameObject);
    }
}
