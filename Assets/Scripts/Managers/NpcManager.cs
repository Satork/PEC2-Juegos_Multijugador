using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tank;

public class NpcManager : MonoBehaviour
{
    public PlayerTank tanque;
    
    // Start is called before the first frame update
    void Start()
    {
        tanque.m_PlayerColor = Color.yellow;
        tanque.SetOriginalColor();
        tanque.m_PlayerName = "CPU";
        tanque.SetPlayerName();
    }

    
}
