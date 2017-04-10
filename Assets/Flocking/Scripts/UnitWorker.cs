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

	void Initialize()
    {
        _thread = new Thread(ThreadRun);
        _thread.IsBackground = true;
        _thread.Start();
	}

    public void Run()
    {
        _timeStep = Time.deltaTime;
        _isRunning = true;
        _mre.Set();
    }

    void Calculate()
    {
        UnitBase unit;
        for (int i = 0; i < Units.Count; i++)
        {
            unit = Units[i];
            unit.UpdatePosition(_timeStep);
        }
    }

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
