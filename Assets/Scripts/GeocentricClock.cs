using UnityEngine;
using UnityEngine.Events;

public class GeocentricClock : MonoBehaviour
{
    [SerializeField] private float _SecondsPerDay = 60f;
    [SerializeField] private Material _KatoMaterial;

    public UnityEvent<float> OnTimeChanged = new UnityEvent<float>();

    public float CurrentTime => _CurrentTime;
    public static GeocentricClock Instance;

    private float _CurrentTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (_CurrentTime < _SecondsPerDay)
        {
            _CurrentTime += Time.deltaTime;
            if (_CurrentTime > _SecondsPerDay)
            {
                _CurrentTime -= _SecondsPerDay;
            }

            //NightTime
            if (_CurrentTime >= _SecondsPerDay * 0.25f && _CurrentTime < _SecondsPerDay * 0.5f)
            {
                var intensity = (_CurrentTime - _SecondsPerDay * 0.25f) / (_SecondsPerDay * 0.25f);
                _KatoMaterial.SetVector("_EmissionColor", new Vector4(intensity,intensity,intensity));
            }
            else if (_CurrentTime >= _SecondsPerDay * 0.5f && _CurrentTime < _SecondsPerDay * 0.75f)
            {
                var intensity = 1f - ((_CurrentTime - _SecondsPerDay * 0.5f) / (_SecondsPerDay * 0.25f));
                _KatoMaterial.SetVector("_EmissionColor", new Vector4(intensity,intensity,intensity));
            }
            else
            {
                _KatoMaterial.SetVector("_EmissionColor", Color.black);
            }

            var timePercent = _CurrentTime / _SecondsPerDay;
            transform.rotation = Quaternion.Euler(timePercent * 360f, 0f, 0f);
            OnTimeChanged.Invoke(timePercent);
        }
    }

    private float GetTimeOfDay()
    {
        return _CurrentTime / _SecondsPerDay;
    }
}