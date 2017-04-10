using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class UnitWorker
{
    private readonly ManualResetEvent _mre = new ManualResetEvent(false);
    private Thread _thread;
    private bool _isRunning = false;

    private float _timeStep = 0;

    public List<UnitBase> Units { get; set; }

    // コンストラクタ
    public UnitWorker()
    {
        Initialize();
    }

    /// <summary>
    /// 初期化
    /// </summary>
	void Initialize()
    {
        _thread = new Thread(ThreadRun);
        _thread.IsBackground = true;
        _thread.Start();
	}

    /// <summary>
    /// 計算スレッドを開始
    /// </summary>
    public void Run()
    {
        _timeStep = Time.deltaTime;
        _isRunning = true;
        _mre.Set();
    }

    /// <summary>
    /// スレッドを終了
    /// </summary>
    public void Abort()
    {
        _thread.Abort();
    }

    /// <summary>
    /// ユニットの位置を計算
    /// </summary>
    void Calculate()
    {
        UnitBase unit;
        for (int i = 0; i < Units.Count; i++)
        {
            unit = Units[i];
            unit.UpdatePosition(_timeStep);
        }
    }

    /// <summary>
    /// スレッドを実行
    /// </summary>
    void ThreadRun()
    {
        _mre.WaitOne();

        try
        {
            Calculate();
        }
        finally
        {
            _isRunning = false;

            _mre.Reset();

            _thread = new Thread(ThreadRun);
            _thread.IsBackground = true;
            _thread.Start();
        }
    }
}
