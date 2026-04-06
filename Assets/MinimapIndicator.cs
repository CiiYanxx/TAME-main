using UnityEngine;

public class MinimapIndicator : MonoBehaviour
{
    private Transform player;
    public float minimapRadius = 10f; // Radius ng iyong minimap circle
    private Vector3 startScale;

    void Start()
    {
        // Hanapin ang player (siguraduhin na ang player mo ay may "Player" tag)
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        
        startScale = transform.localScale;
    }

    void LateUpdate()
    {
        if (player == null) return;

        // 1. Kunin ang direksyon at distansya mula sa player papunta sa animal
        Vector3 offset = transform.parent.position - player.position;
        offset.y = 0; // Balewalain ang height
        float distance = offset.magnitude;

        // 2. I-check kung ang aso ay nasa labas ng minimap radius
        if (distance > minimapRadius)
        {
            // I-clamp ang position sa gilid ng bilog
            Vector3 clampedPosition = player.position + (offset.normalized * minimapRadius);
            clampedPosition.y = transform.position.y; // Panatilihin ang height ng icon
            transform.position = clampedPosition;
            
            // Opsyonal: Paliitin nang kaunti ang icon kapag nasa gilid para alam mong malayo pa
            transform.localScale = startScale * 0.7f;
        }
        else
        {
            // Ibalik sa normal na position (follow the animal)
            transform.localPosition = new Vector3(0, transform.localPosition.y, 0);
            transform.localScale = startScale;
        }

        // 3. Panatilihing nakaharap sa North ang icon
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}