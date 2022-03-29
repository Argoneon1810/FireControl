using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator {
    public class MeshData {
        public Vector3[] vertices;
        public int[] triangles;
        public List<Vector2[]> uvs = new List<Vector2[]>();

        int triangleIndex;

        public MeshData(int meshWidth, int meshHeight) {
            //for plane mesh
            //vertices = new Vector3[meshWidth * meshHeight];
            //uvs = new Vector2[meshWidth * meshHeight];
            //triangles = new int[(meshWidth-1)*(meshHeight-1)*6];

            vertices = new Vector3[meshWidth*meshHeight*2];
            triangles = new int[
                (meshWidth-1)*(meshHeight-1)*6*2 //top and bottom face
                + (meshWidth-1)*6*2           //sideface - width side; *(2-1) is skipped as it is unnecessary. its 2 because there are only two vertical vertices
                + (meshHeight-1)*6*2          //sideface - height side; *(2-1) is skipped as it is unnecessary. its 2 because there are only two vertical vertices
            ];
            AddNewUVChannel();
        }

        /// returns true if the mesh had less than 8 uv channels prior to current adding request
        /// returns false if there were 8 channels already added to the mesh
        public bool AddNewUVChannel() {
            if(uvs.Count >= 8) return false;

            uvs.Add(new Vector2[vertices.Length]);
            return true;
        }
        
        public void AddTriangle(int a, int b, int c) {
            triangles[triangleIndex] = a;
            triangles[triangleIndex+1] = b;
            triangles[triangleIndex+2] = c;
            triangleIndex += 3;
        }

        public Mesh CreateMesh() {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            for(int i = 0; i < uvs.Count; ++i) {
                switch(i) {
                    case 0:
                        mesh.uv  = uvs[i];
                        break;
                    case 1:
                        mesh.uv2 = uvs[i];
                        break;
                    case 2:
                        mesh.uv3 = uvs[i];
                        break;
                    case 3:
                        mesh.uv4 = uvs[i];
                        break;
                    case 4:
                        mesh.uv5 = uvs[i];
                        break;
                    case 5:
                        mesh.uv6 = uvs[i];
                        break;
                    case 6:
                        mesh.uv7 = uvs[i];
                        break;
                    case 7:
                        mesh.uv8 = uvs[i];
                        break;
                }
            }
            mesh.RecalculateNormals();
            return mesh;
        }
    }

    public static MeshData GenerateTerrainChunk(float[,] heightMap, float multiplier, float gain) {
        int width      = heightMap.GetLength(0);
        int height     = heightMap.GetLength(1);
        float topLeftX = (width-1)/-2f;
        float topLeftZ = (height-1)/2f;

        MeshData meshData = new MeshData(width, height);
        int vertexIndex = 0;

        for(int z = 0; z < height; ++z) {
            for(int x = 0; x < width; ++x) {
                meshData.vertices[vertexIndex] = new Vector3(
                    x: topLeftX + x,
                    y: heightMap[x,z] * multiplier + gain,
                    z: topLeftZ - z)
                ;
                meshData.uvs[0][vertexIndex] = new Vector2((x/(float)width)/2, z/(float)height);

                if(x < width-1 && z < height-1) {
                    meshData.AddTriangle(vertexIndex, vertexIndex+1, vertexIndex+width);
                    meshData.AddTriangle(vertexIndex+width, vertexIndex+1, vertexIndex+width+1);
                }

                ++vertexIndex;
            }
        }

        if(meshData.AddNewUVChannel()) {
            for(int z = 0; z < height; ++z) {
                for(int x = 0; x < width; ++x) {
                    meshData.vertices[vertexIndex] = new Vector3(
                        x: topLeftX + x,
                        y: 0,
                        z: topLeftZ - z)
                    ;

                    meshData.uvs[0][vertexIndex] = new Vector2((x/(float)width)/2, z/(float)height);

                    if(x < width-1 && z < height-1) {
                        meshData.AddTriangle(vertexIndex,   vertexIndex+width, vertexIndex+1);
                        meshData.AddTriangle(vertexIndex+1, vertexIndex+width, vertexIndex+width+1);
                    }

                    ++vertexIndex;
                }
            }
        } else Debug.Log("Bottomface Generation: Something Went Wrong");

        if(meshData.AddNewUVChannel()) {
            for (int i = 0; i < width; ++i) {
                int bI = i+width*height;
                //meshData.uvs[1][bI] = new Vector2(i/(float)width, 0);
                //meshData.uvs[1][i]  = new Vector2(i/(float)width, 1);
                if(i < width-1) {
                    meshData.AddTriangle(bI,                    bI+1,                   i);
                    meshData.AddTriangle(i,                     bI+1,                   i+1);
                    meshData.AddTriangle(bI+(width*(height-1)), i+(width*(height-1))+1, bI+(width*(height-1))+1);
                    meshData.AddTriangle(i+(width*(height-1)),  i+(width*(height-1))+1, bI+(width*(height-1)));
                }
            }
        } else Debug.Log("Rimface Width Generation: Something Went Wrong");

        if(meshData.AddNewUVChannel()) {
            for (int i = 0; i < height; ++i) {
                int bZ = width*height;
                //meshData.uvs[2][bZ + width*i] = new Vector2(0, i/(float)width);
                //meshData.uvs[2][width*i]      = new Vector2(1, i/(float)width);
                if(i < height-1) {
                    meshData.AddTriangle(bZ+width*i,           width*i,                  bZ+width*(i+1));
                    meshData.AddTriangle(bZ+width*(i+1),       width*i,                  width*(i+1));
                    meshData.AddTriangle(bZ+(width-1)+width*i, bZ+(width-1)+width*(i+1), (width-1)+width*i);
                    meshData.AddTriangle((width-1)+width*i,    bZ+(width-1)+width*(i+1), (width-1)+width*(i+1));
                }
            }
        } else Debug.Log("Rimface Width Generation: Something Went Wrong");

        return meshData;
    }

}