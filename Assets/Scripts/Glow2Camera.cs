using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Glow2Camera : MonoBehaviour {
	private const float MIN_FACTOR = 0.055f;
	private const float MAX_FACTOR = 1.0f;
	private const int GLOW_OBJECT_LAYER_INDEX = 30;
	private const int GRAB_FRAME_COUNT = 2;

	public class GlowObject {
		public GameObject obj;
		public Material srcMaterial;
		public Material glowMaterial;
		public int layer;
		public GlowObject(GameObject _obj, Material smat, Material gmat, int _layer) {
			obj = _obj;
			srcMaterial = smat;
			glowMaterial = gmat;
			layer = _layer;
		}
	};
	public static List<GlowObject> GlowObjects = new List<GlowObject>();

    public Material GlowCameraMaterial;
    public Material GlowObjectMaterial;

	public float GlowStrength = 20.0f;
    public float BlurRadius = 3.0f;
    public float Factor = 1.0f;

	private float screenBlurFactor;
	private Texture2D[] maskShaderTexture;
	private int texIdx = 0;
    private Vector2 maskSize;
	private GameObject glowCamObj;
    private GameObject depthWriteCamObj;

    private int glowObjectLayer;

    private bool maskGrab = true;
	private bool useGlow = false;

	void Awake() {
		if (GlowCameraMaterial == null) {
			Debug.LogError("GlowCamera GameObject <" + gameObject.name + "> missing the GlowCameraShaderMaterial.");
			return;
		}

        glowObjectLayer = LayerMask.NameToLayer("GlowObject");
        if (glowObjectLayer == -1) {
			glowObjectLayer = GLOW_OBJECT_LAYER_INDEX;
			Debug.Log("Layer 'GlowObject': is not defined. Glow Object Layer is defined to " + GLOW_OBJECT_LAYER_INDEX);
		}

		useGlow = true;

        depthWriteCamObj = new GameObject("DepthWriteCamera");
        depthWriteCamObj.AddComponent<Camera>();
		depthWriteCamObj.GetComponent<Camera>().CopyFrom(gameObject.GetComponent<Camera>());
        depthWriteCamObj.GetComponent<Camera>().depth = -3;
        depthWriteCamObj.GetComponent<Camera>().clearFlags = CameraClearFlags.Skybox;
        depthWriteCamObj.transform.position = gameObject.GetComponent<Camera>().transform.position;
        depthWriteCamObj.transform.rotation = gameObject.GetComponent<Camera>().transform.rotation;
        depthWriteCamObj.transform.parent = gameObject.GetComponent<Camera>().gameObject.transform;
		depthWriteCamObj.GetComponent<Camera>().renderingPath = RenderingPath.VertexLit;
		depthWriteCamObj.GetComponent<Camera>().enabled = false;
        depthWriteCamObj.GetComponent<Camera>().pixelRect = new Rect(0, 0, Screen.width * Factor, Screen.height * Factor);

        glowCamObj = new GameObject("GlowCamera");
        glowCamObj.AddComponent<Camera>();
		glowCamObj.GetComponent<Camera>().CopyFrom(gameObject.GetComponent<Camera>());
        glowCamObj.GetComponent<Camera>().depth = -2;
        glowCamObj.GetComponent<Camera>().cullingMask = 1 << glowObjectLayer;  // only display glowObjectLayer
        glowCamObj.GetComponent<Camera>().clearFlags = CameraClearFlags.Nothing;
        glowCamObj.transform.position = gameObject.GetComponent<Camera>().transform.position;
        glowCamObj.transform.rotation = gameObject.GetComponent<Camera>().transform.rotation;
        glowCamObj.transform.parent = gameObject.GetComponent<Camera>().gameObject.transform;
		glowCamObj.GetComponent<Camera>().renderingPath = RenderingPath.VertexLit;
		//glowCamObj.GetComponent<Camera>().renderingPath = RenderingPath.Forward;
        glowCamObj.GetComponent<Camera>().enabled = false;
        glowCamObj.GetComponent<Camera>().pixelRect = new Rect(0, 0, Screen.width * Factor, Screen.height * Factor);

		screenBlurFactor = (float)Screen.width / 1024.0f * BlurRadius;

		setGlowMaskResolution();
		initGlowMaterials();

		initGlowObjects();
	}

	void initGlowObjects() {
        foreach (GameObject obj in Object.FindObjectsOfType(typeof(GameObject))) {
            if (obj.activeInHierarchy) {
				Renderer renderer = obj.GetComponent<Renderer>();
				if (renderer != null && renderer.material.GetTag("RenderEffect", false) == "Glow2") {
					Material gmat = Instantiate(GlowObjectMaterial) as Material;
					gmat.color = renderer.material.GetColor("_GlowColor");
					GlowObject glowObj = new GlowObject(obj, renderer.material, gmat, obj.layer);
					GlowObjects.Add(glowObj);
	                //Debug.Log(obj.name);
				}
            }
        }
	}

	public void AddGrowObject(GameObject obj) {
		Renderer renderer = obj.GetComponent<Renderer>();
		if (renderer != null && renderer.material.GetTag("RenderEffect", false) == "Glow2") {
			Material gmat = Instantiate(GlowObjectMaterial) as Material;
			gmat.color = renderer.material.GetColor("_GlowColor");
			GlowObject glowObj = new GlowObject(obj, renderer.material, gmat, obj.layer);
			GlowObjects.Add(glowObj);
		}
	}

	public void resetGlowObjects() {
		GlowObjects.Clear();
	}

    void OnPreRender() {
		if(!useGlow)
			return;

        if (maskGrab) {
			renderDepthWriteCam();

			setGlowMaterials();
			renderGlowCam();
			grabGlowMask();

			setSourceMaterials();
		}
	}

    private void setGlowMaskResolution() {
		Factor = Mathf.Clamp(Factor, MIN_FACTOR, MAX_FACTOR);
		maskShaderTexture = new Texture2D[GRAB_FRAME_COUNT];
		for (int i = 0; i < GRAB_FRAME_COUNT; i++) {
	        maskShaderTexture[i] = new Texture2D(nearestPowerOfTwo((int)(Screen.width * Factor)), nearestPowerOfTwo((int)(Screen.height * Factor)), TextureFormat.ARGB32, false);

	        maskShaderTexture[i].anisoLevel = 1;	// Min 1 Max 9
	        maskShaderTexture[i].filterMode = FilterMode.Bilinear;
	        maskShaderTexture[i].wrapMode = TextureWrapMode.Clamp;
		}
        maskSize = new Vector2((float)(Screen.width * Factor) / maskShaderTexture[texIdx].width, (float)(Screen.height * Factor) / maskShaderTexture[texIdx].height);

		GlowCameraMaterial.SetFloat("_SizeX", maskSize.x);
    	GlowCameraMaterial.SetFloat("_SizeY", maskSize.y);
    	GlowCameraMaterial.SetVector("_TexelSize", new Vector4(1.0f / maskShaderTexture[texIdx].width, 1.0f / maskShaderTexture[texIdx].height, 0, 0));
 
        glowCamObj.GetComponent<Camera>().pixelRect = new Rect(0, 0, Screen.width * Factor, Screen.height * Factor);
        depthWriteCamObj.GetComponent<Camera>().pixelRect = new Rect(0, 0, Screen.width * Factor, Screen.height * Factor);
	}

	private void initGlowMaterials() {
		GlowCameraMaterial.SetFloat("_GlowStrength", GlowStrength);
		GlowCameraMaterial.SetVector("_BlurOffsets", new Vector4(screenBlurFactor, screenBlurFactor, 0, 0));
	}

    private int nearestPowerOfTwo(int value) {
        var result = 1;
        while (result < value)
            result *= 2;
        return result;
    }

	private void renderDepthWriteCam() {
		depthWriteCamObj.GetComponent<Camera>().Render();
	}

	private void setGlowMaterials() {
		foreach (GlowObject glo in GlowObjects) {
    		if (glo.obj != null) {
				glo.obj.gameObject.GetComponent<Renderer>().material = glo.glowMaterial;
                glo.obj.layer = glowObjectLayer;
 			}
		}
	}

	private void renderGlowCam() {
		GL.Clear(false, true, Color.clear);  // clear only color buffer
  		glowCamObj.GetComponent<Camera>().Render();
	}

	private void grabGlowMask() {
   		maskShaderTexture[texIdx].ReadPixels(new Rect(0, 0, Screen.width * Factor, Screen.height * Factor), 0, 0, false);
		maskShaderTexture[texIdx].Apply(false, false);
  		maskGrab = false;
	}

	private void setSourceMaterials() {
		foreach (GlowObject glo in GlowObjects) {
			if (glo.obj != null) {
				glo.obj.gameObject.GetComponent<Renderer>().material = glo.srcMaterial;
				glo.obj.layer = glo.layer;
 			}
		}
	}

    void OnGUI() {
		if(!useGlow)
			return;

        if (Event.current.type.Equals(EventType.repaint)) {
			int iterCount = 2;
			for (int i = 0; i < GRAB_FRAME_COUNT; i++) {
				int idx = (texIdx + GRAB_FRAME_COUNT - i) % GRAB_FRAME_COUNT;
				//Debug.Log("idx:" + idx + " texIdx:" + texIdx);
				GlowCameraMaterial.SetTexture("_MaskTex", maskShaderTexture[idx]);
				float alpha = 0.0f;
				if (i != 0) {
					//alpha = 0.4f - (0.38f / (GRAB_FRAME_COUNT * GRAB_FRAME_COUNT)) * i * i;
					//alpha = 0.5f - (0.48f / (GRAB_FRAME_COUNT)) * i;
					alpha = 0.1f;
				}
				GlowCameraMaterial.color = new Color(alpha, alpha, alpha, 1.0f);
				for (int y = 1; y <= iterCount; y++) {
					for (int x = 1; x <= iterCount; x++) {
						float blurPow = 1.0f;
						float glowPow = 1.0f / iterCount;
						float xOff = 1.0f + (2.0f * (x - 1));
						float yOff = 1.0f + (2.0f * (y - 1));
						GlowCameraMaterial.SetVector("_BlurOffsets", new Vector4(screenBlurFactor * blurPow * xOff, screenBlurFactor * blurPow * yOff, 0, 0));
						GlowCameraMaterial.SetFloat("_GlowStrength", GlowStrength * glowPow);

						Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), maskShaderTexture[idx], new Rect(0, 0, maskSize.x, maskSize.y), 0, 0, 0, 0, GlowCameraMaterial);
					}
				}
			}
			initGlowMaterials();
			texIdx = (texIdx + 1) % GRAB_FRAME_COUNT;
	  		maskGrab = true;
			//GlowCameraMaterial.SetVector("_BlurOffsets", new Vector4(screenBlurFactor * 3, screenBlurFactor * 3, 0, 0));
			//Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), maskShaderTexture, new Rect(0, 0, maskSize.x, maskSize.y), 0, 0, 0, 0, GlowCameraMaterial);
		}
	}
}
