using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

public class KatoSpawner : MonoBehaviour
{
    [FormerlySerializedAs("_StatList")] [SerializeField, ListDrawerSettings(HideAddButton = true)]
    private List<StatAttribute> _Stats = new List<StatAttribute>();

    [SerializeField] private KatoManager _KatoPrefab;
    [SerializeField] private float _SpawnIntervals;
    [SerializeField] private Vector2 _SpawnCount;
    [SerializeField] private Transform _SpawnPoint;
    [SerializeField] private int _KatoType;
    [SerializeField] private TextMeshProUGUI _IntervalUI;

    [Header("Add Stat"), InfoBox("$_AddErrorMessage", InfoMessageType.Error, "_AddError")]
    [SerializeField] private String _Attribute;

    [SerializeField] private Vector2 _Range;

    private String _AddErrorMessage = "";
    private bool _AddError;
    private Random _Random = new Random();
    public List<StatAttribute> Stats => _Stats;
    public KatoManager KatoPrefab => _KatoPrefab;

    [Button("Add")]
    private void AddAttribute()
    {
        if (String.IsNullOrEmpty(_Attribute))
        {
            _AddErrorMessage = "You must select an Attribute to assign the value.";
            _AddError = true;
        }
        else if (_Stats.Any(x => x.Type == _Attribute))
        {
            _AddErrorMessage = "Attribute already defined.";
            _AddError = true;
        }
        else
        {
            _Stats.Add(new StatAttribute(_Attribute, _Range));
            _AddError = false;
            _Attribute = "";
            _Range = new Vector2();
        }
    }

    private void Start()
    {
        if (_IntervalUI != null)
        {
            _IntervalUI.text = _SpawnIntervals.ToString("0") + "s";
        }

        StartCoroutine(SpawnPeriodically());
    }

    public void AlterInterval(float number)
    {
        _SpawnIntervals += number;
        if (_IntervalUI != null)
        {
            _IntervalUI.text = _SpawnIntervals.ToString("0") + "s";
        }
    }

    private IEnumerator SpawnPeriodically()
    {
        while (true)
        {
            var spawnCount = _Random.Next((int) _SpawnCount.x, (int) _SpawnCount.y);
            for (int i = 0; i < spawnCount; i++)
            {
                var stats = new Dictionary<string, float>();
                foreach (var stat in _Stats)
                {
                    stats.Add(stat.Type, (float) (_Random.NextDouble() * (stat.Range.y - stat.Range.x) + stat.Range.x));
                }

                stats.Add("Type", _KatoType);
                var kato = UniversalObjectPool.instance.GetObject(KatoPrefab, _SpawnPoint.position,
                    _SpawnPoint.rotation);
                kato.Initialize(stats, this);
            }

            yield return new WaitForSeconds(_SpawnIntervals);
        }
    }
}

[Serializable]
public class StatAttribute
{
    [Title("$_Type")]
    [SerializeField] private Vector2 _Range;

    [SerializeField, HideInInspector] private string _Type;

    public Vector2 Range
    {
        get => _Range;
        set { _Range = value; }
    }

    public string Type => _Type;

    public StatAttribute(String type, Vector2 range)
    {
        _Type = type;
        _Range = range;
    }
}