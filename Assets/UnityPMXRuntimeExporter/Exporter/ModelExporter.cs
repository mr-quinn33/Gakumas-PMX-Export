using System;
using LibMMD.Material;
using LibMMD.Model;
using LibMMD.Reader;
using LibMMD.Writer;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static LibMMD.Model.Morph;
using static LibMMD.Model.SkinningOperator;

namespace UnityPMXExporter
{
    public static class ModelExporter
    {
        public static void ExportModel(GameObject target, string path, PMXModelConfig exportConfig = new PMXModelConfig(), RenderTextureReadWrite colorSpace = RenderTextureReadWrite.Default)
        {
            
            if(string.IsNullOrEmpty(exportConfig.Name) && string.IsNullOrEmpty(exportConfig.NameEn))
            {
                exportConfig = new PMXModelConfig(target);
            }

            var textures = TextureExporter.ExportAllTexture(Path.GetDirectoryName(path), target.gameObject, colorSpace);
            var model = ReadPMXModelFromGameObject(target, textures, exportConfig);

            FileStream fileStream = new FileStream(path, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(fileStream);

            var writeConfig = new ModelConfig() { GlobalToonPath = "Toon" };
            PMXWriter.Write(writer, model, writeConfig);

            writer.Close();
            fileStream.Close();

            Debug.Log($"PMX Save at {path}");
        }

        private static RawMMDModel ReadPMXModelFromGameObject(GameObject target, string[] textures, PMXModelConfig config)
        {
            var model = new RawMMDModel();

            model.Name = config.Name;
            model.NameEn = config.NameEn;
            model.Description = config.Description;
            model.DescriptionEn = config.DescriptionEn;

            //Read Bones
            Transform rootBone = target.transform;
            //var bones = new List<Transform>(rootBone.GetComponentsInChildren<Transform>());
            var bones = new List<Transform>(GetChildrenTransform(rootBone, ExcludeChildrenTransform));

            //Read vertices And triangles
            var renderers = new List<Renderer>(target.GetComponentsInChildren<Renderer>());
            var triangles = new List<int>();
            model.Vertices = ReadVerticesAndTriangles(renderers, bones, ref triangles, target.transform);
            model.TriangleIndexes = triangles.ToArray();

            //Read Texture reference
            foreach (var texture in textures)
            {
                model.TextureList.Add(new MMDTexture(texture));
            }

            model.Parts = ReadPartMaterials(renderers, model);
            model.Bones = ReadMMDBones(bones, rootBone);
            model.Morphs = ReadMorph(renderers);
            model.Rigidbodies = Array.Empty<MMDRigidBody>();
            model.Joints = Array.Empty<MMDJoint>();

            return model;
        }

        private static Transform CreateNewRootTransform(Transform root)
        {
            var ikLeftFoot = root.Find("IKGoal_LeftFoot");
            var ikLeftHand = root.Find("IKGoal_LeftHand");
            var ikRightFoot = root.Find("IKGoal_RightFoot");
            var ikRightHand = root.Find("IKGoal_RightHand");
            var hips = root.Find("Reference/Hips");
            var pelvis = hips.Find("Pelvis");
            var leftLeg = pelvis.Find("LeftUpLeg/LeftLeg");
            var rightLeg = pelvis.Find("RightUpLeg/RightLeg");
            var center = new GameObject("Center").transform;
            center.position = 0.25f * leftLeg.position + 0.25f * rightLeg.position + 0.5f * pelvis.position;
            center.rotation = Quaternion.identity;
            root.DetachChildren();
            ikLeftFoot.SetParent(center);
            ikLeftHand.SetParent(center);
            ikRightFoot.SetParent(center);
            ikRightHand.SetParent(center);
            hips.SetParent(center);
            return center;
        }

        public static Vector3[] Vec4ToVec3(Vector4[] vector4s)
        {
            Vector3[] tmp = new Vector3[vector4s.Length];
            for (int i = 0; i < vector4s.Length; i++)
            {
                tmp[i] = new Vector3(vector4s[i].x, vector4s[i].y, vector4s[i].z);
            }
            return tmp;
        }

        public static Vector3[] CalDelta(Vector3[] ori, Vector3[] end)
        {
            Vector3[] tmp = new Vector3[ori.Length];
            for (int i = 0; i < ori.Length; i++)
            {
                tmp[i] = end[i] - ori[i];
            }
            return tmp;
        }

        private static Morph[] ReadMorph(List<Renderer> renderers)
        {
            List<Morph> morphs = new List<Morph>();
            int vertexOffset = 0;
            foreach (Renderer renderer in renderers)
            {
                Mesh mesh;
                if (renderer is MeshRenderer mr)
                {
                    var meshfilter = mr.GetComponent<MeshFilter>();
                    mesh = meshfilter.mesh;
                }
                else
                {
                    mesh = ((SkinnedMeshRenderer)renderer).sharedMesh;
                }
                var vertexCount = mesh.vertexCount;

                for (int i = 0; i < mesh.blendShapeCount; i++)
                {
                    var deltaVertices = new Vector3[vertexCount];
                    var deltaNormals = new Vector3[vertexCount];
                    var deltaTangents = new Vector3[vertexCount];
                    mesh.GetBlendShapeFrameVertices(i, 0, deltaVertices, deltaNormals, deltaTangents);

                    Morph morph = new Morph();
                    morph.Name = morph.NameEn = mesh.GetBlendShapeName(i);
                    morph.Type = MorphType.MorphTypeVertex;
                    var datas = new VertexMorphData[vertexCount];
                    for (int j = 0; j < vertexCount; j++)
                    {
                        var data = new VertexMorphData();
                        data.VertexIndex = vertexOffset + j;
                        data.Offset = deltaVertices[j];
                        datas[j] = data;
                    }
                    morph.MorphDatas = datas;
                    morph.Category = MorphCategory.MorphCatOther;
                    morphs.Add(morph);
                }

                vertexOffset += mesh.vertexCount;
            }
            return morphs.ToArray();
        }

        private static readonly Dictionary<string, string> mmdBoneMap = new() 
        {
            {"IKGoal_LeftFoot", "左足ＩＫ"},
            {"IKGoal_RightFoot", "右足ＩＫ"},
            {"IKGoal_LeftHand", "左手首ＩＫ"},
            {"IKGoal_RightHand", "右手首ＩＫ"},
            {"Reference", "センター"},
            {"Hips", "腰"},
            {"Spine", "上半身"},
            {"Spine1", "上半身１"},
            {"Spine2", "上半身２"},
            {"Neck", "首"},
            {"Head", "頭"},
            {"LeftEye", "左目"},
            {"RightEye", "右目"},
            {"LeftShoulder", "左肩"},
            {"LeftArm", "左腕"},
            {"LeftForeArm", "左ひじ"},
            {"LeftHand", "左手首"},
            {"LeftHandThumb1", "左親指０"},
            {"LeftHandThumb2", "左親指１"},
            {"LeftHandThumb3", "左親指２"},
            {"LeftHandIndex1", "左人指１"},
            {"LeftHandIndex2", "左人指２"},
            {"LeftHandIndex3", "左人指３"},
            {"LeftHandMiddle1", "左中指１"},
            {"LeftHandMiddle2", "左中指２"},
            {"LeftHandMiddle3", "左中指３"},
            {"LeftHandRing1", "左薬指１"},
            {"LeftHandRing2", "左薬指２"},
            {"LeftHandRing3", "左薬指３"},
            {"LeftHandPinky1", "左小指１"},
            {"LeftHandPinky2", "左小指２"},
            {"LeftHandPinky3", "左小指３"},
            {"RightShoulder", "右肩"},
            {"RightArm", "右腕"},
            {"RightForeArm", "右ひじ"},
            {"RightHand", "右手首"},
            {"RightHandThumb1", "右親指０"},
            {"RightHandThumb2", "右親指１"},
            {"RightHandThumb3", "右親指２"},
            {"RightHandIndex1", "右人指１"},
            {"RightHandIndex2", "右人指２"},
            {"RightHandIndex3", "右人指３"},
            {"RightHandMiddle1", "右中指１"},
            {"RightHandMiddle2", "右中指２"},
            {"RightHandMiddle3", "右中指３"},
            {"RightHandRing1", "右薬指１"},
            {"RightHandRing2", "右薬指２"},
            {"RightHandRing3", "右薬指３"},
            {"RightHandPinky1", "右小指１"},
            {"RightHandPinky2", "右小指２"},
            {"RightHandPinky3", "右小指３"},
            {"Pelvis", "下半身"},
            {"LeftUpLeg", "左足"},
            {"LeftLeg", "左ひざ"},
            {"LeftFoot", "左足首"},
            {"LeftToeBase", "左つま先"},
            {"RightUpLeg", "右足"},
            {"RightLeg", "右ひざ"},
            {"RightFoot", "右足首"},
            {"RightToeBase", "右つま先"}
        };

        private static readonly Dictionary<string, string> mmdIKBoneMap = new()
        {
            {"IKGoal_LeftFoot", "左足ＩＫ"},
            {"IKGoal_RightFoot", "右足ＩＫ"},
            {"IKGoal_LeftHand", "左手首ＩＫ"},
            {"IKGoal_RightHand", "右手首ＩＫ"}
        };
        
        private static readonly Dictionary<string, string> mmdCreateIKBoneMap = new()
        {
            {"LeftToeBase", "左つま先ＩＫ"},
            {"RightToeBase", "右つま先ＩＫ"}
        };

        private static readonly Dictionary<string, Vector3> mmdIKBoneOffsetMap = new()
        {
            {"左つま先ＩＫ", Vector3.down * 0.1f},
            {"右つま先ＩＫ", Vector3.down * 0.1f},
            {"左足ＩＫ", Vector3.back * 0.1f},
            {"右足ＩＫ", Vector3.back * 0.1f},
            {"左手首ＩＫ", Vector3.back * 0.1f},
            {"右手首ＩＫ", Vector3.back * 0.1f}
        };

        private static readonly Dictionary<string, Vector3> mmdChildOffsetMap = new()
        {
            {"下半身", Vector3.down * 0.1f},
            {"左つま先", Vector3.forward * 0.1f},
            {"右つま先", Vector3.forward * 0.1f},
        };
        
        private static readonly Dictionary<string, Vector3> mmdChildOffsetMapEn = new()
        {
            {"Head_Hair", Vector3.up * 0.1f},
            {"FacialDecal", Vector3.forward * 0.1f}
        };

        private static readonly Dictionary<string, string> mmdParentChildNameMap = new()
        {
            {"全ての親", "センター"},
            {"センター", "腰"},
            {"上半身", "上半身１"},
            {"上半身１", "上半身２"},
            {"上半身２", "首"},
            {"左肩", "左腕"},
            {"左腕", "左ひじ"},
            {"左ひじ", "左手首"},
            {"右肩", "右腕"},
            {"右腕", "右ひじ"},
            {"右ひじ", "右手首"}
        };
        
        private static readonly Dictionary<string, string> mmdChildParentNameMap = new()
        {
            {"左つま先ＩＫ", "左足ＩＫ"},
            {"右つま先ＩＫ", "右足ＩＫ"}
        };

        private static readonly HashSet<string> MoveableBones = new()
        {
            "全ての親",
            "センター",
            "グルーブ",
            "左ダミー",
            "右ダミー",
            "左親指０",
            "右親指０",
            "左足ＩＫ親",
            "左足ＩＫ",
            "左つま先ＩＫ",
            "右足ＩＫ親",
            "右足ＩＫ",
            "右つま先ＩＫ"
        };

        private static readonly HashSet<string> ControllableBones = new()
        {
            "左親指０",
            "左親指１",
            "左親指２",
            "左人指１",
            "左人指２",
            "左人指３",
            "左中指１",
            "左中指２",
            "左中指３",
            "左薬指１",
            "左薬指２",
            "左薬指３",
            "左小指１",
            "左小指２",
            "左小指３",
            "右親指０",
            "右親指１",
            "右親指２",
            "右人指１",
            "右人指２",
            "右人指３",
            "右中指１",
            "右中指２",
            "右中指３",
            "右薬指１",
            "右薬指２",
            "右薬指３",
            "右小指１",
            "右小指２",
            "右小指３",
        };

        private static readonly HashSet<string> ExcludeChildrenTransform = new()
        {
            "Geo_Body",
            "IKBody",
            "IKGoal_LeftHand",
            "IKGoal_RightHand",
            "IKHint_LeftElbow",
            "IKHint_LeftKnee",
            "IKHint_RightElbow",
            "IKHint_RightKnee",
            "LookAt",
            "Move",
            "BodyScaleRatio_DIS",
            "LeftLeg_H",
            "LeftLegSkin1_S",
            "LeftUpLeg_H",
            "LeftUpLeg_Roll_H",
            "RightLeg_H",
            "RightLegSkin1_S",
            "RightUpLeg_H",
            "RightUpLeg_Roll_H",
            "LeftArm_H",
            "LeftArm_Roll_H",
            "LeftForeArm_H",
            "LeftForeArm_Roll_H",
            "LeftHand1_E",
            "LeftHand1_I",
            "LeftHand2_I",
            "LeftHand_H",
            "FaceScaleRatio_DIS",
            "FacialDecal",
            "VLSkinningRenderer",
            "Geo_Hair",
            "Geo_HairProp",
            "HairScaleRatio_DIS",
            "Head1_E",
            "Head1_I",
            "Head2_E",
            "Head2_I",
            "RightArm_H",
            "RightArm_Roll_H",
            "RightForeArm_H",
            "RightForeArm_Roll_H",
            "RightHand1_E",
            "RightHand1_I",
            "RightHand2_I",
            "RightHand_H",
            "Reference1_I",
            "Reference2_I"
        };

        private static IEnumerable<Transform> GetChildrenTransform(Component root, ICollection<string> except)
        {
            var childrenTransform = root.GetComponentsInChildren<Transform>();
            return childrenTransform.Except(childrenTransform.Where(child => except.Contains(child.name)).SelectMany(child => child.GetComponentsInChildren<Transform>()));
        }

        private static void RefreshParentChildIndex(List<Bone> bones,
            IReadOnlyDictionary<string, string> parentChildMap,
            IReadOnlyDictionary<string, Vector3> childOffsetMap,
            IReadOnlyDictionary<string, Vector3> childOffsetMapEn)
        {
            foreach (Bone bone in bones)
            {
                if (parentChildMap.TryGetValue(bone.Name, out string childName))
                {
                    bone.ChildBoneVal.Index = bones.FindIndex(childBone => childBone.Name.Equals(childName));
                }

                if (childOffsetMap.TryGetValue(bone.Name, out Vector3 offset))
                {
                    bone.ChildBoneVal.ChildUseId = false;
                    bone.ChildBoneVal.Offset = offset;
                }
                
                if (childOffsetMapEn.TryGetValue(bone.NameEn, out Vector3 offsetEn))
                {
                    bone.ChildBoneVal.ChildUseId = false;
                    bone.ChildBoneVal.Offset = offsetEn;
                }
            }
        }

        private static void RefreshChildParentIndex(List<Bone> bones, IReadOnlyDictionary<string, string> childParentMap)
        {
            foreach (Bone bone in bones)
            {
                if (childParentMap.TryGetValue(bone.Name, out string parentName))
                {
                    bone.ParentIndex = bones.FindIndex(parentBone => parentBone.Name.Equals(parentName));
                }
            }
        }

        private static void RefreshBoneProperties(List<Bone> bones)
        {
            foreach (Bone bone in bones)
            {
                if (MoveableBones.Contains(bone.Name))
                {
                    bone.Movable = true;
                }

                if (ControllableBones.Contains(bone.Name))
                {
                    bone.Controllable = true;
                }

                if (bone.Name.EndsWith("先"))
                {
                    bone.Visible = false;
                }
            }
        }

        private static string GetMMDBoneName(Transform transform)
        {
            return mmdBoneMap.TryGetValue(transform.name, out string boneName) ? boneName : transform.name;
        }
        
        private static Bone[] ReadMMDBones(List<Transform> bones, Transform root)
        {
            var boneList = ReadBones(bones, root);
            RefreshParentChildIndex(boneList, mmdParentChildNameMap, mmdChildOffsetMap, mmdChildOffsetMapEn);
            RefreshChildParentIndex(boneList, mmdChildParentNameMap);
            RefreshBoneProperties(boneList);
            return boneList.ToArray();
        }
        
        private static List<Bone> ReadBones(List<Transform> bonelist, Transform rootBone)
        {
            var pmxbones = new List<Bone>();
            var createIKBoneList = new HashSet<Transform>();
            foreach (Transform bone in bonelist)
            {
                var pmxbone = new Bone
                {
                    Name = bone == rootBone ? "全ての親" : GetMMDBoneName(bone),
                    NameEn = bone.name,
                    Position = bone.position,
                    ParentIndex = bonelist.IndexOf(bone.parent),
                    TransformLevel = 0,
                    Rotatable = true,
                    Movable = false,
                    HasIk = mmdIKBoneMap.ContainsKey(bone.name),
                    Visible = true,
                    Controllable = false,
                    ChildBoneVal = new Bone.ChildBone
                    {
                        ChildUseId = true,
                        Index = bone.childCount > 0 ? bonelist.IndexOf(bone.GetChild(0)) : -1
                    }
                };
                if (pmxbone.HasIk)
                {
                    int ikTargetIndex = bonelist.FindIndex(target => target.name.Equals(pmxbone.NameEn[7..]));
                    pmxbone.Movable = true;
                    pmxbone.TransformLevel = 1;
                    pmxbone.ParentIndex = -1;
                    pmxbone.ChildBoneVal.ChildUseId = false;
                    pmxbone.ChildBoneVal.Offset = mmdIKBoneOffsetMap[pmxbone.Name];
                    pmxbone.IkInfoVal = new Bone.IkInfo
                    {
                        IkTargetIndex = ikTargetIndex,
                        CcdIterateLimit = 40,
                        CcdAngleLimit = 2,
                        IkLinks = new Bone.IkLink[]
                        {
                            new()
                            {
                                LinkIndex = bonelist.IndexOf(bonelist[ikTargetIndex].parent)
                            },
                            new()
                            {
                                LinkIndex = bonelist.IndexOf(bonelist[ikTargetIndex].parent.parent)
                            }
                        }
                    };
                }

                if (mmdCreateIKBoneMap.ContainsKey(bone.name))
                {
                    createIKBoneList.Add(bone);
                }
                
                pmxbones.Add(pmxbone);
            }

            foreach (Transform bone in createIKBoneList)
            {
                if (mmdCreateIKBoneMap.TryGetValue(bone.name, out string iKBoneName))
                {
                    int ikTargetIndex = bonelist.FindIndex(target => target.name.Equals(bone.name));
                    var ikBone = new Bone
                    {
                        Name = iKBoneName,
                        NameEn = bone.name,
                        Position = bone.position,
                        ParentIndex = bonelist.IndexOf(bone.parent),
                        TransformLevel = 1,
                        Rotatable = true,
                        Movable = true,
                        HasIk = true,
                        Visible = true,
                        Controllable = false,
                        ChildBoneVal = new Bone.ChildBone
                        {
                            ChildUseId = false,
                            Offset = mmdIKBoneOffsetMap[iKBoneName]
                        },
                        IkInfoVal = new Bone.IkInfo
                        {
                            IkTargetIndex = ikTargetIndex,
                            CcdIterateLimit = 3,
                            CcdAngleLimit = 4,
                            IkLinks = new Bone.IkLink[]
                            {
                                new()
                                {
                                    LinkIndex = bonelist.IndexOf(bonelist[ikTargetIndex].parent)
                                }
                            }
                        }
                    };
                    pmxbones.Add(ikBone);
                }
            }
            
            return pmxbones;
        }

        private static Part[] ReadPartMaterials(List<Renderer> renderers, RawMMDModel model)
        {
            var parts = new List<Part>();
            var baseShift = 0;

            foreach (Renderer renderer in renderers)
            {
                Mesh mesh;
                if (renderer is MeshRenderer mr)
                {
                    var meshfilter = mr.GetComponent<MeshFilter>();
                    mesh = meshfilter.mesh;
                }
                else if (((SkinnedMeshRenderer)renderer).sharedMesh.isReadable)
                {
                    mesh = ((SkinnedMeshRenderer)renderer).sharedMesh;
                }
                else
                {
                    mesh = new Mesh();
                    ((SkinnedMeshRenderer)renderer).BakeMesh(mesh);
                }

                var materials = new List<Material>(renderer.sharedMaterials);
                for (int i = 0; i < mesh.subMeshCount; i++)
                {
                    Material material = i < materials.Count ? materials[i] : materials[^1];
                    var part = new Part();
                    //MMDMaterial mat = CreateNewMMDMaterialForPart(model, part, material);
                    CreateNewMMDMaterialForPart(model, part, material);
                    part.BaseShift = baseShift + mesh.GetSubMesh(i).indexStart;
                    part.TriangleIndexNum = mesh.GetSubMesh(i).indexCount;
                    parts.Add(part);
                    
                    // TODO: debug here
                    /*
                    const string hairColorAlpha = "_hir_col_alp.png";
                    const string hairSphere = "_hir_sph.png";
                    if (mat.Texture.TexturePath.EndsWith(hairColorAlpha))
                    {
                        var newPart = new Part();
                        string newPath = mat.Texture.TexturePath.Replace(hairColorAlpha, hairSphere);
                        MMDMaterial newMat = CreateNewMMDMaterialForPart(model, newPart, material);
                        newMat.Texture.TexturePath = newPath;
                        newPart.BaseShift = baseShift + mesh.GetSubMesh(i).indexStart;
                        newPart.TriangleIndexNum = mesh.GetSubMesh(i).indexCount;
                        parts.Add(newPart);
                    }
                    */
                }
                
                baseShift += mesh.triangles.Length;
            }
            
            return parts.ToArray();
        }

        private static void CreateNewMMDMaterialForPart(RawMMDModel model, Part part, Material material)
        {
            var mat = new MMDMaterial();
            part.Material = mat;
            mat.Name = mat.NameEn = material.name.Replace(" (Instance)", "");
            mat.DiffuseColor = Color.white;
            mat.SpecularColor = Color.clear;
            mat.AmbientColor = Color.white * 0.5f;
            mat.Shiness = 5;
            mat.CastSelfShadow = true;
            mat.DrawGroundShadow = true;
            mat.DrawSelfShadow = true;
            mat.EdgeColor = Color.black;
            mat.EdgeSize = 0.4f;
            var propNames = material.GetTexturePropertyNames();
            if (propNames.Length > 0)
            {
                var main_tex = material.GetTexture(propNames[0]);
                var tex = model.TextureList.Find(t => t.TexturePath.Contains($"/{main_tex.name}.png"));
                if (tex != null)
                {
                    mat.Texture = tex;
                }
                else if (model.TextureList.Count > 0)
                {
                    mat.Texture = model.TextureList[0];
                }
            }
            mat.MetaInfo = "";
        }

        private static Vertex[] ReadVerticesAndTriangles(List<Renderer> renderers, List<Transform> bones, ref List<int> triangleList, Transform root)
        {
            List<Vertex> verticesList = new List<Vertex>();
            int vertexOffset = 0;

            foreach (Renderer renderer in renderers)
            {
                if (renderer is MeshRenderer mr)
                {
                    var meshfilter = mr.GetComponent<MeshFilter>();
                    //Cache this data to avoid copying arrays.
                    var mesh = meshfilter.mesh;
                    var vertices = mesh.vertices;
                    var normals = mesh.normals;
                    var uv = mesh.uv;
                    var uv1 = mesh.uv2;
                    var uv2 = mesh.uv3;
                    var colors = mesh.colors;
                    var triangles = mesh.triangles;

                    for (int i = 0; i < vertices.Length; i++)
                    {
                        Vertex vertex = new Vertex();
                        vertex.Coordinate = renderer.transform.TransformPoint(vertices[i]);
                        vertex.Normal = normals[i];
                        vertex.UvCoordinate = uv[i];

                        vertex.ExtraUvCoordinate = new Vector4[]
                        {
                        uv1.Length > 0 ? uv1[i] : Vector2.zero,
                        uv2.Length > 0 ? uv2[i] : Vector2.zero,
                        colors.Length > 0 ? colors[i] : Color.clear
                        };

                        vertex.SkinningOperator = new SkinningOperator{Type = SkinningType.SkinningBdef1};
                        vertex.SkinningOperator.Param = new Bdef1{BoneId = bones.IndexOf(renderer.transform)};
                        vertex.EdgeScale = 1;
                        verticesList.Add(vertex);
                    }
                    
                    foreach (var triangle in triangles)
                    {
                        triangleList.Add(triangle + vertexOffset);
                    }
                    vertexOffset += vertices.Length;
                }
                else if (renderer is SkinnedMeshRenderer smr)
                {
                    Mesh mesh = new Mesh();
                    if (smr.sharedMesh.isReadable)
                    {
                        mesh = smr.sharedMesh;
                    }
                    else
                    {
                        smr.BakeMesh(mesh);
                    }

                    var vertices = mesh.vertices;
                    var normals = mesh.normals;
                    var uv = mesh.uv;
                    var uv1 = mesh.uv2;
                    var uv2 = mesh.uv3;
                    var colors = mesh.colors;
                    var skinbones = smr.bones;
                    var boneCounts = smr.sharedMesh.GetBonesPerVertex();
                    var weights = smr.sharedMesh.boneWeights;
                    var bakemesh = new Mesh();
                    smr.BakeMesh(bakemesh, true);

                    for (int i = 0; i < vertices.Length; i++)
                    {
                        Vertex vertex = new Vertex();
                        vertex.Coordinate = root.InverseTransformPoint(smr.transform.TransformPoint(bakemesh.vertices[i]));
                        vertex.Normal = normals[i];
                        vertex.UvCoordinate = new Vector2(uv[i].x, 1 - uv[i].y);
                        vertex.ExtraUvCoordinate = new Vector4[3]
                        {
                        uv1.Length > 0 ? new Vector2(uv1[i].x, 1 - uv1[i].y) : Vector2.zero,
                        uv2.Length > 0 ? new Vector2(uv2[i].x, 1 - uv2[i].y) : Vector2.zero,
                        colors.Length > 0 ? colors[i] : Color.clear
                        };

                        var boneWeight = weights[i];
                        var boneCount = boneCounts[i];

                        switch (boneCount)
                        {
                            case 0:
                                vertex.SkinningOperator = new SkinningOperator() { Type = SkinningType.SkinningBdef1 };
                                vertex.SkinningOperator.Param = new Bdef1() { BoneId = GetBoneIndex(bones, renderer.transform) };
                                break;

                            default:
                            case 1:
                                vertex.SkinningOperator = new SkinningOperator() { Type = SkinningType.SkinningBdef1 };
                                vertex.SkinningOperator.Param = new Bdef1() { BoneId = GetBoneIndex(bones, skinbones[boneWeight.boneIndex0]) };
                                break;

                            case 2:
                                vertex.SkinningOperator = new SkinningOperator() { Type = SkinningType.SkinningBdef2 };
                                vertex.SkinningOperator.Param = new Bdef2()
                                {
                                    BoneId = new int[]{
                                    GetBoneIndex(bones, skinbones[boneWeight.boneIndex0]),
                                    GetBoneIndex(bones, skinbones[boneWeight.boneIndex1]),
                                },
                                    BoneWeight = boneWeight.weight0
                                };
                                break;

                            case 3:
                            case 4:
                                vertex.SkinningOperator = new SkinningOperator() { Type = SkinningType.SkinningBdef4 };
                                vertex.SkinningOperator.Param = new Bdef4()
                                {
                                    BoneId = new int[]{
                                    GetBoneIndex(bones, skinbones[boneWeight.boneIndex0]),
                                    GetBoneIndex(bones, skinbones[boneWeight.boneIndex1]),
                                    GetBoneIndex(bones, skinbones[boneWeight.boneIndex2]),
                                    GetBoneIndex(bones, skinbones[boneWeight.boneIndex3]),
                                },
                                    BoneWeight = new float[]
                                    {
                                    boneWeight.weight0,
                                    boneWeight.weight1,
                                    boneWeight.weight2,
                                    boneWeight.weight3
                                    }
                                };
                                break;
                        }
                        vertex.EdgeScale = 1;
                        verticesList.Add(vertex);
                    }

                    foreach (var triangle in mesh.triangles)
                    {
                        triangleList.Add(triangle + vertexOffset);
                    }
                    vertexOffset += vertices.Length;
                }
            }
            return verticesList.ToArray();
        }

        private static int GetBoneIndex(List<Transform> bones, Transform bone)
        {
            return bones.Contains(bone) ? bones.IndexOf(bone) : 0;
        }
    }

    public struct PMXModelConfig
    {
        public string Name;
        public string NameEn;
        public string Description;
        public string DescriptionEn;

        public PMXModelConfig(GameObject gameObject)
        {
            Name = NameEn = gameObject.name;
            Description = DescriptionEn = gameObject.name;
        }
    }
}
