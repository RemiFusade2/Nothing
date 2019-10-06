using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnableItem
{
    public int count;
    public GameObject prefab;
    public float spawnDelay;
}

public class GameEngineBehaviour : MonoBehaviour
{
    public static GameEngineBehaviour instance;

    public List<SpawnableItem> spawnableItemsList;
    private int currentSpawnIndex;

    public GameObject spawnEffectPrefab;

    private GameObject currentVisibleSpawnEffectGo;
    private float delaySinceStartSpawn;
    private bool itemHasBeenSpawned;

    private List<Collider2D> allItemsColliders;

    public BlackHoleBehaviour blackHole;

    public List<SpawnableItem> allSpawnableItems;
    private SpawnableItem selectedNextSpawnableItem;

    public Animator uiPanelAnimator;
    
    // Start is called before the first frame update
    void Start()
    {
        currentSpawnIndex = 0;
        instance = this;
        currentVisibleSpawnEffectGo = null;
        allItemsColliders = new List<Collider2D>();
        blackHole = null;
        selectedNextSpawnableItem = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddItem(GameObject newItem)
    {
        allItemsColliders.Add(newItem.GetComponent<Collider2D>());
    }

    public void SetNextSpawnItemType(ItemType item)
    {
        if (item == ItemType.APPLE)
        {
            selectedNextSpawnableItem = allSpawnableItems[0];
        }
        else if (item == ItemType.BOUNCING_BALL)
        {
            selectedNextSpawnableItem = allSpawnableItems[1];
        }
        else if (item == ItemType.BUBBLE)
        {
            selectedNextSpawnableItem = allSpawnableItems[2];
        }
        else if(item == ItemType.ANVIL)
        {
            selectedNextSpawnableItem = allSpawnableItems[3];
        }
        else if(item == ItemType.GLASS)
        {
            selectedNextSpawnableItem = allSpawnableItems[4];
        }
        else if (item == ItemType.DUCK)
        {
            selectedNextSpawnableItem = allSpawnableItems[5];
        }
        else if (item == ItemType.BLACK_HOLE)
        {
            selectedNextSpawnableItem = allSpawnableItems[6];
        }
    }

    private void SpawnItem(SpawnableItem spawnItem, Vector3 position)
    {
        Quaternion randomOrientation = Quaternion.Euler(0, 0, Random.Range(-45, 45));

        currentSpawnIndex = (currentSpawnIndex + 1 >= spawnableItemsList.Count) ? 0 : (currentSpawnIndex + 1);

        List<Vector3> deltaSpawns = new List<Vector3>() { Vector3.zero, 2 * (Vector3.right + Vector3.up), 2 * (-Vector3.right + Vector3.up), -2 * (Vector3.right + Vector3.up), 2 * (Vector3.right - Vector3.up), -2 * Vector3.right, -2 * Vector3.up, 2 * Vector3.up, 2 * Vector3.right };
        
        int currentIndex = 0;
        Vector3 deltaSpawn = deltaSpawns[currentIndex];

        for (int i = 0; i < spawnItem.count; i++)
        {
            GameObject spawnedItem = Instantiate(spawnItem.prefab, position + deltaSpawn, randomOrientation, this.transform);

            if (spawnedItem.GetComponent<Collider2D>() != null)
            {
                allItemsColliders.Add(spawnedItem.GetComponent<Collider2D>());
            }

            currentIndex++;
            deltaSpawn = deltaSpawns[currentIndex];
            deltaSpawn += Random.Range(-0.1f, 0.1f) * Vector3.right + Random.Range(-0.2f, 0.2f) * Vector3.up;
        }
    }

    private IEnumerator WaitAndSpawnItem(SpawnableItem spawnItem, Vector3 position, float waitDelay)
    {
        yield return new WaitForSeconds(waitDelay);
        SpawnItem(spawnItem, position);
    }

    public ClickableItemBehaviour GetClickedItem(Vector3 mousePosition)
    {
        ClickableItemBehaviour clickedItem = null;
        Vector2 mousePos2D = new Vector2(mousePosition.x, mousePosition.y);
        foreach (Collider2D col in allItemsColliders)
        {
            if (col != null && col.OverlapPoint(mousePos2D))
            {
                clickedItem = col.GetComponent<ClickableItemBehaviour>();
                break;
            }
        }
        return clickedItem;
    }

    public void StartHoldingItem(ClickableItemBehaviour item, Vector3 mousePosition)
    {
        item.GetComponent<Rigidbody2D>().isKinematic = true;
    }

    public void UpdateHeldItemPosition(ClickableItemBehaviour item, Vector3 mousePosition)
    {
        item.GetComponent<Rigidbody2D>().MovePosition(new Vector2(mousePosition.x, mousePosition.y));
    }

    public void ReleaseHeldItem(ClickableItemBehaviour item, Vector3 mousePosition, Vector3 mousePositionAtLastFrame)
    {
        Vector3 newVelocity = (mousePosition - mousePositionAtLastFrame) / Time.deltaTime;
        Vector2 newVelocity2D = new Vector2(newVelocity.x, newVelocity.y);
        item.GetComponent<Rigidbody2D>().isKinematic = false;
        item.GetComponent<Rigidbody2D>().velocity = newVelocity2D;
    }

    public void ComputeClickOnItem(ClickableItemBehaviour item)
    {
        item.HandleClick();
    }

    public void StartSpawnEffect(Vector3 position)
    {
        itemHasBeenSpawned = false;
        currentVisibleSpawnEffectGo = Instantiate(spawnEffectPrefab, position, Quaternion.identity, this.transform);
        currentVisibleSpawnEffectGo.GetComponent<ParticleSystem>().Play();
        delaySinceStartSpawn = 0;
    }
    public void UpdateSpawnEffectPosition(Vector3 position)
    {
        currentVisibleSpawnEffectGo.transform.position = position;
        delaySinceStartSpawn += Time.deltaTime;

        SpawnableItem spawnItem;
        if (selectedNextSpawnableItem != null)
        {
            spawnItem = selectedNextSpawnableItem;
        }
        else
        {
            spawnItem = spawnableItemsList[currentSpawnIndex];
        }

        if (!itemHasBeenSpawned && delaySinceStartSpawn > spawnItem.spawnDelay)
        {
            StartCoroutine(WaitAndSpawnItem(spawnItem, position, 1));
            itemHasBeenSpawned = true;
        }
    }
    public void StopSpawnEffect()
    {
        currentVisibleSpawnEffectGo.GetComponent<ParticleSystem>().Stop();
        if (!itemHasBeenSpawned)
        {
            currentVisibleSpawnEffectGo.GetComponent<AudioSource>().Stop();
        }
        StartCoroutine(WaitAndDestroyGo(10, currentVisibleSpawnEffectGo));
    }

    private IEnumerator WaitAndDestroyGo(float delay, GameObject go)
    {
        yield return new WaitForSeconds(delay);
        Destroy(go);
    }

    public void BlackHolePresent(BlackHoleBehaviour bH)
    {
        blackHole = bH;
    }
    public void BlackHoleOver()
    {
        blackHole = null;
        foreach (Collider2D itemCol in allItemsColliders)
        {
            if (itemCol != null && itemCol.gameObject != null && itemCol.gameObject.activeInHierarchy)
            {
                Destroy(itemCol.gameObject);
            }
        }
        allItemsColliders.Clear();

        uiPanelAnimator.SetBool("Visible", true);
    }

    public void VacuumAllItems()
    {
        foreach (Collider2D itemCol in allItemsColliders)
        {
            ClickableItemBehaviour item = itemCol.GetComponent<ClickableItemBehaviour>();
            item.VacuumTowards(blackHole.transform.position);
        }
    }
}
