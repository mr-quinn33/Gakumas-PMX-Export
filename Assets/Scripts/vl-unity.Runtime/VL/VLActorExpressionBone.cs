using System;
using UnityEngine;

namespace VL
{
	public class VLActorExpressionBone : MonoBehaviour
	{
		// Fields
		private Transform _referenceBone; // 0x20
		private string _referenceActorBoneName; // 0x28
		private float _coefficient; // 0x3c
		private float _min; // 0x40
		private float _max; // 0x44
		private bool _useAxisMask; // 0x48
		private bool _useCustomOutput; // 0x59
		private string _lispText; // 0x68
		public float init; // 0x70
		private HumanPartDof _humanPartDof; // 0x74
		private ArmDof _armDof; // 0x78
		private int _muscleAxisType; // 0x7c
	}
}