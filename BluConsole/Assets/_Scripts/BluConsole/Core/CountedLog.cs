
namespace BluConsole
{

public class CountedLog
{

    private LogInfo _log;
    private int _quantity;

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