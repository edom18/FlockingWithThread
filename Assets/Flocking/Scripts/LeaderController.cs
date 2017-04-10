using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderController : MonoBehaviour
{
    [SerializeField]
    private float _speed = 0.3f;

	void Update()
    {
        bool isShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.position -= transform.right * _speed;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.position += transform.right * _speed;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            if (isShift)
            {
                transform.position += transform.up * _speed;
            }
            else
            {
                transform.position += transform.forward * _speed;
            }
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            if (isShift)
            {
                transform.position -= transform.up * _speed;
            }
            else
            {
                transform.position -= transform.forward * _speed;
            }
        }
	}
}
