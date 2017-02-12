using UnityEngine;
using System.Collections;

public class simpleGymCam : MonoBehaviour
{
    public static float deltaX = 0;
    public static float deltaY = 0;
    public static bool rotating = false;

    public float sensetive = 0.01f;

    public float LimitY_Base = 0;
    public float LimitY_Top = 30;

    public float LerpingArea = 5;

    // Use this for initialization
    void Start ()
    {

        cachedY = transform.localEulerAngles.x;


    }

    float reMapEuler(float euler)
    {
        euler = euler - 360 * Mathf.Floor(euler / 360);
        if (euler > 180)
        {
            euler = euler - 360;
        }
        return euler;
    }

    private float cachedY = 0;

	// Update is called once per frame
	void Update ()
	{
	    if (rotating)
	    {
	        float x = deltaX * sensetive;
	        float y = deltaY * sensetive;

	        transform.Rotate(new Vector3(0, 1, 0), x, Space.World);
	        //transform.Rotate(new Vector3(1, 0, 0), -y, Space.Self);

	        cachedY -= y;

            // clamp to calc
	        cachedY = Mathf.Clamp(cachedY, this.LimitY_Base, this.LimitY_Top);

	        var limitMiddle = (this.LimitY_Top + this.LimitY_Base) * 0.5f;
	        var value = (this.cachedY - limitMiddle) / ((this.LimitY_Top - this.LimitY_Base) * 0.5f);
            // more cliff
            //value = Mathf.Sign(value) * Mathf.Pow(Mathf.Abs(value), 2.0f);
	        value *= 0.5f * Mathf.PI;
            value = Mathf.Clamp(value, -0.5f * Mathf.PI, 0.5f * Mathf.PI);



            var dist = Mathf.Sin(value);

	        dist *= (this.LimitY_Top - this.LimitY_Base) * 0.5f + limitMiddle;

            Vector3 euler = transform.localEulerAngles;
	        euler.x = dist;
	        transform.localEulerAngles = euler;
	    }
	}
}
