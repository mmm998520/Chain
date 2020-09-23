using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    public int touchNum = 0;
    public GameObject door;

    void Update()
    {
            if (touchNum >= transform.childCount)
            {
                Destroy(gameObject);
                Destroy(door);
            }
    }
}
