using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InfoDisplay : MonoBehaviour
{
    [SerializeField] public GameObject capsule; 
    [SerializeField] public GameObject hexagon; 
    
    private TMP_Text capsuletxt;
    private Vector3 capsulePos;

    private TMP_Text hexatxt;
    private Vector3 hexaPos;
    // Start is called before the first frame update
    void Start()
    {
        capsuletxt = capsule.GetComponent<TMP_Text>();
        capsulePos = capsule.transform.position;

        hexatxt = hexagon.GetComponent<TMP_Text>();
        hexaPos = hexagon.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        capsuletxt.text = (capsuletxt != null) ? capsulePos.ToString() : "Cannot Get Capsule Pos";
        hexatxt.text = (hexatxt != null) ? hexaPos.ToString() : "Cannot Get hexa Pos";
    }
    
    
}
