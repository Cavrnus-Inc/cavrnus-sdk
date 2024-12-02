using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Collab.Base.Collections;
using Collab.Base.Core;
using Collab.Base.Graphics;
using Collab.Base.Math;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = System.Diagnostics.Debug;
using Mesh = Collab.Base.Graphics.Mesh;

namespace UnityBase.Content
{
	public static class MeshConvert
	{
		public static UnityEngine.Mesh ConstructUnityMesh(Mesh mesh)
		{
			if (!SystemInfo.supports32bitsIndexBuffer && mesh.Vertices.Count >= ushort.MaxValue - 5)
			{
				var split = MeshSplitter.Split(mesh, Int32.MaxValue, ushort.MaxValue - 5);
				DebugOutput.Warning("Failed to fully load mesh.  System unable to handle large meshes.  Mesh size: " + mesh.Vertices.Count);
				return ConvertSingleMesh(split[0]);
			}
			return ConvertSingleMesh(mesh);
		}


		private static unsafe UnityEngine.Mesh ConvertSingleMesh(Mesh m)
		{
//			Debug.Assert(m.Vertices.Count <= MAX_VERTEX_COUNT);

			UnityEngine.Mesh r = new UnityEngine.Mesh();

			r.bounds = new Bounds(m.BoundingBox.GetCenter().ToVec3(), (m.BoundingBox.max - m.BoundingBox.min).ToVec3());
			r.vertices = BufferConverters.UnsafeCopyTranslateVector3(m.Vertices.ToArray());

			if (m.HasNormals)
			{
				r.normals = BufferConverters.UnsafeCopyTranslateVector3(m.Normals.ToArray());
			}
			if (m.HasTextureCoordinates)
			{
				r.uv = BufferConverters.UnsafeCopyTranslateVector2(m.TextureCoordinates.ToArray());
			}
			if (m.HasTangentsWithOrientation)
			{
				r.tangents = BufferConverters.UnsafeCopyTranslateVector4(m.TangentsWithOrientation.ToArray());
			}
			if (m.HasTextureCoordinates2)
			{
				r.uv2 = BufferConverters.UnsafeCopyTranslateVector2(m.TextureCoordinates2.ToArray());
			}
			if (m.HasTextureCoordinates3)
			{
				r.uv3 = BufferConverters.UnsafeCopyTranslateVector2(m.TextureCoordinates3.ToArray());
			}
			if (m.HasTextureCoordinates4)
			{
				r.uv4 = BufferConverters.UnsafeCopyTranslateVector2(m.TextureCoordinates4.ToArray());
			}
			if (m.HasColors)
			{
				r.colors32 = BufferConverters.UnsafeCopyTranslateColor4(m.Colors.ToArray());
			}

			if (m.HasTransformWeights && m.HasTransformIndices)
			{
				if (m.TransformIndices8 != null)
				{
					var boneweights = new BoneWeight[m.TransformIndices8.Count];
					for (int i = 0; i < m.TransformIndices8.Count; i++)
					{
						boneweights[i] = new BoneWeight()
						{
							boneIndex0 = m.TransformIndices8[i].A,
							boneIndex1 = m.TransformIndices8[i].B,
							boneIndex2 = m.TransformIndices8[i].C,
							boneIndex3 = m.TransformIndices8[i].D,
							weight0 = Colors.ByteToFloat(m.TransformWeights[i].A),
							weight1 = Colors.ByteToFloat(m.TransformWeights[i].B),
							weight2 = Colors.ByteToFloat(m.TransformWeights[i].C),
							weight3 = Colors.ByteToFloat(m.TransformWeights[i].D)
						};
					}

					r.boneWeights = boneweights;
				}
				else if (m.TransformIndices16 != null)
				{
					var boneweights = new BoneWeight[m.TransformIndices16.Count];
					for (int i = 0; i < m.TransformIndices16.Count; i++)
					{
						boneweights[i] = new BoneWeight()
						{
							boneIndex0 = m.TransformIndices16[i].A,
							boneIndex1 = m.TransformIndices16[i].B,
							boneIndex2 = m.TransformIndices16[i].C,
							boneIndex3 = m.TransformIndices16[i].D,
							weight0 = Colors.ByteToFloat(m.TransformWeights[i].A),
							weight1 = Colors.ByteToFloat(m.TransformWeights[i].B),
							weight2 = Colors.ByteToFloat(m.TransformWeights[i].C),
							weight3 = Colors.ByteToFloat(m.TransformWeights[i].D)
						};
					}

					r.boneWeights = boneweights;

				}

				r.bindposes = m.inverseBindingMatrices.Select(mtx=>new Matrix4x4(mtx.a.ToVec4(0), mtx.b.ToVec4(0), mtx.c.ToVec4(0), mtx.d.ToVec4(1))).ToArray();
			}
			if (m.HasFaces)
			{
				int[] fs = BufferConverters.UnsafeCopyTranslateInt3(m.Faces.ToArray());
				/*else
				{
					fs = new int[m.Faces.Count * 3];
					for (int i = 0; i < m.Faces.Count; i++)
					{
						fs[i * 3 + 0] = m.Faces[i].X;
						fs[i * 3 + 1] = m.Faces[i].Z;
						fs[i * 3 + 2] = m.Faces[i].Y;
					}
				}*/
				if (m.Vertices.Count > ushort.MaxValue - 5)
					r.indexFormat = IndexFormat.UInt32;
				r.SetIndices(fs, MeshTopology.Triangles, 0);
			}
			else
			{
				if (m.Vertices.Count > ushort.MaxValue - 5)
					r.indexFormat = IndexFormat.UInt32;
				r.SetIndices(Enumerate.Range(0, m.Vertices.Count - 1).ToArray(), MeshTopology.Points, 0);
			}

			if (m.HasSubMeshes)
			{
				r.subMeshCount = m.SubMeshCount;
				var subMeshes = m.SubMeshRanges.Select(s => new SubMeshDescriptor(s.IndexStart, s.IndexCount, MeshTopology.Triangles)).ToList();
				 // NOTICE: We're adding a 'fake' submesh on top of the list of submeshes that covers the whole span! This lets over-layered materials cover the full surface of split meshes
				if (subMeshes.Count > 1) // only if we're a divided mesh do we do this.
					subMeshes.Add(new SubMeshDescriptor(0, m.Faces.Count * 3, MeshTopology.Triangles));
				
				r.SetSubMeshes(subMeshes.ToArray());
			}

			if (!m.HasTangentsWithOrientation && m.HasNormals && m.HasTextureCoordinates)
			{
				r.RecalculateTangents();
			}

			if (m.HasBlendShapes)
			{
				foreach (var mbs in m.blendShapes)
				{
					foreach (var frame in mbs.frames)
					{
						r.AddBlendShapeFrame(mbs.name, frame.weight,
							frame.deltaVerts.Select(v => v.ToVec3()).ToArray(),
							frame.deltaNormals.Select(n => n.ToVec3()).ToArray(),
							frame.deltaTangents.Select(t => t.ToVec3()).ToArray());
					}
				}
			}
			
			return r;
		}

