using UnityEngine;

public class shockwaveSlash : MonoBehaviour
{
    public Rigidbody2D body;
    public SpriteRenderer sr;

    public Vector2 direction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (direction == Vector2.left)
        {
            sr.flipX = true;
        }
        else
        {
            sr.flipX = false; ;
        }
        ApplyForce();
    }

    public void ApplyForce()
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, Vector2.one, 0, Vector2.zero, 1f);
        if (hit.collider != null)
        {
            hit.rigidbody.AddForce((direction + Vector2.up), ForceMode2D.Impulse);
        }
    }
}
