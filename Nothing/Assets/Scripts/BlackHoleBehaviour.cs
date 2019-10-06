using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHoleBehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameEngineBehaviour.instance.BlackHolePresent(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void VacuumEverything()
    {
        GameEngineBehaviour.instance.VacuumAllItems();
    }

    public void EndBlackHole()
    {
        GameEngineBehaviour.instance.BlackHoleOver();
    }
}