		public static Mesh RevertToGeometryMesh(UnityEngine.Mesh mesh)
		{
			Mesh m = new Mesh();

			if (mesh.subMeshCount == 0)
			{
				var tris = mesh.triangles;
				if (tris != null && tris.Length > 0)
				{
					m.CreateFacesList(tris.Length / 3);
					m.AddTriangles(tris.Triowise((a, b, c) => new Int3(a, b, c)));
				}
			}
			else
			{
				m.CreateSubMeshList(mesh.subMeshCount);
				if (Enumerable.Range(0, mesh.subMeshCount).Select(ind => mesh.GetSubMesh(ind)).All(m => m.topology == MeshTopology.Triangles))
				{
					// All triangles, normal case. Copy indices like usual.
					var tris = mesh.triangles;
					if (tris != null && tris.Length > 0)
					{
						m.CreateFacesList(tris.Length / 3);
						m.AddTriangles(tris.Triowise((a, b, c) => new Int3(a, b, c)));
					}

					for (int sm = 0; sm < mesh.subMeshCount; sm++)
					{
						var submeshdesc = mesh.GetSubMesh(sm);
						if (sm > 1 && sm == mesh.subMeshCount - 1) // Last and not first or second. We're checking for our injected complete-span submesh so we can elide it
						{
							if (m.HasFaces && submeshdesc.indexStart == 0 && submeshdesc.indexCount >= (m.Faces.Count - 1) * 3) // if its the last but starts at 0 and ends at max, it's probably our injected complete-span
							{
								continue;
							}
						}
						m.AddSubMesh(submeshdesc.indexStart, submeshdesc.indexCount);

					}
				}
				else // Mismatched topologies.. We don't support everything so we'll have to dump or convert to triangles
				{
					m.CreateFacesList();
					for (int sm = 0; sm < mesh.subMeshCount; sm++)
					{
						var submeshdesc = mesh.GetSubMesh(sm);
						if (sm > 1 && sm == mesh.subMeshCount - 1) // Last and not first or second. We're checking for our injected complete-span submesh so we can elide it
						{
							if (m.HasFaces && submeshdesc.indexStart == 0 && submeshdesc.indexCount >= (m.Faces.Count - 1) * 3) // if its the last but starts at 0 and ends at max, it's probably our injected complete-span
							{
								continue;
							}
						}

						int curEndIndex = m.Faces.Count*3;
						switch (submeshdesc.topology)
						{
							case MeshTopology.Triangles:
							{
								var tris = mesh.GetIndices(sm);
								m.AddTriangles(tris);
								m.AddSubMesh(curEndIndex, tris.Length);
							}
							break;

							case MeshTopology.Quads:
							{
								var quads = mesh.GetIndices(sm);
								var tris = new int[(quads.Length * 6) / 4];
								for (int i = 0; i < quads.Length; i += 4)
								{
									tris[i / 4 * 6 + 0] = quads[i + 0];
									tris[i / 4 * 6 + 1] = quads[i + 1];
									tris[i / 4 * 6 + 2] = quads[i + 2];
									tris[i / 4 * 6 + 3] = quads[i + 0];
									tris[i / 4 * 6 + 4] = quads[i + 2];
									tris[i / 4 * 6 + 5] = quads[i + 3];
								}
								m.AddTriangles(tris);
								m.AddSubMesh(curEndIndex, tris.Length);
							}
								break;

							default: // Things we don't handle because we haven't needed to.
							{
								m.AddSubMesh(curEndIndex, 0);
							} 
							break;
						}
					}
				}
			}

			m.CreateVertexList(mesh.vertices.Length);
			m.AddVertices(mesh.vertices.Select((v) => v.ToFloat3()));
			if (mesh.normals != null)
			{
				m.CreateNormalList(mesh.vertices.Length);
				m.AddNormals(mesh.normals.Select((n) => n.ToFloat3()));
			}
			if (mesh.tangents != null)
			{
				m.CreateTangentWithOrientationList(mesh.vertices.Length);
				m.AddTangentsWithOrientation(mesh.tangents.Select(t=>t.ToFloat4()));
			}
			if (mesh.uv != null)
			{
				m.CreateTextureCoordinateList(mesh.uv.Length);
				m.AddTextureCoordinates(mesh.uv.Select((u) => u.ToFloat2()));
			}
			if (mesh.uv2 != null)
			{
				m.CreateTextureCoordinateList2(mesh.uv2.Length);
				m.AddTextureCoordinates2(mesh.uv2.Select((u) => u.ToFloat2()));
			}
			if (mesh.uv3 != null)
			{
				m.CreateTextureCoordinateList3(mesh.uv3.Length);
				m.AddTextureCoordinates3(mesh.uv3.Select((u) => u.ToFloat2()));
			}
			if (mesh.uv4 != null)
			{
				m.CreateTextureCoordinateList4(mesh.uv4.Length);
				m.AddTextureCoordinates4(mesh.uv4.Select((u) => u.ToFloat2()));
			}
			if (mesh.colors != null)
			{
				m.CreateColorList(mesh.colors.Length);
				m.AddColors(mesh.colors.Select(c => c.ToColor4()));
			}
			else if (mesh.colors32 != null)
			{
				m.CreateColorList(mesh.colors32.Length);
				m.AddColors(mesh.colors32.Select(c => new Color4(c.r, c.g, c.b, c.a)));
			}

			if (mesh.boneWeights != null && mesh.bindposes != null && mesh.boneWeights.Length > 0)
			{
				if (mesh.bindposes.Length > 254) // use 16 bit indices
				{
					m.CreateTransformIndexList16(mesh.boneWeights.Length);
					for (int i = 0; i < mesh.boneWeights.Length; i++)
					{
						var bw = mesh.boneWeights[i];
						m.AddTransformIndex(
							new Short4(BoneIndexToShort(bw.boneIndex0), BoneIndexToShort(bw.boneIndex1),
								BoneIndexToShort(bw.boneIndex2), BoneIndexToShort(bw.boneIndex3)),
							new Byte4(Colors.FloatToByte(bw.weight0), Colors.FloatToByte(bw.weight1),
								Colors.FloatToByte(bw.weight2), Colors.FloatToByte(bw.weight3)));
					}
				}
				else
				{
					m.CreateTransformIndexList8(mesh.boneWeights.Length);
					for (int i = 0; i < mesh.boneWeights.Length; i++)
					{
						var bw = mesh.boneWeights[i];
						m.AddTransformIndex(
							new Byte4(BoneIndexToByte(bw.boneIndex0), BoneIndexToByte(bw.boneIndex1),
								BoneIndexToByte(bw.boneIndex2), BoneIndexToByte(bw.boneIndex3)),
							new Byte4(Colors.FloatToByte(bw.weight0), Colors.FloatToByte(bw.weight1),
								Colors.FloatToByte(bw.weight2), Colors.FloatToByte(bw.weight3)));

					}
				}
				m.InverseBindingMatrices =
					mesh.bindposes.Select(mtx => mtx.ToMatrix34()).ToList();
			}

			if (mesh.blendShapeCount > 0)
			{
				Vector3[] dvs = new Vector3[mesh.vertexCount];
				Vector3[] dns = new Vector3[mesh.vertexCount];
				Vector3[] dts = new Vector3[mesh.vertexCount];
				for (var bsi = 0; bsi < mesh.blendShapeCount; bsi++)
				{
					Mesh.MeshBlendShape newshape = new Mesh.MeshBlendShape();
					newshape.name = mesh.GetBlendShapeName(bsi);
					newshape.frames = new List<Mesh.MeshBlendShapeFrame>();
					int frames = mesh.GetBlendShapeFrameCount(bsi);
					for (var bsif = 0; bsif < frames; bsif++)
					{
						float wgt = mesh.GetBlendShapeFrameWeight(bsi, bsif);
						mesh.GetBlendShapeFrameVertices(bsi, bsif, dvs, dns, dts);
						newshape.frames.Add(new Mesh.MeshBlendShapeFrame()
						{
							weight = wgt,
							deltaVerts = dvs.Select((v) => v.ToFloat3()).ToList(),
							deltaNormals = dns.Select((n)=>n.ToFloat3()).ToList(),
							deltaTangents = dts.Select((t)=>t.ToFloat3()).ToList()
						});
					}

					m.AddBlendShape(newshape);
				}
			}

			return m;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static byte BoneIndexToByte(int transformindex)
		{
			if (transformindex < -1 || transformindex >= 256)
				throw new ArgumentOutOfRangeException($"transformindex is out of range (is {transformindex}, not -1<=x<256");
			if (transformindex < 0)
				return byte.MaxValue; // set to none.

			return (byte) transformindex;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static short BoneIndexToShort(int transformindex)
		{
			if (transformindex < -1 || transformindex >= short.MaxValue)
				throw new ArgumentOutOfRangeException($"transformindex is out of range (is {transformindex}, not -1<=x<{short.MaxValue}");

			if (transformindex < 0)
				return short.MaxValue;

			return (short)transformindex;
		}
	}
}