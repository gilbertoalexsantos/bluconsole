using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluConsole.Core;
using BluConsole.Core.UnityLoggerApi;
using BluConsole.Extensions;


namespace BluConsole.Editor
{

    public class BluConsoleEditorWindow : EditorWindow, IHasCustomMenu
    {
        
        #region Variables

        public static BluConsoleEditorWindow Instance;

        // Cache Variables
        private UnityApiEvents _unityApiEvents;
        private BluLogSettings _settings;
        private BluLogConfiguration _configuration;
        private List<BluLog> _cacheLog = new List<BluLog>();
        private List<bool> _cacheLogComparer = new List<bool>();
        private List<string> _stackTraceIgnorePrefixs = new List<string>();
        private int[] _cacheIntArr = new int[100];
        private BluLog[] _cacheLogArr = new BluLog[100];
        private int _cacheLogCount;
        private int _qtLogs;

        // Resizer Variables
        private float _topPanelHeight;
        private Rect _cursorChangeRect;
        private bool _isResizing;

        private float _drawYPos;
        private BluListWindow _listWindow;
        private BluDetailWindow _detailWindow;
        private BluToolbarWindow _toolbarWindow;

        #endregion Variables


        #region Properties

        private bool IsRepaintEvent { get { return Event.current.type == EventType.Repaint; } } 
        
        #endregion Properties        

        
        #region Windows
        
        [MenuItem("Window/BluConsole")]
        public static void ShowWindow()
        {
            var window = EditorWindow.GetWindow<BluConsoleEditorWindow>("BluConsole");

            var consoleIcon = BluConsoleSkin.ConsoleIcon;
            if (consoleIcon != null)
                window.titleContent = new GUIContent("BluConsole", consoleIcon);
            else
                window.titleContent = new GUIContent("BluConsole");

            window._topPanelHeight = window.position.height / 2.0f;
        }
        
        #endregion Windows
        
        
        #region OnEvents

        private void OnEnable()
        {
            Instance = this;
            _stackTraceIgnorePrefixs = BluUtils.StackTraceIgnorePrefixs;
            _settings = BluLogSettings.Instance;
            _configuration = BluLogConfiguration.Instance;
            _unityApiEvents = UnityApiEvents.Instance;
            _listWindow = new BluListWindow();
            _detailWindow = new BluDetailWindow();
            _toolbarWindow = new BluToolbarWindow();
            SetDirtyLogs();
        }

        private void OnDestroy()
        {
            Instance = null;
            _settings = null;
            _unityApiEvents = null;
            Resources.UnloadAsset(titleContent.image);
            Resources.UnloadUnusedAssets();
        }

        private void OnGUI()
        {       
            InitVariables();
            CalculateResizer();

            BeginWindows();
            {
                CheckDirties();
                _drawYPos += DrawToolbarWindow(id: 1);
                PreProcessLogs();
                _drawYPos += DrawListWindow(id: 2);
                _drawYPos += DrawResizer();
                _drawYPos += DrawDetailWindow(id: 3);
            }
            EndWindows();

            Repaint();
        }

        public void OnBeforeCompile()
        { 
            SetDirtyLogs(); 
        }

        public void OnAfterCompile()
        {
            SetDirtyLogs();
            _listWindow.SelectedMessage = -1;
            _detailWindow.SelectedMessage = -1;
        }

        public void OnBeginPlay()
        {
            SetDirtyLogs();
        }

        public void OnStopPlay()
        {
            SetDirtyLogs();
        }
        
        #endregion OnEvents


        #region IHasCustomMenu

        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Add Filter"), false, OnAddFilterTabClicked);
        }

        private void OnAddFilterTabClicked()
        {
            if (_settings == null)
                return;
                
            Selection.activeObject = _settings;
        }

        #endregion IHasCustomMenu

        
        #region Draw

