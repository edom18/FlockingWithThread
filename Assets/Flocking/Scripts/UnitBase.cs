using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitBase : MonoBehaviour
{
    #region SerializeField
    [SerializeField]
    private Transform _leader;
    public Transform Leader
    {
        get { return _leader; }
        set { _leader = value; }
    }

    [SerializeField]
    private float _speed = 1f;
    public float Speed
    {
        get { return _speed; }
        set { _speed = value; }
    }

    [SerializeField]
    private bool _isLeader = false;

    [SerializeField]
    private float _elasticity = 5f;
    #endregion SerializeField

    #region Variables
    // 目標とするオブジェクト
    private Transform _target;
    public Transform Target
    {
        get
        {
            // ターゲットが未設定の場合はリーダーをターゲット
            if (_target == null)
            {
                return _leader;
            }

            return _target;
        }
        set { _target = value; }
    }

    public Vector3 CachedPos { get; private set; }

    private DroneFactory _droneFactory;
    private List<UnitBase> Units
    {
        get
        {
            return _droneFactory.Units;
        }
    }

    // 実際の移動先位置ベクトル
    private Vector3 _moveTarget;

    private float _neighborLimit = 2.5f * 2.5f;
    private float _leaderDistanceMax = 2f * 2f;
    private float _invLeaderDistanceMax;

    private Vector3 _targetPosition;

    private List<UnitBase> _neighbors = new List<UnitBase>();

    private readonly Vector3 _zeroCheck = Vector3.zero;
    #endregion Variables

    void Start()
    {
        // 距離補正用に予め逆数を計算しておく
        _invLeaderDistanceMax = 1f / (_leaderDistanceMax - 0.1f);

        _droneFactory = FindObjectOfType<DroneFactory>();
    }

    private void Update()
    {
        CachedPos = transform.position;

        if (_isLeader)
        {
            return;
        }

        _targetPosition = Target.transform.position;

        CalcAlignDirection();

        transform.position = Vector3.Lerp(transform.position, _moveTarget, Time.deltaTime * _speed);
    }

    /// <summary>
    /// スレッドによって呼ばれる
    /// 近傍のユニットを元に、次の位置を決定する。
    /// </summary>
	public void UpdatePosition(float timeStep)
    {
        if (_isLeader)
        {
            return;
        }

        CalcCenterOfMass(timeStep);
    }

    /// <summary>
    /// 方向をリーダーに合わせる
    /// </summary>
    void CalcAlignDirection()
    {
        Vector3 dir = Vector3.Lerp(transform.forward, _leader.forward, Time.deltaTime);
        transform.forward = dir;
    }

    /// <summary>
    /// 全体を見て、結合行動をさせる位置に移動する
    /// </summary>
    void CalcCenterOfMass(float timeStep)
    {
        _neighbors.Clear();

        // Cache
        int count = Units.Count;

        for (int i = 0; i < count; i++)
        {
            UnitBase unit = Units[i];
            if (unit == this) { continue; }
            if (unit.Target != Target) { continue; }
            if ((unit.CachedPos - CachedPos).sqrMagnitude > _neighborLimit) { continue; }

            _neighbors.Add(unit);
        }

        // 近くにいない場合はリーダーに向かって近づくように移動する
        if (_neighbors.Count == 0)
        {
            _moveTarget = _targetPosition;
            return;
        }

        Vector3 center = Vector3.zero;
        Vector3 separate = Vector3.zero;
        UnitBase u;
        Vector3 delta, uPos;
        for (int i = 0; i < _neighbors.Count; i++)
        {
            u = _neighbors[i];
            uPos = u.CachedPos;

            // 1.
            // 結合行動のために近傍ユニットの重心を求める
            center += uPos;

            // 2.
            // 分離行動のために離れる距離を計算する
            delta = CachedPos - uPos;
            separate += delta.normalized / delta.magnitude;
        }
        center /= _neighbors.Count;

        // 結合行動、分離行動の力を合成する
        Vector3 combineVec = center + separate;

        // 3.
        // リーダーの近傍範囲から離れていた場合は、リーダーの方に近づく力を混ぜる
        Vector3 distance = _targetPosition - CachedPos;
        bool needsSeekLeader = Mathf.Abs(distance.sqrMagnitude) >= _leaderDistanceMax;
        if (needsSeekLeader)
        {
            combineVec += distance;
        }
        else
        {
            // 近づきすぎている場合は離れる力を混ぜる
            Vector3 dist = combineVec - _targetPosition;
            float force = Mathf.Clamp01(1f - dist.magnitude * _invLeaderDistanceMax) * _elasticity;
            combineVec += dist.normalized * force;
        }

        // 計算結果をターゲットとして保持
        if (_moveTarget == _zeroCheck)
        {
            _moveTarget = combineVec;
        }
        else
        {
            // 近傍でガクつくので前の位置と補間して滑らかにする
            _moveTarget = Vector3.Lerp(_moveTarget, combineVec, timeStep);
        }
    }
}
