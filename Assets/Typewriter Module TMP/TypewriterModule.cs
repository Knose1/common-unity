using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
public class TypewriterModule : MonoBehaviour {
    private TextMeshPro text;
    [SerializeField] private float secondsBetweenCharacters = 0.05f;

    private Coroutine currentCoroutine = null;

    // Start is called before the first frame update
    void Start() {
        text = GetComponent<TextMeshPro>();
    }

    public bool Reveal(int totalLength) {

        if (currentCoroutine != null) {
            StopCoroutine(currentCoroutine);
            text.maxVisibleCharacters = text.textInfo.characterCount;
            currentCoroutine = null;
            return false;
        }

        currentCoroutine = StartCoroutine(TextReveal(totalLength));
        return true;
    }

    private IEnumerator TextReveal(int totalLength) {
        int count = 0;
        text.maxVisibleCharacters = 0;

        while (count < totalLength) {
            count++;
            text.maxVisibleCharacters = count;

            yield return new WaitForSeconds(secondsBetweenCharacters);

            Debug.Log(count + " : " + totalLength);
        }

        currentCoroutine = null;
        yield break;
    }
}
