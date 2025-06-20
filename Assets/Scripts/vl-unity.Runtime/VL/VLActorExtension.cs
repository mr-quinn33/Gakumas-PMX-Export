using System;
using UnityEngine;
using VL.Core;

namespace VL
{
	public class VLActorExtension : MonoBehaviour, IComparable<VLActorExtension>, IDisposable
	{
		// Fields
		private VLActorController _controller; // 0x20

		// Properties
		protected VLActorController controller { get; }
		public virtual int order { get; }
		public virtual bool runtimeOnly { get; }
		
		public int CompareTo(VLActorExtension other)
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			// TODO release managed resources here
		}
	}
}