using UnityEngine;

public class PlatformMovement : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float moveDistance = 3f;
    public Vector3 startPositon;
    private bool movingUp = true;

    // 플레이어 원래 부모 저장
    private Transform originalParent; 

    void Start()
    {
        startPositon = transform.position;
    }

    void Update()
    {
        float distanceCovered = (transform.position - startPositon).y;

        if (movingUp)
        {
            transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
            if (distanceCovered >= moveDistance)
            {
                movingUp = false;
            }
        }
        else
        {
            transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
            if (distanceCovered <= 0)
            {
                movingUp = true;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            originalParent = collision.transform.parent;
            collision.gameObject.transform.SetParent(transform);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.transform.SetParent(originalParent ?? null);
        }
    }
}
