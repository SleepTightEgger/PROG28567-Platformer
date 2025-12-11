using UnityEngine;

public class shockwaveSlash : MonoBehaviour
{
    public Rigidbody2D body;
    public SpriteRenderer sr;

    public Vector2 direction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // set this gameobject to be destroyed after 1 second
        Destroy(gameObject, 1);
    }

    // Update is called once per frame
    void Update()
    {
        // if the passed in direction variable is left
        if (direction == Vector2.left)
        {
            // flip the sprite to face the left
            sr.flipX = true;
        }
        // in all other cases
        else
        {
            // do not flip the sprite
            sr.flipX = false; ;
        }
        // create a small 1 by 1 boxcast centered on the gameobject
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, Vector2.one, 0, Vector2.zero, 1f);
        // if that boxcast collides with something's collider that does not ignore raycasts
        if (hit.collider != null)
        {
            // add a force to the rigidbody of the object it collided with in the direction it
            // is traveling and also upwards
            hit.rigidbody.AddForce((direction + Vector2.up), ForceMode2D.Impulse);
        }
    }
}
