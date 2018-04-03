using BluConsole.Editor;
using BluConsole.Extensions;
using UnityEngine;


public class BluDetailWindowCell
{

	public BluDetailWindowCell(string message, 
							   int selectedMessageWhenClicked, 
							   int stackTraceIndex, 
							   GUIStyle messageStyle, 
							   GUIStyle backgroundStyle)
	{
		Message = message;
		SelectedMessageWhenClicked = selectedMessageWhenClicked;
		StackTraceIndex = stackTraceIndex;
		MessageStyle = messageStyle;
		BackgroundStyle = backgroundStyle;
	}

    public string Message { get; set; }

	public GUIStyle MessageStyle { get; private set; }

	public GUIStyle BackgroundStyle { get; private set; }

	public int SelectedMessageWhenClicked { get; private set; }

	public int StackTraceIndex { get; private set; }

    public float GetHeight(float width)
	{
		string message = string.IsNullOrEmpty(Message) ? "A" : Message;
		return BluConsoleSkin.MessageDetailCallstackStyle.CalcHeight(message.GUIContent(), width);
	}

}
