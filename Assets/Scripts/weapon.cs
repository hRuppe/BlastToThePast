using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class weapon : MonoBehaviour
{
    public abstract void PrimaryFire();

    public abstract void AltFire();

    public virtual bool CanFire()
    {
        return !gameManager.instance.isPaused;
    }

}
