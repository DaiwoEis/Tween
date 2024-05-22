using System.Collections.Generic;
using UnityEngine;

public class TestTween : MonoBehaviour
{
    public GameObject item1;
    public GameObject item2;

    public GameObject item3;
    public GameObject item4;

    private void Start()
    {
        TweenManager.Init();
    }

    private EaseType _selectType = EaseType.EaseInOutBack;
    private string[] _typeNames = new[] { "Linear", "EaseInSine", "EaseOutSine", "EaseInOutSine", "EaseInBack", "EaseOutBack", "EaseInOutBack" };

    private List<TweenerBase> _tweens = new List<TweenerBase>();

    private void OnGUI()
    {
        _selectType = (EaseType)GUI.Toolbar(new Rect { x = 100, y = 50, width = _typeNames.Length * 100, height = 50 }, (int)_selectType, _typeNames);

        if (GUI.Button(new Rect { x = 100, y = 100, width = 200, height = 50 }, "Tween"))
        {
            for (int i = 0; i < _tweens.Count; ++i)
                _tweens[i].Kill();
            _tweens.Clear();

            item1.transform.position = new Vector3(-5, 2, 0);
            _tweens.Add(item1.transform.DoMove(new Vector3(5, 2, 0), 1f).SetEaseType(EaseType.Linear));

            item2.transform.position = new Vector3(-5, 0, 0);
            _tweens.Add(item2.transform.DoMove(new Vector3(5, 0, 0), 1f).SetEaseType(_selectType));

            item3.transform.localScale = Vector3.one;
            _tweens.Add(item3.transform.DoScale(new Vector3(2, 2, 2), 1f).SetEaseType(EaseType.Linear));

            item4.transform.localScale = Vector3.one;
            _tweens.Add(item4.transform.DoScale(new Vector3(2, 2, 2), 1f).SetEaseType(_selectType));
        }

        if (GUI.Button(new Rect { x = 100, y = 150, width = 200, height = 50 }, "Kill"))
        {            
            for (int i = 0; i < _tweens.Count; ++i)
                _tweens[i].Kill();
            _tweens.Clear();
        }
    }
}
