using UnityEngine;
using System.Collections;


public class IndividualSSAA
{

    public static RenderTexture rtBig = null;
    public static Material matBlit = null;
    // Use this for initialization
    public static void InitializeMe(Camera target)
    {
        if (rtBig == null)
        {
            matBlit = new Material(Shader.Find("Hidden/Blit"));
            rtBig = new RenderTexture(Screen.width * 2, Screen.height * 2, 24, RenderTextureFormat.Default);
            //rtBig.antiAliasing = 8;
            rtBig.filterMode = FilterMode.Bilinear;
        }

        if (target.GetComponent<IndividualSSAAProducer>() == null)
        {
            target.gameObject.AddComponent<IndividualSSAAProducer>();
            //target.clearFlags = CameraClearFlags.SolidColor;
        }
    }
}

public class IndividualSSAAProducer : MonoBehaviour
{
    RenderBuffer prevC, prevD;

    public Material matBloom = null;

    public int downsample = 2;
    public int iterate = 2;
    public float blurSize = 2.5f;

    public float threshold = 0.0f;
    public float oneminusthreshold = 1.0f;

    void OnPreRender()
    {
        prevC = Graphics.activeColorBuffer;
        prevD = Graphics.activeDepthBuffer;
        GetComponent<Camera>().targetTexture = IndividualSSAA.rtBig;
        Graphics.SetRenderTarget(IndividualSSAA.rtBig.colorBuffer, IndividualSSAA.rtBig.depthBuffer);

        this.matBloom = new Material(Shader.Find("Hidden/FastBloom"));
    }
    void OnPostRender()
    {
        if(true)
        {
            // bloom process here
            int rtW = 256;
            int rtH = 256;

            matBloom.SetVector ("_Parameter", new Vector4 (0, 0, threshold, oneminusthreshold));

            // downsample
            RenderTexture rtCopy = RenderTexture.GetTemporary (IndividualSSAA.rtBig.width, IndividualSSAA.rtBig.height, 0, IndividualSSAA.rtBig.format);
            Graphics.Blit(IndividualSSAA.rtBig, rtCopy);

            RenderTexture rt256 = RenderTexture.GetTemporary (256, 256, 0, IndividualSSAA.rtBig.format);
            rt256.filterMode = FilterMode.Bilinear;
            RenderTexture rt128 = RenderTexture.GetTemporary (128, 128, 0, IndividualSSAA.rtBig.format);
            rt128.filterMode = FilterMode.Bilinear;
            RenderTexture rt64 = RenderTexture.GetTemporary (64, 64, 0, IndividualSSAA.rtBig.format);
            rt64.filterMode = FilterMode.Bilinear;

            // rt -> lightCull
            Graphics.Blit( IndividualSSAA.rtBig, rt256, this.matBloom, 1 );


            int passOffs = 3;
            float widthMod = 1.0f;

            {
                matBloom.SetVector ("_Parameter", new Vector4 (blurSize * widthMod, -blurSize * widthMod, threshold, oneminusthreshold));
                RenderTexture tmp = RenderTexture.GetTemporary (256, 256, 0, IndividualSSAA.rtBig.format);
                tmp.filterMode = FilterMode.Bilinear;
                Graphics.Blit (rt256, tmp, matBloom, 1 + passOffs);
                Graphics.Blit (tmp, rt256, matBloom, 2 + passOffs);
                RenderTexture.ReleaseTemporary (tmp);
            }

            Graphics.Blit( rt256, rt128);

            {
                matBloom.SetVector ("_Parameter", new Vector4 (blurSize * widthMod, -blurSize * widthMod, threshold, oneminusthreshold));
                RenderTexture tmp = RenderTexture.GetTemporary (128, 128, 0, IndividualSSAA.rtBig.format);
                tmp.filterMode = FilterMode.Bilinear;
                Graphics.Blit (rt128, tmp, matBloom, 1 + passOffs);
                Graphics.Blit (tmp, rt128, matBloom, 2 + passOffs);
                RenderTexture.ReleaseTemporary (tmp);
            }

            Graphics.Blit( rt128, rt64);

            {
                matBloom.SetVector ("_Parameter", new Vector4 (blurSize * widthMod, -blurSize * widthMod, threshold, oneminusthreshold));
                RenderTexture tmp = RenderTexture.GetTemporary (64, 64, 0, IndividualSSAA.rtBig.format);
                tmp.filterMode = FilterMode.Bilinear;
                Graphics.Blit (rt64, tmp, matBloom, 1 + passOffs);
                Graphics.Blit (tmp, rt64, matBloom, 2 + passOffs);
                RenderTexture.ReleaseTemporary (tmp);
            }


            // downsample 256, 128, 64, 32
//            for(int i = 0; i < 2; i++) {
//                float iterationOffs = (i*1.0f);
//                matBloom.SetVector ("_Parameter", new Vector4 (blurSize * widthMod + iterationOffs, -blurSize * widthMod - iterationOffs, threshold, oneminusthreshold));
//
//                // vertical blur
//                RenderTexture rt2 = RenderTexture.GetTemporary (rtW, rtH, 0, IndividualSSAA.rtBig.format);
//                rt2.filterMode = FilterMode.Bilinear;
//                Graphics.Blit (rt, rt2, matBloom, 1 + passOffs);
//                RenderTexture.ReleaseTemporary (rt);
//                rt = rt2;
//
//                // horizontal blur
//                rt2 = RenderTexture.GetTemporary (rtW, rtH, 0, IndividualSSAA.rtBig.format);
//                rt2.filterMode = FilterMode.Bilinear;
//                Graphics.Blit (rt, rt2, matBloom, 2 + passOffs);
//                RenderTexture.ReleaseTemporary (rt);
//                rt = rt2;
//            }

            // merge
            matBloom.SetTexture("_Bloom256",rt256);
            matBloom.SetTexture("_Bloom128",rt128);
            matBloom.SetTexture("_Bloom64",rt64);
            Graphics.Blit(rtCopy, IndividualSSAA.rtBig, this.matBloom, 0);


            rt256.DiscardContents();
            rt128.DiscardContents();
            rt64.DiscardContents();
            rtCopy.DiscardContents();

            RenderTexture.ReleaseTemporary (rt256);
            RenderTexture.ReleaseTemporary (rt128);
            RenderTexture.ReleaseTemporary (rt64);
            RenderTexture.ReleaseTemporary (rtCopy);
        }


        // output


        GetComponent<Camera>().targetTexture = null;
        Graphics.SetRenderTarget(prevC, prevD);
        GL.Viewport(new Rect(0, 0, Screen.width, Screen.height));

        Graphics.Blit(IndividualSSAA.rtBig, IndividualSSAA.matBlit);
        IndividualSSAA.rtBig.DiscardContents();
    }
}

public class IndividualSSAAFlusher : MonoBehaviour
{
    void Start()
    {
        Application.targetFrameRate = 60;

//#if UNITY_EDITOR
        Camera camThis = GetComponent<Camera>();
        if(camThis != null)
        {
            IndividualSSAA.InitializeMe(camThis);
        }
//#endif
    }
}
