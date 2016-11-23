BluConsole
========================


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

<br>

For now, that's it! New features are coming, stay tuned.


Features to Come
----------------------------

#### Abstraction on top of Debug.Log

Sometimes, we want Debug.Log to only occur in the UnityEditor, or only in Debug mode, or only in Mobile.

So I propose an abstraction on top of Debug.Log, like an 'BluConsole.Log', with exactly the same behaviour and a few additions.

There will be a Global Configuration, that per default will allow the log to occur in all environments, and each time you log some message (using a directive like Debug.Log),
you'll have the option to override the Global Configuration (e.g log only in iOS devices).

It'll be nice to have some colors options too. Maybe an extension of the String class to embed it with colors directives.


#### Theme

The current theme (I didn't test it yet) probably will not look nice in the UnityPro (I'm using hardcoded colors).

Create a logic to use the colors of the theme of the editor.


#### Default Configurations

A scriptable object that will hold configurations, like:

* Maximum logs
* Maximum print characters
* Font size
* Theme
* Global Configurations of log (see the first section 'Abstraction no top of Debug.Log')


TODO
----------------------------

* Publish in AssetStore
	* Generate a license and put in the header of all files
	* Create a description file to the AssetStore (see [submission guideline](https://unity3d.com/asset-store/sell-assets/submission-guidelines))
	* Create [Key Images](https://unity3d.com/asset-store/sell-assets/submission-guidelines) to AssetStore submission
	* Change the window logo of the console (the one we're using reflection...)
* Support Regex in the Search Box


Known Issues
----------------------------

EMPTY


Copying
----------------------------

[Copying](COPYING)
