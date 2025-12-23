using UnityEngine;

public class RockSpawner : MonoBehaviour
{
    public RockLinearMotion rockPrefab;
    public Transform[] dropPoints;
    public float interval = 4f;
    public bool randomizePoint = true;
    public bool autoStart = true;
    //if true drop from  point each interval (constant rockfall wall)
    public bool spawnAllPointsEachInterval = false;

    //  if randomizeoint is false  cycle through points instead of always using 0
    public bool sequentialPoints = false;

    

     [Header("Cleanup")]
     public bool autoDestroySpawnedRocks = true;
     public float destroyAfterSeconds = 20f;


    private int _nextIndex = 0; 

    private float _timer;
    //MATHS CONTENT PRESENT HERE
    private void Start()
    {
        _timer = autoStart ? interval : Mathf.Infinity;
    }
    //MATHS CONTENT PRESENT HERE
    private void Update()
    {
        if (_timer == Mathf.Infinity) return;

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            SpawnOne();
            _timer = interval;
        }
    }

    public void SpawnOne() //MATHS CONTENT PRESENT HERE
    {
        if (rockPrefab == null || dropPoints == null || dropPoints.Length == 0) return;

        //  constant rockfall drops from all points 
        if (spawnAllPointsEachInterval)
        {
                        for (int i = 0; i < dropPoints.Length; i++)
                             {
                                 Transform tp = dropPoints[i];
                                RockLinearMotion r = Instantiate(rockPrefab, tp.position, tp.rotation);
                                r.Drop();
                
                 if (autoDestroySpawnedRocks && destroyAfterSeconds > 0f)
                                         Destroy(r.gameObject, destroyAfterSeconds);
                             }

            return;
        }

        Transform t;

        // randomized point
        if (randomizePoint)
        {
            t = dropPoints[Random.Range(0, dropPoints.Length)];
        }
        // non-random
        else if (sequentialPoints)
        {
            t = dropPoints[_nextIndex];
            _nextIndex = (_nextIndex + 1) % dropPoints.Length;
        }
        //  non-random  0
        else
        {
            t = dropPoints[0];
        }
                 RockLinearMotion rock = Instantiate(rockPrefab, t.position, t.rotation);
                 rock.Drop();
        
         if (autoDestroySpawnedRocks && destroyAfterSeconds > 0f)
                         Destroy(rock.gameObject, destroyAfterSeconds);


    }
}
