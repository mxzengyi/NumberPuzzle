using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum TweenTargetType
{
    Pos,
    Rotation,
    Scale,
    Color,
    Alpha,
}

public enum EndActionType
{
    None,
    Deactive,
    Destory,
}

public class Tweener : MonoBehaviour {

    public float Delay;

    public float Duration;

    public TweenTargetType target;

    public EndActionType EndAction;

    public Vector3 Src;

    public Vector3 Des;

    private RectTransform _transform;

    private float _lifetime=0;
	// Use this for initialization
	void Start () {
        _transform = transform as RectTransform;
	}
	
    public void Set(float delay, float duration)
    {
        Delay = delay;
        Duration = duration;
        _lifetime = 0;
    }


    // Update is called once per frame
    void Update()
    {
        _lifetime += Time.deltaTime;

        if (_lifetime < Delay)
        {
            return;
        }


        if (_lifetime > (Delay + Duration))
        {
            switch (EndAction)
            {
                case EndActionType.Deactive:
                    gameObject.SetActive(false);
                    break;
                case EndActionType.Destory:
                    GameObject.Destroy(gameObject);
                    break;
                default:
                    break;
            }
        }

        Vector3 value = Src + (Des - Src) * (_lifetime - Delay) / Duration;

        switch (target)
        {
            case TweenTargetType.Pos:
                _transform.localPosition = value;
                break;
            case TweenTargetType.Rotation:
                _transform.localEulerAngles = value;
                break;
            case TweenTargetType.Scale:
                _transform.localScale = value;
                break;
            case TweenTargetType.Color:
                gameObject.GetComponent<Graphic>().color = new Color(value.x,value.y,value.z,gameObject.GetComponent<Graphic>().color.a);
                break;
            case TweenTargetType.Alpha:
                Color color = gameObject.GetComponent<Graphic>().color;
                gameObject.GetComponent<Graphic>().color = new Color(color.r, color.g, color.b, value.x);
                break;
            default:
                break;
        }

	}
}
