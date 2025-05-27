using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityPMXExporter;
using VL.FaceSystem;
using Object = UnityEngine.Object;

public class ModelLoader : MonoBehaviour
{
	public CharacterConfigSO configSO;
	
	// Start is called before the first frame update
	public Object ShaderFile;
	//public Object FaceFile;
	//public Object HairFile;
	//public Object BodyFile;

	public List<Shader> ShaderList = new();

	public GameObject Body;
	public GameObject Face;
	public GameObject Hair;

	public List<Object> AssetHolder = new();

	public Transform ConnectBone;
	public Light DirectionalLight;

	[Button(nameof(Export), ButtonSizes.Medium, ButtonStyle.CompactBox)]
	private void Export()
	{
		AssetBundle.UnloadAllAssetBundles(true);
		for (var i = 0; i < configSO.Size; i++)
		{
			Export(configSO, i);
			ShaderList.Clear();
			AssetHolder.Clear();
			AssetBundle.UnloadAllAssetBundles(true);
		}
	}

	private void Export(ICharacterConfigSO config, int y)
	{
		(Object face, Object hair, Object body) = config[y];
		Export(face, hair, body);
	}

	private void Export(Object faceFile, Object hairFile, Object bodyFile)
	{
		if (AssetDatabase.GetAssetPath(ShaderFile) != "")
		{
			var shader_ab = AssetBundle.LoadFromFile(AssetDatabase.GetAssetPath(ShaderFile));
			ShaderList.AddRange(shader_ab.LoadAllAssets<Shader>());
		}
		else
		{
			Debug.Log($"ShaderFile {ShaderFile} is None !");
		}

		if (AssetDatabase.GetAssetPath(bodyFile) != "" && File.Exists(AssetDatabase.GetAssetPath(bodyFile)))
		{
			var body_ab = AssetBundle.LoadFromFile(AssetDatabase.GetAssetPath(bodyFile));
			foreach (var ab in body_ab.LoadAllAssets())
			{
				if (ab is GameObject go)
				{
					Body = Instantiate(go);
					Body.name = Body.name.Replace("(Clone)", string.Empty);
					ConnectBone = Body.transform.Find("Reference/Hips/Spine/Spine1/Spine2/Neck/Head");
				}
				AssetHolder.Add(ab);
			}
		}
		else
		{
			Debug.Log($"BodyFile {bodyFile} is None !");
		}

		if (AssetDatabase.GetAssetPath(faceFile) != "" && File.Exists(AssetDatabase.GetAssetPath(faceFile)))
		{
			var face_ab = AssetBundle.LoadFromFile(AssetDatabase.GetAssetPath(faceFile));
			foreach (var ab in face_ab.LoadAllAssets())
			{
				if (ab is GameObject go)
				{
					Face = Instantiate(go);
					Face.name = Face.name.Replace("(Clone)", string.Empty);
					var vl = Face.GetComponentInChildren<VLActorFaceModel>();
					var skinned = vl.gameObject.AddComponent<SkinnedMeshRenderer>();
					var mesh = vl.mesh;
					byte[] bonesPerVertex = new byte[mesh.vertexCount];
					for (int i = 0; i < bonesPerVertex.Length; i++)
					{
						bonesPerVertex[i] = 1;
					}
					BoneWeight1[] weights = new BoneWeight1[mesh.vertexCount];
					for (int i = 0; i < weights.Length; i++)
					{
						weights[i].boneIndex = 0;
						weights[i].weight = 1;
					}

					var bonesPerVertexArray = new NativeArray<byte>(bonesPerVertex, Allocator.Temp);
					var weightsArray = new NativeArray<BoneWeight1>(weights, Allocator.Temp);
					mesh.SetBoneWeights(bonesPerVertexArray, weightsArray);

					skinned.sharedMesh = vl.mesh;
					skinned.bones = vl.bones;
					mesh.bindposes = vl.bindposes;
					foreach (var bs in vl.blendShapes)
					{
						var del_ver = new Vector3[mesh.vertexCount];
						foreach (var ver in bs.blendShapeVertices)
						{
							del_ver[ver.vertIndex] = ver.position;
						}
						mesh.AddBlendShapeFrame(bs.blendShapeName, 1, del_ver, null, null);
					}
					skinned.localBounds = vl.localBounds;
					skinned.rootBone = vl.rootBone;
					skinned.materials = vl.sharedMaterials;
					if (ConnectBone)
					{
						Face.transform.SetParent(ConnectBone, false);
						skinned.bones[0] = ConnectBone;
						skinned.rootBone = ConnectBone;
					}
				}
				AssetHolder.Add(ab);
			}
		}
		else
		{
			Debug.Log($"FaceFile {faceFile} is None !");
		}

		if (AssetDatabase.GetAssetPath(hairFile) != "" && File.Exists(AssetDatabase.GetAssetPath(hairFile)))
		{
			var hair_ab = AssetBundle.LoadFromFile(AssetDatabase.GetAssetPath(hairFile));
			foreach (var ab in hair_ab.LoadAllAssets())
			{
				if (ab is GameObject go)
				{
					Hair = Instantiate(go);
					Hair.name = Hair.name.Replace("(Clone)", string.Empty);
					if (ConnectBone)
					{
						Hair.transform.SetParent(ConnectBone, false);
						SkinnedMeshRenderer skinned = Hair.GetComponentInChildren<SkinnedMeshRenderer>();
						skinned.bones[0] = ConnectBone;
						skinned.rootBone = ConnectBone;
					}
				}
				AssetHolder.Add(ab);
			}
		}
		else
		{
			Debug.Log($"HairFile {hairFile} is None !");
		}

		var chrName = bodyFile.name.Substring(8, 4);
		var subName = bodyFile.name.Substring(13, 4);
		var numName = bodyFile.name.Substring(18, 4);
		var hairSubName = hairFile.name.Substring(13, 4);
		var hairNumName = hairFile.name.Substring(18, 4);
		var hairName = hairFile.name.Substring(23, 4);
		var PMXPath = $"Assets/Export/{chrName}/{subName}/{numName}/{bodyFile.name}-{hairSubName}-{hairNumName}_{hairName}.pmx";
		var path = Path.GetDirectoryName(PMXPath);
		var name = Path.GetFileName(PMXPath);

		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		else
		{
			Debug.LogWarning($"Directory already exists: {path}");
		}

		if (!File.Exists(PMXPath) && Body && Face && Hair)
		{
			var outPath = string.IsNullOrEmpty(name) ? $"{path}/Model.pmx" : PMXPath;
			ModelExporter.ExportModel(Body, outPath, colorSpace: RenderTextureReadWrite.sRGB);
			Debug.Log($"Export model to {outPath}");
		}
		else
		{
			Debug.Log($"Export Failed \nDirectory Exists:{Directory.Exists(path)} \nBody:{Body} \nFace:{Face} \nHair:{Hair}");
		}
	}

