using UnityEngine;

public class RockSpawner : MonoBehaviour
{
    public RockLinearMotion rockPrefab;
    public Transform[] dropPoints;
    public float interval = 4f;
    public bool randomizePoint = true;
    public bool autoStart = true;

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

        Transform t = randomizePoint ? dropPoints[Random.Range(0, dropPoints.Length)] : dropPoints[0];
        RockLinearMotion rock = Instantiate(rockPrefab, t.position, t.rotation);
        rock.Drop();
    }
}
