using System;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

namespace VL.Core
{
	public interface IVLActorController
	{
	}

	public interface IVLTransform
	{
	}
	
	public class VLActorController : MonoBehaviour, IVLActorController, IVLTransform
	{
		// Fields
		private static List<VLActorController> _actorControllers; // 0x10
		protected int _index; // 0x20
		protected Animator _animator; // 0x28
		protected Avatar _avatar; // 0x30
		protected List<VLActorController> _children; // 0x40
		protected GameObject[] _buildResources; // 0x60
		protected Bounds _bounds; // 0x68
		protected bool _isDestroyed; // 0x80
		private JobHandle _jobHandle; // 0x88
	}
}