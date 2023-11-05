using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentMenu : MonoBehaviour
{
    private Animator animator;

    private bool flag = false;

    [SerializeField] private TMP_Text label;
    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.TryGetComponent<Animator>(out animator))
        {

        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleEquipmentMenu()
    {
        flag = !flag;

        if (flag == true)
        {
            animator.SetTrigger("Show");
            label.text = "HIDE EQUIPMENTS";
            Debug.Log("Toggled equipment menu");

        }
        else
        {
            animator.SetTrigger("Hide");
            label.text = "SHOW EQUIPMENTS";

            Debug.Log("Untoggled equipment menu");


        }
    }
}