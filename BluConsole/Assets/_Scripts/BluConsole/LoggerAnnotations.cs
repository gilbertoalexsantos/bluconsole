using UnityEngine;
using System.Collections;
using System;


namespace BluConsole
{

[AttributeUsageAttribute(AttributeTargets.Method)]
public class StackTraceIgnore : Attribute
{
}

}