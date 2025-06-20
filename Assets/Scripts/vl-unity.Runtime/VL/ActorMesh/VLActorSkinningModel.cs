using UnityEngine;
using UnityEngine.Rendering;
using VL.Core;
using VL.Rendering.Skinning;

namespace VL.ActorMesh
{
	public class VLActorSkinningModel : VLSkinningModel
	{
		public Transform rootBone; // 0x20
		public Transform[] bones; // 0x28
		public Bounds localBounds; // 0x30
		private VLActorSkinningSystem _system; // 0x48
		private BlendShapeWeight _blendShapeWeights; // 0x50
		
		// Properties
		public bool isSystemActive { get; }
		public Bounds bounds { get; }
		
		public override Transform GetRootBone()
		{
			throw new System.NotImplementedException();
		}

		public override Transform[] GetBones()
		{
			throw new System.NotImplementedException();
		}

		public override Bounds GetLocalBounds()
		{
			throw new System.NotImplementedException();
		}

		public override void SetupSkinning()
		{
			throw new System.NotImplementedException();
		}

		internal override void ScheduleSkinning()
		{
			throw new System.NotImplementedException();
		}

		internal override void ApplySkinning(CommandBuffer cmd)
		{
			throw new System.NotImplementedException();
		}
	}
}