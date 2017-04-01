using System;


namespace BluConsole.Extensions
{

public static class Extensions 
{

    public static void SafeInvoke(this Action action)
    {
        if (action != null)
            action();
    }
    
}

}
