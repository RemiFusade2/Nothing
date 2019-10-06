using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum ItemType
{
    APPLE, // eat twice and change sprite
    ANVIL, // click as much as you want (sound + slightly more red)
    BUBBLE, // pop
    DUCK, // squeeze
    BALLOON, // pop
    BOUNCING_BALL, // throw down
    GLASS, // breaks after 3 clicks or when falling
    GLASS_PIECE,
    CUSHION, // soft, slow down fall


    BLACK_HOLE // swallow all
}

public class ClickableItemBehaviour : MonoBehaviour
{
    public ItemType item;

    public AudioSource audioSource;
    public SpriteRenderer spriteRenderer;
    public Collider2D col2D;
    public Rigidbody2D rb2D;

    [Header("Apple")]
    public List<Sprite> appleBits;
    private int currentIndex;
    public AudioClip appleCrunch;
    public AudioClip appleFall;

    [Header("Anvil")]
    public Color hotColor;
    private float currentHotValue;
    public AudioClip anvilHit;
    public AudioClip anvilFall;

    [Header("Bouncing Ball")]
    public AudioClip bouncingBallHit;
    public AudioClip bouncingBallFall;

    [Header("Bubble")]
    public AudioClip bubblePop;

    [Header("Duck")]
    public AudioClip duckHit;
    public AudioClip duckFall;

    [Header("Glass")]
    public AudioClip glassHit;
    public AudioClip glassBreaks;
    private int hitCount;
    public GameObject brokenGlassPrefab;

