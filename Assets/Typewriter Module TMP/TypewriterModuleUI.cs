using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TypewriterModuleUI : MonoBehaviour {
    private TextMeshProUGUI text;
    [SerializeField] private float secondsBetweenCharacters = 0.05f;
	[SerializeField] private bool autoStart = false;
	[SerializeField] private float autoStartDelay = 0;

	private Coroutine currentCoroutine = null;

    private void Awake() {
        text = GetComponent<TextMeshProUGUI>();
    }

	private void OnEnable()
	{
		if (autoStart)
		{
			if (autoStartDelay == 0)
				Reveal();
			else
				StartCoroutine(RevealWithDelay());
		}
	}

	private IEnumerator RevealWithDelay()
	{
		text.maxVisibleCharacters = 0;
		yield return new WaitForSeconds(autoStartDelay);
		Reveal();
	}

	public bool Reveal() => Reveal(text.text.Length);
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
        }

        currentCoroutine = null;
        yield break;
    }
}
