using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoughPingController : MonoBehaviour
{
    public static CoughPingController instance;
    [SerializeField] List<GameObject> pingList;
    Coroutine pingCoroutine;
    Dictionary<SymptomEffect.KnockbackDirection, GameObject> pingDictionary;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
        {
            Destroy(instance.gameObject);
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        pingDictionary = new Dictionary<SymptomEffect.KnockbackDirection, GameObject>()
        {
            {SymptomEffect.KnockbackDirection.Left,pingList[0]},
            {SymptomEffect.KnockbackDirection.Right,pingList[1]},
            {SymptomEffect.KnockbackDirection.Top,pingList[2]},
            {SymptomEffect.KnockbackDirection.Bottom,pingList[3]}
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void ActivatePing(bool status, SymptomEffect.KnockbackDirection direction,float delay, float duration)
    {
       StartCoroutine(PingCoroutine(status, direction, delay, duration));
    }
    public IEnumerator PingCoroutine(bool status, SymptomEffect.KnockbackDirection direction,float delay, float duration)
    {
        yield return new WaitForSeconds(delay);

        AudioManager.instance.Play("Ping", transform.position);

        Debug.Log("Direction: " + direction);

        pingDictionary[direction].SetActive(true);

        yield return new WaitForSeconds(duration);

        DeactivateAllPing();


    }

    public void DeactivateAllPing()
    {
        // Traversing the dictionary
        foreach (KeyValuePair<SymptomEffect.KnockbackDirection, GameObject> entry in pingDictionary)
        {
            entry.Value.SetActive(false);
        }
    }
}
