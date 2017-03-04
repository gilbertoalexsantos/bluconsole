BluConsole <img src="images/console-brand.png" width=30 style="margin: 0px 0px -5px">
========================

Are you looking for [images](#images)?

Fell free to contribute! Any PR are welcome (just be consistent with the code guideline)

Any doubts, you can post an issue, or send me an email: <gilberto.alexsantos@gmail.com>

Features Implemented
----------------------------

It has all the UnityConsole features, with a few additions:


#### Search Box

You can filter your logs in a [Helm](https://github.com/emacs-helm/helm) like way.

#### Callstack Navigation

You can open any line of the call stack with a double click.

#### Log Copy

Right click on the Log, and copy the text to the Clipboard. Simple like that!

### Default filter

If you catch yourself filtering always by the same query, just put that in the FilterSettings.

#### StackTraceIgnore

If you want to ignore a function in the StackTrace, just put the [StackTraceIgnore] annotation on it.

<br>

For now, that's it! New features are coming, stay tuned.


TODO
----------------------------

* Publish in AssetStore
	* Create a description file to the AssetStore (see [submission guideline](https://unity3d.com/asset-store/sell-assets/submission-guidelines))
	* Create [Key Images](https://unity3d.com/asset-store/sell-assets/submission-guidelines) to AssetStore submission
* Support Regex in the Search Box
* Support for themes (Font size, Colors, etc)

Known Issues
----------------------------

* When filtering logs, the toggles with the number of logs are displayed incorrectly
* Sometimes when you double click the log to open the file on your Editor, if your unity is configured to open VS, it opens Mono instead. I saw a workaround here:
[A workaround exists: Manually open Visual Studio and open the Unity project. Now double clicking a file will open it in Visual Studio but the whole project has to be migrated each time... it appears Unity is not accepting the Visual Studio Migration changes.](http://answers.unity3d.com/questions/236390/monodevelop-opens-instead-of-visual-studio.html)


License
----------------------------

[License](LICENSE)


Copying
----------------------------

[Copying](COPYING)


Images <a name="images"></a>
----------------------------

![](images/image01.png)
<hr>
![](images/image02.png)
<hr>
![](images/image03.png)
<hr>
