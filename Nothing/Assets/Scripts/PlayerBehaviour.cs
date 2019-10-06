using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    public static PlayerBehaviour instance;

    [Header("Settings")]
    public float distanceFromWorld;
    public float delayToConsiderDrag;
    public float distanceToConsiderDrag;


    [Header("Runtime")]
    public bool isDraggingItem;

    private bool spawnSpellActive;
    private bool holdItem;
    private ClickableItemBehaviour currentHeldItem;

    private float timeSinceMouseDown;
    private Vector3 positionWhenMouseDown;
    private Vector3 previousMousePosition;



    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        spawnSpellActive = false;
        holdItem = false;
        timeSinceMouseDown = 0;
    }

    private void HandleMouseDown(Vector3 mousePosition)
    {
        timeSinceMouseDown = Time.time;
        positionWhenMouseDown = mousePosition;
        isDraggingItem = false;

        currentHeldItem = GameEngineBehaviour.instance.GetClickedItem(mousePosition);
        if (currentHeldItem != null)
        {
            // we just clicked on an item
            holdItem = true;
            GameEngineBehaviour.instance.StartHoldingItem(currentHeldItem, mousePosition);
        }
        else
        {
            // no click on item : we start spawn spell
            Vector3 positionInSpace = Camera.main.ScreenToWorldPoint(Input.mousePosition + distanceFromWorld * Vector3.forward);
            GameEngineBehaviour.instance.StartSpawnEffect(positionInSpace);
            spawnSpellActive = true;
        }
    }

    private void HandleMouseMove(Vector3 mousePosition)
    {
        if (spawnSpellActive)
        {
            GameEngineBehaviour.instance.UpdateSpawnEffectPosition(mousePosition);
        }
        else if (holdItem)
        {
            GameEngineBehaviour.instance.UpdateHeldItemPosition(currentHeldItem, mousePosition);
            if (!isDraggingItem && ((Time.time - timeSinceMouseDown) > delayToConsiderDrag || Vector3.Distance(mousePosition, positionWhenMouseDown) > distanceToConsiderDrag))
            {
                isDraggingItem = true;
            }
        }
        
    }

    private void HandleMouseRelease(Vector3 mousePosition)
    {
        if (spawnSpellActive)
        {
            GameEngineBehaviour.instance.StopSpawnEffect();
            spawnSpellActive = false;
        }
        else if (holdItem)
        {
            GameEngineBehaviour.instance.ReleaseHeldItem(currentHeldItem, mousePosition, previousMousePosition);
            if (!isDraggingItem)
            {
                GameEngineBehaviour.instance.ComputeClickOnItem(currentHeldItem);
            }
            currentHeldItem = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameEngineBehaviour.instance.blackHole == null)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }

            Vector3 positionInSpace = Camera.main.ScreenToWorldPoint(Input.mousePosition + distanceFromWorld * Vector3.forward);

            if (Input.GetMouseButtonDown(0))
            {
                // mouse button down
                HandleMouseDown(positionInSpace);
            }

            if (Input.GetMouseButton(0))
            {
                // mouse button hold
                HandleMouseMove(positionInSpace);
            }

            if (Input.GetMouseButtonUp(0))
            {
                HandleMouseRelease(positionInSpace);
            }

            previousMousePosition = positionInSpace;
        }
    }
}