	public Color MatCapParam;
	public Color MatCapRimColor;
	public Color VLSpecColor;
	public Color EyeHightlightColor;
	public Color FadeParam;
	public float OutlineWidth;
	public Vector4 MatCapRimLight;

	private void Start()
	{
		enabled = false;
	}

	void Update()
	{
		var dir = -DirectionalLight.transform.forward;
		Shader.SetGlobalFloat("_SkinSaturation", 1);
		Shader.SetGlobalColor("_MatCapLightColor", DirectionalLight.color);
		Shader.SetGlobalVector("_MatCapRimColor", MatCapRimColor);
		Shader.SetGlobalVector("_VLSpecColor", VLSpecColor);
		Shader.SetGlobalVector("_MatCapParam", MatCapParam);
		Shader.SetGlobalVector("_MatCapMainLight", dir);
		Shader.SetGlobalVector("_MatCapRimLight", MatCapRimLight);
		Shader.SetGlobalVector("_HeadDirection", Face.transform.forward);
		Shader.SetGlobalVector("_HeadUpDirection", Face.transform.up);
		Shader.SetGlobalVector("_EyeHightlightColor", EyeHightlightColor);
		Shader.SetGlobalVector("_OutlineParam", new Color(OutlineWidth, 0, 0, 0));
		Shader.SetGlobalVector("_FadeParam", FadeParam);
	}
}