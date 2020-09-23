using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{



    private void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.gameObject.layer != 8)
        {
            transform.parent.GetComponent<ButtonManager>().touchNum++;
        }
    }
    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.layer != 8)
        {
            transform.parent.GetComponent<ButtonManager>().touchNum--;
        }
    }
}
