using UnityEngine;

public class movement : MonoBehaviour
{
    [SerializeField] private float speed;
    
private Rigidbody2D rb;

private void Awake()
{
    rb = GetComponent<Rigidbody2D>();

}



private void Update()
{
    rb.velocity = new Vector2(Input.GetAxis("Horizontal") * speed, rb.velocity.y);


    if(Input.GetKey(KeyCode.Space))
        rb.velocity = new Vector2(rb.velocity.x, speed);
    

}


}