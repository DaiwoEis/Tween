using System.Collections.Generic;
using UnityEngine;
using System;

public static class TweenExt
{
    public static TweenerBase DoMove(this Transform transform, Vector3 endValue, float duration)
    {
        var tweener = new Tweener_Vector3(() => transform.position, (v) => transform.position = v, endValue, duration);
        TweenManager.GetInstance().AddTweener(tweener);
        return tweener;
    }

    public static TweenerBase DoScale(this Transform transform, Vector3 endValue, float duration)
    {
        var tweener = new Tweener_Vector3(() => transform.localScale, (v) => transform.localScale = v, endValue, duration);
        TweenManager.GetInstance().AddTweener(tweener);
        return tweener;
    }
}

public class TweenManager : MonoBehaviour
{
    public static TweenManager _instance;

    private List<TweenerBase> _tweeners = new List<TweenerBase>();

    public static void Init()
    {
        if (_instance != null)
            return;
        var go = new GameObject("TweenManager");
        _instance = go.AddComponent<TweenManager>() as TweenManager;       
    }

    public static TweenManager GetInstance()
    {
        if (_instance == null)
        {
            Debug.LogError("tween not init");
        }
        return _instance;
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private List<TweenerBase> _cache = new List<TweenerBase>();

    private void Update()
    {
        _cache.Clear();
        foreach (var tweener in _tweeners)
        {
            _cache.Add(tweener);
        }
        foreach (var tweener in _cache)
        {
            tweener.Update();
        }
        _cache.Clear();
        for (int i = 0; i < _tweeners.Count;)
        {
            var tweener = _tweeners[i];
            if (tweener.IsEnd())
            {
                var last = _tweeners[_tweeners.Count - 1];
                _tweeners[i] = last;
                _tweeners.RemoveAt(_tweeners.Count - 1);
            }
            else
            {
                ++i;
            }
        }
    }

    public void AddTweener(TweenerBase tweener)
    {
        tweener.SetTweenManager(this);
        _tweeners.Add(tweener);
    }

    public void Kill(TweenerBase tweener)
    {
        _tweeners.Remove(tweener);        
    }

    public void KillAll()
    {
        _tweeners.Clear();
    }

    public void Destroy()
    {           
        Destroy(gameObject);        
    }

    private void OnDestroy()
    {
        KillAll();
        _instance = null;
    }
}

public enum EaseType
{
    Linear = 0,
    EaseInSine = 1,
    EaseOutSine = 2,
    EaseInOutSine = 3,
    EaseInBack = 4,
    EaseOutBack = 5,
    EaseInOutBack = 6,
}

public abstract class TweenerBase
{
    public TweenerBase(float duration)
    {
        _duration = duration;
        _easeType = EaseType.Linear;
        _startTime = Time.time;
    }

    protected float _startTime;
    protected float _timer;
    protected float _duration;
    protected EaseType _easeType;

    public virtual void Update()
    {
        _timer = Time.time - _startTime;
        _timer = Mathf.Min(_timer, _duration);
    }

    public bool IsEnd()
    {
        return _timer >= _duration;
    }

    private TweenManager _tweenManager;

    public void SetTweenManager(TweenManager tweenManager)
    {
        _tweenManager = tweenManager;
    }

    public void Kill() 
    { 
        if (_tweenManager != null) 
        {
            _tweenManager.Kill(this);
        }        
    }

    public virtual TweenerBase SetEaseType(EaseType easeType)
    {
        _easeType = easeType;
        return this;
    }
}

public abstract class Tweener<T> : TweenerBase where T : struct
{
    public Tweener(Func<T> getter, Action<T> setter, T endValue, float duration) : base(duration)
    {                
        _getter = getter;
        _setter = setter;
        _startValue = _getter();
        _endValue = endValue;
    }

    protected Func<T> _getter;
    protected Action<T> _setter;
    protected T _startValue;
    protected T _endValue;

    public override void Update()
    {
        base.Update();
        var v = GetValue(_timer / _duration);
        _setter?.Invoke(v);
    }

    public abstract T GetValue(float t);
}

public class Tweener_Float : Tweener<float>
{
    public Tweener_Float(Func<float> getter, Action<float> setter, float endValue, float duration) : base(getter, setter, endValue, duration) { }

    public override float GetValue(float t)
    {
        switch (_easeType)
        {
            case EaseType.Linear:
                return _Linear(t);
            case EaseType.EaseInSine:
                return _Linear(_EaseInSine(t));
            case EaseType.EaseOutSine:
                return _Linear(_EaseOutSine(t));
            case EaseType.EaseInOutSine:
                return _Linear(_EaseInOutSine(t));
            case EaseType.EaseOutBack:
                return _Linear(_EaseOutBack(t));
            case EaseType.EaseInOutBack:
                return _Linear(_EaseInOutBack(t));
            case EaseType.EaseInBack:
                return _Linear(_EaseInBack(t));
            default:
                return _Linear(t);
        }
    }

    private float _Linear(float t)
    {
        return _startValue + (_endValue - _startValue) * t;
    }

    private float _EaseInSine(float t)
    {
        return 1 - Mathf.Cos((t * Mathf.PI) / 2);
    }

    private float _EaseOutSine(float t)
    {
        return Mathf.Sin((t * Mathf.PI) / 2);
    }

    private float _EaseInOutSine(float t)
    {
        return -(Mathf.Cos(Mathf.PI * t) - 1) / 2;
    }

    private float _EaseInBack(float t)
    {
        var c1 = 1.70158f;
        var c3 = c1 + 1;

        return c3 * t * t * t - c1 * t * t;
    }

    private float _EaseOutBack(float t)
    {
        var c1 = 1.70158f;
        var c3 = c1 + 1;

        return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
    }

    private float _EaseInOutBack(float t)
    {
        var c1 = 1.70158f;
        var c2 = c1 * 1.525f;

        return t < 0.5
          ? (Mathf.Pow(2 * t, 2) * ((c2 + 1) * 2 * t - c2)) / 2
          : (Mathf.Pow(2 * t - 2, 2) * ((c2 + 1) * (t * 2 - 2) + c2) + 2) / 2;
    }
}

public class Tweener_Vector3 : Tweener<Vector3>
{
    public Tweener_Vector3(Func<Vector3> getter, Action<Vector3> setter, Vector3 endValue, float duration) : base(getter, setter, endValue, duration)
    {
        _xTweener = new Tweener_Float(() => _startValue.x, null, _endValue.x, duration);
        _yTweener = new Tweener_Float(() => _startValue.y, null, _endValue.y, duration);
        _zTweener = new Tweener_Float(() => _startValue.z, null, _endValue.z, duration);
    }

    private Tweener_Float _xTweener;
    private Tweener_Float _yTweener;
    private Tweener_Float _zTweener;

    public override Vector3 GetValue(float t)
    {
        Vector3 v;
        v.x = _xTweener.GetValue(t);
        v.y = _yTweener.GetValue(t);
        v.z = _zTweener.GetValue(t);
        return v;
    }

    public override void Update()
    {
        _xTweener.Update();
        _yTweener.Update();
        _zTweener.Update();
        base.Update();
    }

    public override TweenerBase SetEaseType(EaseType easeType)
    {
        _xTweener.SetEaseType(easeType);
        _yTweener.SetEaseType(easeType);
        _zTweener.SetEaseType(easeType);
        return this;
    }
}
