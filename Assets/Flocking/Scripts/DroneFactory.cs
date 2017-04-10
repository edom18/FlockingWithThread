using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class DroneFactory : MonoBehaviour
{
    #region Variables
    [SerializeField]
    private Transform _leader;

    [SerializeField]
    private Transform _target;

    [SerializeField]
    private GameObject _unitPrefab;

    private List<UnitBase> _units = new List<UnitBase>();
    public List<UnitBase> Units
    {
        get
        {
            return _units;
        }
    }

    private UnitWorker[] _unitWorkers = new UnitWorker[4];

    private bool _needsStopThread = false;
    #endregion Variables

    #region MonoBehaviour
    void Start()
    {
        _units = new List<UnitBase>(GetComponentsInChildren<UnitBase>());

        for (int i = 0; i < _unitWorkers.Length; i++)
        {
            _unitWorkers[i] = new UnitWorker();
        }

        GiveUnits();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.T))
        {
            Injetion();
        }

        if (Input.GetKey(KeyCode.A))
        {
            GenerateUnit();
        }

        for (int i = 0; i < _unitWorkers.Length; i++)
        {
            _unitWorkers[i].Run();
        }
    }
    #endregion MonoBehaviour

    /// <summary>
    /// 保持しているユニットを割り振る
    /// </summary>
    void GiveUnits()
    {
        int len = _unitWorkers.Length;
        int range = _units.Count / len;
        for (int i = 0; i < _unitWorkers.Length; i++)
        {
            List<UnitBase> units = _units.GetRange(range * i, range);
            _unitWorkers[i].Units = units;
        }
    }

    /// <summary>
    /// ユニットをターゲットに向けて射出する
    /// </summary>
    void Injetion()
    {
        UnitBase unit = Units.FirstOrDefault(u => u.Target != _target);
        if (unit != null)
        {
            unit.Target = _target;
        }
    }

    /// <summary>
    /// ユニットを生成してリストに追加する
    /// </summary>
    void GenerateUnit()
    {
        GameObject unitObj = Instantiate(_unitPrefab, transform.position, Quaternion.identity);
        UnitBase unit = unitObj.GetComponent<UnitBase>();
        unit.Leader = _leader;
        unit.Speed = Random.Range(0.2f, 0.5f);
        _units.Add(unit);

        GiveUnits();
    }
}