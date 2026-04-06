using UnityEngine;
using System.Collections;
using ithappy.Animals_FREE;

public class AnimalInteractable : MonoBehaviour
{
    [Header("Quest Data")]
    public QuestInfo currentQuest;

    [Header("Run Away Settings")]
    public float runAwaySpeed = 1.0f;
    public float runAwayDuration = 3.0f;
    
    [Header("Emote Settings")]
    public GameObject angryEmotePrefab;
    public Vector3 emoteOffset = new Vector3(0f, 1.5f, 0f);

    private GameObject activeFoodBowl;

    public void SetFoodBowlReference(GameObject bowl) { activeFoodBowl = bowl; }
    
    public void ReportMissionOutcome(bool success)
    {
        HandleBowlDissolve();

        if (success) {
            if (RescueController.Instance != null) RescueController.Instance.ReportMissionOutcome(true);
            Destroy(gameObject);
        } else {
            if (RescueController.Instance != null) RescueController.Instance.ReportMissionOutcome(false);
            StartCoroutine(RunAwayAndVanish());
        }
    }

    private void HandleBowlDissolve()
    {
        if (activeFoodBowl != null)
        {
            DebrisItem bowlDissolve = activeFoodBowl.GetComponent<DebrisItem>() ?? activeFoodBowl.AddComponent<DebrisItem>();
            bowlDissolve.StartDissolve();
            Destroy(activeFoodBowl, 2.0f);
        }
    }

    IEnumerator RunAwayAndVanish()
    {
        GameObject currentEmote = null;
        if (angryEmotePrefab != null) {
            currentEmote = Instantiate(angryEmotePrefab, transform.position + emoteOffset, Quaternion.identity);
            currentEmote.transform.SetParent(this.transform);
        }

        CreatureMover mover = GetComponent<CreatureMover>();
        if (mover != null) {
            Vector3 runDirection = (transform.position - PlayerMovement.Instance.transform.position).normalized;
            Vector3 target = transform.position + (runDirection * 15f);
            
            float t = 0;
            while (t < runAwayDuration) {
                mover.SetInput(new Vector2(0, 1 * runAwaySpeed), target, true, false);
                t += Time.deltaTime;
                yield return null;
            }
        }

        DebrisItem animalDissolve = GetComponent<DebrisItem>() ?? gameObject.AddComponent<DebrisItem>();
        animalDissolve.StartDissolve();

        yield return new WaitForSeconds(1.5f);
        if (currentEmote != null) Destroy(currentEmote);
        Destroy(gameObject);
    }
}