    // Start is called before the first frame update
    void Start()
    {
        if (item == ItemType.APPLE)
        {
            currentIndex = 0;
        }
        else if (item == ItemType.ANVIL)
        {
            currentHotValue = 0;
            spriteRenderer.color = Color.Lerp(Color.white, hotColor, currentHotValue);
            StartCoroutine(WaitAndCoolDown(0.05f));
        }
        else if (item == ItemType.GLASS)
        {
            hitCount = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HandleClick()
    {
        if (item == ItemType.APPLE && currentIndex < appleBits.Count - 1)
        {
            currentIndex++;
            spriteRenderer.sprite = appleBits[currentIndex];
            audioSource.PlayOneShot(appleCrunch, 1);
        }
        else if (item == ItemType.ANVIL)
        {
            currentHotValue += 0.01f;
            spriteRenderer.color = Color.Lerp(Color.white, hotColor, currentHotValue);
            audioSource.PlayOneShot(anvilHit, 1);
        }
        else if (item == ItemType.BOUNCING_BALL)
        {
            rb2D.AddForce(1000*Vector2.up);
            audioSource.PlayOneShot(bouncingBallHit, 1);
        }
        else if (item == ItemType.BUBBLE)
        {
            audioSource.PlayOneShot(bubblePop, 1);
            spriteRenderer.enabled = false;
            col2D.enabled = false;
        }
        else if (item == ItemType.DUCK)
        {
            rb2D.AddForce(200 * Vector2.up);
            audioSource.PlayOneShot(duckHit, 1);
        }
        else if (item == ItemType.GLASS)
        {
            hitCount++;
            if (hitCount > 5)
            {
                GlassBreaks();
            }
            else
            {
                audioSource.PlayOneShot(glassHit, 1);
            }
        }
        else if (item == ItemType.GLASS_PIECE)
        {
            audioSource.PlayOneShot(glassHit, 1);
        }
    }

    private void GlassBreaks()
    {
        spriteRenderer.enabled = false;
        col2D.enabled = false;
        audioSource.PlayOneShot(glassBreaks, 1);

        int piecesCount = Random.Range(5, 8);
        for (int i = 0; i < piecesCount; i++)
        {
            GameObject go = Instantiate(brokenGlassPrefab, this.transform.position + Random.Range(0, 1.0f) * Vector3.up + ((i % 2 == 0) ? -1 : 1) * 0.4f * i * Vector3.right, Quaternion.Euler(0, 0, Random.Range(-90, 90)), this.transform.parent);
            go.GetComponent<Rigidbody2D>().velocity = Random.Range(-0.3f, 0.3f) * Vector3.right + Random.Range(-0.1f, 0.1f) * Vector3.up;
            GameEngineBehaviour.instance.AddItem(go);
        }
    }

    public void OnCollisionEnter2D(Collision2D col)
    {
        float audioLevel = col.relativeVelocity.magnitude / 10.0f;
        audioLevel = (audioLevel > 0.8f) ? 0.8f : audioLevel;

        if (item == ItemType.APPLE)
        {
            audioSource.PlayOneShot(appleFall, audioLevel);
        }
        else if (item == ItemType.ANVIL)
        {
            audioSource.PlayOneShot(anvilFall, audioLevel);
        }
        else if (item == ItemType.BOUNCING_BALL && audioLevel > 0.5f)
        {
            this.transform.localScale = Vector3.one - Vector3.up * audioLevel * 0.1f + Vector3.right * audioLevel * 0.05f;
            StartCoroutine(WaitAndRestoreScale(0.1f));
            audioSource.PlayOneShot(bouncingBallFall, audioLevel * 0.4f);
        }
        else if (item == ItemType.BUBBLE && audioLevel > 0.1f)
        {
            audioSource.PlayOneShot(bubblePop, 1);
            spriteRenderer.enabled = false;
            col2D.enabled = false;
        }
        else if (item == ItemType.DUCK && audioLevel > 0.35f)
        {
            audioSource.PlayOneShot(duckFall, audioLevel);
        }
        else if (item == ItemType.GLASS && (audioLevel > 0.55f || (col.rigidbody != null && col.rigidbody.mass >= 9) ))
        {
            if (col.relativeVelocity.magnitude > 15 || (col.rigidbody != null && col.rigidbody.mass >= 9) )
            {
                GlassBreaks();
            }
            else
            {
                hitCount++;
                if (hitCount > 5)
                {
                    GlassBreaks();
                }
                else
                {
                    audioSource.PlayOneShot(glassHit, audioLevel);
                }
            }
        }
        else if (item == ItemType.GLASS_PIECE && audioLevel > 0.5f)
        {
            audioSource.PlayOneShot(glassHit, audioLevel);
        }
    }

    private IEnumerator WaitAndRestoreScale(float delay)
    {
        yield return new WaitForSeconds(delay);
        this.transform.localScale = Vector3.one;
    }

    private IEnumerator WaitAndCoolDown(float delay)
    {
        yield return new WaitForSeconds(delay);
        currentHotValue -= 0.01f;
        currentHotValue = (currentHotValue < 0) ? 0 : currentHotValue;
        spriteRenderer.color = Color.Lerp(Color.white, hotColor, currentHotValue);
        StartCoroutine(WaitAndCoolDown(0.15f));
    }

    private float blackHoleForce;

    public void VacuumTowards(Vector3 blackHoleposition)
    {
        blackHoleForce = 0;
        StartCoroutine(WaitAndVacuum(Random.Range(0.5f,4.8f), blackHoleposition));
    }

    private IEnumerator WaitAndVacuum (float delay, Vector3 blackHoleposition)
    {
        yield return new WaitForSeconds(delay);

        rb2D.mass = 1;

        float distanceWithBlackHole = Vector3.Distance(blackHoleposition, this.transform.position);
        this.transform.localScale = Vector3.one * (distanceWithBlackHole > 5 ? 1 : (distanceWithBlackHole / 5));

        Vector3 blackHoleForce3d = blackHoleposition - this.transform.position;
        Vector2 blackHoleForce2d = new Vector2(blackHoleForce3d.x, blackHoleForce3d.y);

        blackHoleForce += 10;

        rb2D.AddForce(blackHoleForce2d * blackHoleForce / (distanceWithBlackHole > 10 ? 10 : distanceWithBlackHole));

        if (distanceWithBlackHole > 2.0f)
        {
            StartCoroutine(WaitAndVacuum(0.1f, blackHoleposition));
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
