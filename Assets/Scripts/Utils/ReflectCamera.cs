using UnityEngine;
using System.Collections;

public class ReflectCamera : MonoBehaviour
{

    public Camera SourceCamera;

    public RenderTexture targetRT;

    public int downsample = 2;

    public int iterate = 2;

    public float blurSize = 1.0f;

    public Material blurMaterial = null;



    // Use this for initialization
	void Start () {

	    this.targetRT = new RenderTexture(Screen.width, Screen.height, 16);

	    gameObject.GetComponent<Camera>().targetTexture = this.targetRT;

	    this.blurMaterial = new Material(Shader.Find("Hidden/FastBlur"));
	}
	
	// Update is called once per frame
	void Update ()
	{

	    Vector3 sourceDir = this.SourceCamera.transform.forward;
	    Vector3 sourcePos = this.SourceCamera.transform.position;

	    Plane base0 = new Plane( Vector3.up, 0 );

	    Vector3 targetDir = Vector3.Reflect(sourceDir, Vector3.up);
	    Vector3 targetPos = Vector3.Reflect(sourcePos, Vector3.up);

	    transform.forward = targetDir;
	    transform.position = targetPos;

	    //Blur();

	    Shader.SetGlobalTexture("ReflectionTex", this.targetRT);

	}

    void OnPostRender()
    {
        Blur();
    }

    void Blur()
    {
        float widthMod = 1.0f / (1.0f * (1<<downsample));


        RenderTexture source = this.targetRT;

        blurMaterial.SetVector ("_Parameter", new Vector4 (blurSize * widthMod, -blurSize * widthMod, 0.0f, 0.0f));
        source.filterMode = FilterMode.Bilinear;

        int rtW = source.width >> downsample;
        int rtH = source.height >> downsample;

        // downsample
        RenderTexture rt = RenderTexture.GetTemporary (rtW, rtH, 0, source.format);

        rt.filterMode = FilterMode.Bilinear;
        Graphics.Blit (source, rt, blurMaterial, 0);

        var passOffs=2;

        for(int i = 0; i < iterate; i++) {
            float iterationOffs = (i*1.0f);
            blurMaterial.SetVector ("_Parameter", new Vector4 (blurSize * widthMod + iterationOffs, -blurSize * widthMod - iterationOffs, 0.0f, 0.0f));

            // vertical blur
            RenderTexture rt2 = RenderTexture.GetTemporary (rtW, rtH, 0, source.format);
            rt2.filterMode = FilterMode.Bilinear;
            Graphics.Blit (rt, rt2, blurMaterial, 1 + passOffs);
            RenderTexture.ReleaseTemporary (rt);
            rt = rt2;

            // horizontal blur
            rt2 = RenderTexture.GetTemporary (rtW, rtH, 0, source.format);
            rt2.filterMode = FilterMode.Bilinear;
            Graphics.Blit (rt, rt2, blurMaterial, 2 + passOffs);
            RenderTexture.ReleaseTemporary (rt);
            rt = rt2;
        }

        Graphics.Blit (rt, source);

        RenderTexture.ReleaseTemporary (rt);
    }
}
