using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class TweenExt
{
    public static TweenerBase DoMove(this Transform transform, Vector3 endValue, float duration)
    {
        var tweener = new Tweener_Vector3(() => transform.position, (v) => transform.position = v, endValue, duration);
        Tween.Instance.AddTweener(tweener);
        return tweener;
    }

    public static TweenerBase DoScale(this Transform transform, Vector3 endValue, float duration)
    {
        var tweener = new Tweener_Vector3(() => transform.localScale, (v) => transform.localScale = v, endValue, duration);
        Tween.Instance.AddTweener(tweener);
        return tweener;
    }
}

public class Tween : MonoBehaviour
{
    public static Tween Instance;

    private LinkedList<TweenerBase> _tweeners = new LinkedList<TweenerBase>();

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        var curr = _tweeners.First;
        while (curr != null)
        {
            var tweener = curr.Value;
            tweener.Update(Time.deltaTime);
            if (tweener.IsEnd())
                _tweeners.Remove(tweener);
            curr = curr.Next;
        }
    }

    public void AddTweener(TweenerBase tweener)
    {
        _tweeners.AddLast(tweener);
    }

    public void Kill(TweenerBase tweener)
    {
        _tweeners.Remove(tweener);
    }
}

public enum EaseType
{
    Linear,
    EaseInSine,
    EaseOutSine,
    EaseInOutSine,
    EaseInBack,
    EaseOutBack,
    EaseInOutBack,
}

public abstract class TweenerBase
{
    public TweenerBase(float duration)
    {
        _duration = duration;
    }

    protected float _timer = 0;
    protected float _duration;

    public virtual void Update(float deltaTime)
    {
        _timer += deltaTime;
        _timer = Mathf.Min(_timer, _duration);
    }

    public bool IsEnd()
    {
        return _timer >= _duration;
    }

    public void Kill()
    {
        Tween.Instance.Kill(this);
    }
}

public abstract class Tweener<T> : TweenerBase where T : struct
{
    public Tweener(Func<T> getter, Action<T> setter, T endValue, float duration) : base(duration)
    {                
        _getter = getter;
        _setter = setter;
        _easeType = EaseType.Linear;
        _startValue = _getter();
        _endValue = endValue;
    }

    protected float _timer = 0;
    protected float _duration;
    protected Func<T> _getter;
    protected Action<T> _setter;
    protected EaseType _easeType;
    protected T _startValue;
    protected T _endValue;

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        var v = GetValue(_timer / _duration);
        _setter?.Invoke(v);
    }

    public abstract T GetValue(float t);

    public void SetEaseType(EaseType easeType)
    {
        _easeType = easeType;
    }
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

    public override void Update(float deltaTime)
    {
        _xTweener.Update(deltaTime);
        _yTweener.Update(deltaTime);
        _zTweener.Update(deltaTime);
        base.Update(deltaTime);
    }
}
