using System.Collections;
using UnityEngine;

public class FallingObject : MonoBehaviour
{
    #region 내부 변수 
    public float fallSpeed = 10f;
    private Rigidbody2D rb;
    public AudioSource audioSource;
    public AudioClip DestorySound;
    #endregion

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector3.down * fallSpeed;
        // 운석 생성시 효과음
        PlayObjSound();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Ground"))
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                collision.gameObject.GetComponent<PlayerState>().TakeDamage(10);
            }
            Destroy(gameObject);
            
        }
    }

    public void PlayObjSound()
    {
        audioSource.clip = DestorySound;
        audioSource.Play();
    }
}
