using System;


namespace BluConsole
{

    [AttributeUsageAttribute(AttributeTargets.Method)]
    public class StackTraceIgnore : Attribute
    {
    }

}
