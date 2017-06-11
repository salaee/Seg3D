using UnityEngine;
using System.Collections;
using Intel.RealSense;
using Intel.RealSense.Segmentation;

public class Seg : MonoBehaviour
{

    [Header("Seg3D Settings")]
    public int segWidth = 1280;
    public int segHeight = 720;
    public float segFPS = 30f;
    public Material SegMaterial;

    private SenseManager sm = null;
    private Seg3D seg = null;
    private NativeTexturePlugin texPlugin = null;

    private System.IntPtr segTex2DPtr = System.IntPtr.Zero;

    void OnFrameProcessed(System.Object sender, FrameProcessedEventArgs args)
    {
        Seg3D s = (Seg3D)sender;
        if (s != null)
        {
            Image image = s.AcquireSegmentedImage();
            if (image != null)
            {
                Debug.Log("Color = " + image.Info.width + "x" + image.Info.height);
                texPlugin.UpdateTextureNative(image, segTex2DPtr);
            }
            else
            {
                Debug.Log("Image is null.");
            }
        }

    }

    // Use this for initialization
    void Start()
    {

        /* Create SenseManager Instance */
        sm = SenseManager.CreateInstance();

        /* Selecting a higher resolution profile*/
        StreamProfileSet profiles = new StreamProfileSet();
        profiles.color.imageInfo.width = 1280;
        profiles.color.imageInfo.height = 720;
        RangeF32 f_rate = new RangeF32(30, 30);
        profiles.color.frameRate = f_rate;
        profiles.depth.imageInfo.width = 640;
        profiles.depth.imageInfo.height = 480;
        RangeF32 f_drate = new RangeF32(30, 30);
        profiles.depth.frameRate = f_drate;

        /* Setting the resolution profile */
        sm.CaptureManager.FilterByStreamProfiles(profiles);

        /* Enable and Get a segmentation instance here for configuration */
        seg = Seg3D.Activate(sm);

        /* Subscribe to seg arrived event */
        seg.FrameProcessed += OnFrameProcessed;

        /* Initialize pipeline */
        sm.Init();

        /* Create NativeTexturePlugin to render Texture2D natively */
        texPlugin = NativeTexturePlugin.Activate();

        SegMaterial.mainTexture = new Texture2D(segWidth, segHeight, TextureFormat.BGRA32, false); // Update material's Texture2D with enabled image size.
        SegMaterial.mainTextureScale = new Vector2(-1, -1); // Flip the image
        segTex2DPtr = SegMaterial.mainTexture.GetNativeTexturePtr();// Retrieve native Texture2D Pointer

        /* Start Streaming */
        sm.StreamFrames(false);

    }

    // Use this for clean up
    void OnDisable()
    {

        /* Clean Up */
        seg.FrameProcessed -= OnFrameProcessed;

        if (sm != null) sm.Dispose();
    }

}