        private float DrawToolbarWindow(int id)
        {
            float height = BluConsoleSkin.ToolbarButtonStyle.CalcSize("Clear".GUIContent()).y;

            _toolbarWindow.Settings = _settings;
            _toolbarWindow.Configuration = _configuration;
            _toolbarWindow.ListWindow = _listWindow;
            _toolbarWindow.DetailWindow = _detailWindow;
            _toolbarWindow.HasSetDirtyLogs = false;
            _toolbarWindow.HasSetDirtyComparer = false;

            GUI.Window(id, new Rect(0, 0, position.width, height), _toolbarWindow.OnGUI, GUIContent.none, GUIStyle.none);

            return height;
        }        

        private float DrawListWindow(int id)
        {
            Rect originalRect = new Rect(0, _drawYPos, position.width, _topPanelHeight - _drawYPos);

            _listWindow.WindowRect = new Rect(0, 0, originalRect.width, originalRect.height);
            _listWindow.Rows = GetCachedIntArr(_qtLogs);
            _listWindow.Logs = GetCachedLogsArr(_qtLogs);
            _listWindow.LogConfiguration = _configuration;
            _listWindow.QtLogs = _qtLogs;
            _listWindow.StackTraceIgnorePrefixs = _stackTraceIgnorePrefixs;

            GUI.Window(id, originalRect, _listWindow.OnGUI, GUIContent.none, GUIStyle.none);

            return originalRect.height;
        }

        private float DrawDetailWindow(int id)
        {
            Rect originalRect = new Rect(0, _drawYPos, position.width, position.height - _drawYPos);

            _detailWindow.WindowRect = new Rect(0, 0, originalRect.width, originalRect.height);
            _detailWindow.Rows = GetCachedIntArr(_qtLogs);
            _detailWindow.Logs = GetCachedLogsArr(_qtLogs);
            _detailWindow.LogConfiguration = _configuration;
            _detailWindow.QtLogs = _qtLogs;
            _detailWindow.StackTraceIgnorePrefixs = _stackTraceIgnorePrefixs;
            _detailWindow.ListWindowSelectedMessage = _listWindow.SelectedMessage;

            GUI.Window(id, originalRect, _detailWindow.OnGUI, GUIContent.none, GUIStyle.none);

            return originalRect.height;
        }

        private float DrawResizer()
        {
            if (!IsRepaintEvent)
                return _configuration.ResizerHeight;
            
            var rect = new Rect(0, _drawYPos, position.width, _configuration.ResizerHeight);
            EditorGUI.DrawRect(rect, _configuration.ResizerColor);

            return _configuration.ResizerHeight;
        }

        #endregion Draw

        
        #region Gets

        private int[] GetCachedIntArr(int size)
        {
            if (size > _cacheIntArr.Length)
                _cacheIntArr = new int[size * 2];
            return _cacheIntArr;
        }

        private BluLog[] GetCachedLogsArr(int size)
        {
            if (size > _cacheLogArr.Length)
                _cacheLogArr = new BluLog[size * 2];
            return _cacheLogArr;
        }

        private BluLog GetSimpleLog(int row)
        {
            int realCount = _cacheLog.Count;
            var log = UnityLoggerServer.GetSimpleLog(row);
            if (realCount > row)
                _cacheLog[row] = log;
            else
                _cacheLog.Add(log);
            return _cacheLog[row];
        }

        private bool GetButtonClamped(GUIContent content, GUIStyle style)
        {
            return GUILayout.Button(content, style, GUILayout.MaxWidth(style.CalcSize(new GUIContent(content)).x));
        }

        private bool GetToggleClamped(bool state, GUIContent content, GUIStyle style)
        {
            return GUILayout.Toggle(state, content, style, GUILayout.MaxWidth(style.CalcSize(content).x));
        }

