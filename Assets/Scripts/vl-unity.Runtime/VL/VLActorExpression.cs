using System;
using UnityEngine.Animations;

namespace VL
{
	public interface IAnimationPlayer
	{
	}
	
	public class VLActorExpression : VLActorExtension
	{
		// Fields
		public const int ExpressionCapacity = 0; // 0x0
		public const int MuscleExpressionCapacity = 0; // 0x0
		public const int SymbolCapacity = 0; // 0x0
		private VLActorExpressionBone[] _expressionBones; // 0x28
		private AnimationScriptPlayable _scriptPlayable; // 0x30
		private IAnimationPlayer _animationPlayer; // 0x40
		private AnimationPlayableOutput _output; // 0x68
	}
}