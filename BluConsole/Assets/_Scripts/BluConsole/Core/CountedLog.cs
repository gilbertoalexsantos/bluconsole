using UnityEngine;
using System;
using System.Collections;


namespace BluConsole
{

[Serializable]
public class CountedLog
{

    [SerializeField] private LogInfo _log;
    [SerializeField] private int _quantity;

    public LogInfo Log
    {
        get
        {
            return _log;
        }
    }

    public int Quantity
    {
        get
        {
            return _quantity;
        }
        set
        {
            _quantity = value;
        }
    }

    public CountedLog(
        LogInfo log,
        int quantity)
    {
        _log = log;
        _quantity = quantity;
    }
	
}

}