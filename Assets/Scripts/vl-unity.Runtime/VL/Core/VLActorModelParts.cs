using System;
using System.Collections.Generic;
using UnityEngine;
using VL.Rendering.Skinning;

namespace VL.Core
{
	public class VLActorModelParts : MonoBehaviour
	{
		// Fields
		public const int TransformCapacity = 0; // 0x0
		public const int RendererCapacity = 0; // 0x0
		public List<Renderer> renderers; // 0x58
		public List<GameObject> rendererGameObjects; // 0x60
		public List<Renderer> skinRenderers; // 0x68
		public VLSkinningModel skinningModel; // 0x70
		public List<Material> materials; // 0x78
		public List<Transform> aabbTransform; // 0x88
		public Bounds defaultBounds; // 0x98
		public Transform defaultRootTransform; // 0xb0
		protected List<Transform> _tempRootTransBoneList; // 0x90
		protected bool _rendererActive; // 0x50
		private string _assetName; // 0x20
		private string _partsId; // 0x28
		private List<Transform> _rootBoneLinkList; // 0x48
		
		// Properties
		public string assetName { get; set; }
		public string partsId { get; set; }
		public int boneCount { get; }
		public bool rendererActive { get; }
	}

	[Serializable]
	public class VLActorModelParts<T> : VLActorModelParts
	{
	}
}