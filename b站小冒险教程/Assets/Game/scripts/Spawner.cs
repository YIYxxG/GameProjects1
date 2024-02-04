using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Spawner : MonoBehaviour
{
   private List<SpawnPoint> spawnPointList;
   private List<character> spawnCharacters;
   private bool hasSpowned;
   public Collider _collider;
   public UnityEvent OnAllSpawnedCharacterEliminated;
   private void Awake()
   {
        var spawnPointArray = transform.parent.GetComponentsInChildren<SpawnPoint>();
        spawnPointList = new List<SpawnPoint>(spawnPointArray);
        spawnCharacters = new List<character>();
   }
   private void Update() 
   {
        if(!hasSpowned || spawnCharacters.Count == 0)
        {
            return;
        }
        bool allSpawnedAreDead = true;
        foreach(character c in spawnCharacters)
        {
            if (c.CurrentState != character.CharacterState.Dead)
            {
                allSpawnedAreDead = false;
                break;
            }
        }
        if(allSpawnedAreDead)
        {
            if(OnAllSpawnedCharacterEliminated != null)
            {
                OnAllSpawnedCharacterEliminated.Invoke();
                
            }
            spawnCharacters.Clear();
        }
   }
   public void SpawnCharacters()
   {
        if(hasSpowned)
        {
            return;
        }
        hasSpowned = true;
        foreach(SpawnPoint point in spawnPointList)
        {
            if(point.EnemyToSpawn !=null)
            {
                GameObject spawnedGameobject = Instantiate(point.EnemyToSpawn,point.transform.position,point.transform.rotation);
                spawnCharacters.Add(spawnedGameobject.GetComponent<character>());
            }
        }
   }
   private void OnTriggerEnter(Collider other) 
   {
        if(other.tag == "Player")
        {
            SpawnCharacters();
        }
   }
   private void OnDrawGizmos() 
   {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position,_collider.bounds.size);
   }
}
