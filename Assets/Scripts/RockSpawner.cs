using UnityEngine;

public class RockSpawner : MonoBehaviour
{
    public RockLinearMotion rockPrefab;
    public Transform[] dropPoints;
    public float interval = 4f;
    public bool randomizePoint = true;
    public bool autoStart = true;
    // NEW (defaults OFF): if true, drop from EVERY point each interval (constant rockfall wall)
    public bool spawnAllPointsEachInterval = false;

    // NEW (defaults OFF): if randomizePoint is false, you can cycle through points instead of always using [0]
    public bool sequentialPoints = false;

    private int _nextIndex = 0; // used only when sequentialPoints = true

    private float _timer;

    private void Start()
    {
        _timer = autoStart ? interval : Mathf.Infinity;
    }

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

    public void SpawnOne()
    {
        if (rockPrefab == null || dropPoints == null || dropPoints.Length == 0) return;

        // NEW: constant rockfall mode (drops from ALL points every interval)
        if (spawnAllPointsEachInterval)
        {
            for (int i = 0; i < dropPoints.Length; i++)
            {
                Transform tp = dropPoints[i];
                RockLinearMotion r = Instantiate(rockPrefab, tp.position, tp.rotation);
                r.Drop();
            }
            return;
        }

        Transform t;

        // Existing behavior (unchanged): randomized point
        if (randomizePoint)
        {
            t = dropPoints[Random.Range(0, dropPoints.Length)];
        }
        // NEW: non-random but sequential through points
        else if (sequentialPoints)
        {
            t = dropPoints[_nextIndex];
            _nextIndex = (_nextIndex + 1) % dropPoints.Length;
        }
        // Existing behavior (unchanged): non-random always uses point 0
        else
        {
            t = dropPoints[0];
        }

        RockLinearMotion rock = Instantiate(rockPrefab, t.position, t.rotation);
        rock.Drop();

    }
}
