using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    static int touchNum = 0;
    public GameObject door;

    void Update()
    {
        if (touchNum >= 3)
        {
            Destroy(gameObject);
            Destroy(door);
        }
    }
    private void OnTriggerEnter2D(Collider2D collider)
    {
        touchNum++;
    }
    private void OnTriggerExit2D(Collider2D collider)
    {
        touchNum--;
    }
}