        private bool IsClicked(Rect rect)
        {
            return Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition);
        }
        
        private bool ShouldLog(BluLog log, int row)
        {
            var messageLower = log.MessageLower;

            if (_toolbarWindow.SearchStringPatterns != null &&
                _toolbarWindow.SearchStringPatterns.Any(pattern => !messageLower.Contains(pattern)))
            {
                CacheLogComparer(row, false);
                return false;
            }

            var hasPattern = true;
            for (var i = 0; i < _settings.Filters.Count; i++)
            {
                if (!_toolbarWindow.ToggledFilters[i])
                    continue;

                if (!_settings.Filters[i].Patterns.Any(pattern => messageLower.Contains(pattern)))
                {
                    hasPattern = false;
                    break;
                }
            }

            CacheLogComparer(row, hasPattern);
            return hasPattern;
        }

        #endregion Gets
        
        
        #region Sets
        
        private void SetDirtyComparer()
        {
            _cacheLogComparer.Clear();
        }

        private void SetDirtyLogs()
        {
            _cacheLog.Clear();
            _cacheLogCount = 0;
            _cacheIntArr = new int[50];
            _cacheLogArr = new BluLog[50];
            SetDirtyComparer();
        }
        
        #endregion Sets

        
        #region Actions

        private void CheckDirties()
        {
            if (_toolbarWindow.HasSetDirtyLogs)
                SetDirtyLogs();
            if (_toolbarWindow.HasSetDirtyComparer)
                SetDirtyComparer();
        }

        private void CalculateResizer()
        {
            var resizerY = _topPanelHeight;

            _cursorChangeRect = new Rect(0, resizerY - 2f, position.width, _configuration.ResizerHeight + 3f);

            EditorGUIUtility.AddCursorRect(_cursorChangeRect, MouseCursor.ResizeVertical);

            if (IsClicked(_cursorChangeRect))
                _isResizing = true;
            else if (Event.current.rawType == EventType.MouseUp)
                _isResizing = false;

            if (_isResizing)
            {
                _topPanelHeight = Event.current.mousePosition.y;
                _cursorChangeRect.Set(_cursorChangeRect.x, resizerY, _cursorChangeRect.width, _cursorChangeRect.height);
            }

            _topPanelHeight = Mathf.Clamp(_topPanelHeight, 
                                          _configuration.MinHeightOfTopAndBotton, 
                                          position.height - _configuration.MinHeightOfTopAndBotton);
        }

        private void CacheLogComparer(int row, bool value)
        {
            if (row < _cacheLogComparer.Count)
                _cacheLogComparer[row] = value;
            else
                _cacheLogComparer.Add(value);
        }

        private void InitVariables()
        {
            while (_toolbarWindow.ToggledFilters.Count < _settings.Filters.Count)
                _toolbarWindow.ToggledFilters.Add(false);
            _drawYPos = 0f;
        }

        private void PreProcessLogs()
        {
            _qtLogs = UnityLoggerServer.StartGettingLogs();

            _cacheLogCount = _qtLogs;

            // Filtering logs with ugly code
            int cntLogs = 0;
            int[] rows = GetCachedIntArr(_qtLogs);
            BluLog[] logs = GetCachedLogsArr(_qtLogs);
            int index = 0;
            int cacheLogComparerCount = _cacheLogComparer.Count;
            for (int i = 0; i < _qtLogs; i++)
            {
                // Ugly code to avoid function call 
                int realCount = _cacheLog.Count;
                BluLog log = null;
                if (i < _cacheLogCount && i < realCount)
                    log = _cacheLog[i];
                else
                    log = GetSimpleLog(i);

                // Ugly code to avoid function call 
                bool has = false;
                if (i < cacheLogComparerCount)
                    has = _cacheLogComparer[i];
                else
                    has = ShouldLog(log, i);
                if (has)
                {
                    cntLogs++;
                    rows[index] = i;
                    logs[index++] = log;
                }
            }

            _qtLogs = cntLogs;

            UnityLoggerServer.StopGettingsLogs();
        }

        #endregion Actions

    }

}
