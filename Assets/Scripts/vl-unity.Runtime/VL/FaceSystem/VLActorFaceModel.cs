using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Rendering;
using VL.ActorMesh;
using VL.Core;
using VL.Rendering.Skinning;

namespace VL.FaceSystem
{
    public class VLActorFaceModel : VLSkinningModel
    {
        public const int CustomBlendShapeLength = 0; // 0x0
        public static readonly string CustomBlendShapeName; // 0x0
        public Mesh mesh;
        public Material[] sharedMaterials;
        public uint renderingLayerMask; // 0x30
        public Transform rootBone;
        public Transform[] bones;
        public Matrix4x4[] bindposes;
        public Bounds localBounds;
        public uint[] boneWeightAndIndices;
        public BlendShapeData[] blendShapes;
        public BlendShapeWeight blendShapeWeights;
        private VLActorSkinningSystem _system; // 0x378
        private Dictionary<string, BlendShapeData> _blendShapeNameTable; // 0x380
        private Dictionary<string, BlendShapeData> _geometryBlendShapeNameTable; // 0x388
        private VLActorFaceBindVertex[] _bindVertices; // 0x390
        private TransformAccessArray _bindVertexTransformAccessArray; // 0x398
        private NativeArray<Vector2> _bindVertexArray2DInfos; // 0x3a0
        private NativeArray<Vector3> _bindVertexBasePositions; // 0x3b0
        private NativeArray<Vector4> _bindVertexData; // 0x3c0
        
        // Properties
        public bool isSystemActive { get; }
        public Bounds bounds { get; }
        public int blendShapeLength { get; }
        public IReadOnlyList<VLActorFaceBindVertex> bindVertices { get; }
        
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

