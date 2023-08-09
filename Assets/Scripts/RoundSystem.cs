using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RoundSystem : MonoBehaviour
{
    private MyNetworkManager Room { get => NetworkManager.singleton as MyNetworkManager; }

    public static float roundTime = 60f;
        
    void Update()
    {
        DecrementTime();
    }

    void DecrementTime()
    {
        if (roundTime > 0)
        {
            roundTime -= Time.deltaTime;

            if (roundTime <= 0)
            {
                foreach (FirstPersonPlayer fpp in Room.players)
                {
                    fpp.RoundEnded = true;
                    Room.RoundEnded = true;
                }
            }
        }
    }
}
