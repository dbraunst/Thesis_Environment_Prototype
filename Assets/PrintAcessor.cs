using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PrintAcessor : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject _writer = null;
    void Start()
    {
        // StreamWriter localWriter 
        _writer = GameObject.FindGameObjectWithTag("PrinterManager");

        if (_writer != null) {
            Debug.Log("Writer Assigned");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U)){
            _writer.GetComponent<PrintManager>().AddLine("This is a string to add to output");
        }
    }
}
