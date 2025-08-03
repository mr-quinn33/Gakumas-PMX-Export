using UnityEngine;
using UnityEngine.Rendering;

namespace VL.Rendering.Skinning
{
	public abstract class VLSkinningModel : MonoBehaviour
	{
		public abstract Transform GetRootBone();

		public abstract Transform[] GetBones();

		public abstract Bounds GetLocalBounds();

		public abstract void SetupSkinning();

		internal abstract void ScheduleSkinning();

		internal abstract void ApplySkinning(CommandBuffer cmd);
	}
}