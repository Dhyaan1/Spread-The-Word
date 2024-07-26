using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Homingmissile : MonoBehaviour
{
    private Transform target;
    public float speed = 5f;
    public float rotateSpeed = 200f;

    private Rigidbody2D rb;

    // Awake is called before Start
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component not found");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("No GameObject named 'Player' found");
            return;
        }
        target = player.transform;
    }

    void FixedUpdate () 
{
    GameObject player = GameObject.FindGameObjectWithTag("Player");
    if (player == null)
    {
        Debug.LogError("No GameObject tagged 'Player' found");
        return;
    }
    target = player.transform;

    if (target == null)
    {
        Debug.LogError("Target is null");
        return;
    }   

    Vector2 direction = (Vector2)target.position - rb.position;

    direction.Normalize();

    float rotateAmount = Vector3.Cross(direction, transform.up).z;

    rb.angularVelocity = -rotateAmount * rotateSpeed;

    rb.velocity = transform.up * speed;
}
	void OnTriggerEnter2D ()
	{
		// Put a particle effect here
		Destroy(gameObject);
	}
}
