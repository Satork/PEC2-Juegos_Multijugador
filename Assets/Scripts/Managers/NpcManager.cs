using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using Tank;
using Complete;

public class NpcManager : MonoBehaviour
{
    public PlayerTank tanque;
    public TankShooting disparos;
    private float timeBetweenShoots = 0.8f; 
    private float actualTimeBetweenShoots = 0;
    private bool atackmode;

    
    void Start()
    {
        if (this.TryGetComponent(out TankShooting player)) {
            this.disparos = player;
        }
        tanque.m_PlayerColor = Color.yellow;
        tanque.SetOriginalColor();
        tanque.m_PlayerName = "CPU";
        tanque.SetPlayerName();
    }
    void Update(){
        if (atackmode){
            actualTimeBetweenShoots += Time.deltaTime;
            
        }

    }
    public void OnTriggerEnter(Collider col){
        if(col.gameObject.tag == "Player"){
            atackmode = true;
            Atack(col);
        }
    }
    public void OnTriggerStay(Collider col){
        if (col.gameObject.tag == "Player")
        {
            atackmode = true;
            Atack(col);
        }
    }
    public void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            atackmode = false;
        }
    }
    public void Atack(Collider col){
        Vector3 lookDirection = col.transform.position - transform.position;
        transform.rotation = Quaternion.FromToRotation(Vector3.forward, new Vector3(lookDirection.x, 0, lookDirection.z));
        if(actualTimeBetweenShoots > timeBetweenShoots)
        {
            actualTimeBetweenShoots = 0;
            
                disparos.FireNPC();
        }

    }


    
}
