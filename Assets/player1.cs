using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player1 : MonoBehaviour
{
    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.GetComponent<Rigidbody2D>().velocity = (new Vector3(Input.GetAxis("HorizontalWASD"), Input.GetAxis("VerticalWASD")) * speed);
    }
}
