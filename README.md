unity-bluconsole
=======================


TODO
-----

* Limit the number of characters in message
* Popup to copy full raw message (without the limit of characters)
* Create a logic to move the scroll to the end
* Change every dynamic LayoutGroup (LogList and LogDetail) to use Rects instead of relying on the LayoutGroup. It'll make it faster and fix rares LayoutGroup crazy exceptions. Thanks Unity for the best Gui system ever!
* Create a Dirty logic save Repaint logics
* Cache the filtered list of logs (and use some Dirty logic to recalculate)


Known Issues
------------

* Sometimes when resizing the log panel, it happens an exception. ArgumentException: GUILayout: Mismatched LayoutGroup.Ignore. It happens in the BeginHorizontal from DragLogList
* When using the button to move the LogList ScrollView, at each three clicks (?), there's a hipcup. I think changing the LogList to always Draw RECTS (in other words, you know the position of everything, instead of rely on LayoutGroup)
