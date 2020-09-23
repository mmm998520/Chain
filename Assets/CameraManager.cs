using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Transform players;
    void Update()
    {
        transform.position = new Vector3(transform.position.x, players.GetChild(0).localPosition.y + players.GetChild(1).localPosition.y, transform.position.z);
    }
}
