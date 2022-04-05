using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator {
    public class MeshData {
        public Vector3[] vertices;
        public int[] triangles;
        public Vector2[] uvs;

        int triangleIndex;

        public MeshData(int meshWidth, int meshHeight) {
            vertices = new Vector3[meshWidth*meshHeight];
            triangles = new int[(meshWidth-1)*(meshHeight-1)*6];
            uvs = new Vector2[vertices.Length];
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
            mesh.uv  = uvs;
            mesh.RecalculateNormals();
            return mesh;
        }
    }

    public static MeshData[] GenerateTerrainChunk(float[,] heightMap, float multiplier, float gain) {
        int width      = heightMap.GetLength(0);
        int height     = heightMap.GetLength(1);
        float topLeftX = (width-1)/-2f;
        float topLeftZ = (height-1)/2f;

        MeshData topMeshData = new MeshData(width, height);
        int vertexIndex = 0;

        for(int z = 0; z < height; ++z) {
            for(int x = 0; x < width; ++x) {
                topMeshData.vertices[vertexIndex] = new Vector3(
                    x: topLeftX + x,
                    y: heightMap[x,z] * multiplier + gain,
                    z: topLeftZ - z
                );
                topMeshData.uvs[vertexIndex] = new Vector2((x/(float)width), z/(float)height);  //망가지면 여기 width 뒤에 /2 없앰 아래도 똑같음

                if(x < width-1 && z < height-1) {
                    topMeshData.AddTriangle(vertexIndex, vertexIndex+1, vertexIndex+width);
                    topMeshData.AddTriangle(vertexIndex+width, vertexIndex+1, vertexIndex+width+1);
                }

                ++vertexIndex;
            }
        }

        MeshData bottomMeshData = new MeshData(width, height);
        vertexIndex = 0;
        for(int z = 0; z < height; ++z) {
            for(int x = 0; x < width; ++x) {
                bottomMeshData.vertices[vertexIndex] = new Vector3(
                    x: topLeftX + x,
                    y: 0,
                    z: topLeftZ - z
                );

                bottomMeshData.uvs[vertexIndex] = new Vector2((x/(float)width), z/(float)height);

                if(x < width-1 && z < height-1) {
                    bottomMeshData.AddTriangle(vertexIndex,   vertexIndex+width, vertexIndex+1);
                    bottomMeshData.AddTriangle(vertexIndex+1, vertexIndex+width, vertexIndex+width+1);
                }

                ++vertexIndex;
            }
        }
        
        MeshData leftMeshData = new MeshData(width, height);
        MeshData rightMeshData = new MeshData(width, height);
        vertexIndex = 0;
        for(int z = 0; z < 2; ++z) {    //2 as there is only two layers in vertical axis
            for(int x = 0; x < width; ++x) {
                leftMeshData .vertices[vertexIndex] = z==0 ? bottomMeshData.vertices[x]                    : topMeshData.vertices[x];
                rightMeshData.vertices[vertexIndex] = z==0 ? bottomMeshData.vertices[x + width*(height-1)] : topMeshData.vertices[x + width*(height-1)];

                leftMeshData .uvs[vertexIndex] = new Vector2((x/(float)width), z/(float)height);
                rightMeshData.uvs[vertexIndex] = new Vector2((x/(float)width), z/(float)height);

                if(x < width-1 && z < 1) {
                    leftMeshData .AddTriangle(vertexIndex,       vertexIndex+1,     vertexIndex+width);
                    leftMeshData .AddTriangle(vertexIndex+width, vertexIndex+1,     vertexIndex+width+1);
                    rightMeshData.AddTriangle(vertexIndex,       vertexIndex+width, vertexIndex+1);
                    rightMeshData.AddTriangle(vertexIndex+1,     vertexIndex+width, vertexIndex+width+1);
                }

                ++vertexIndex;
            }
        }
        //MeshData frontMeshData = new MeshData(width, height);
        //MeshData backMeshData = new MeshData(width, height);
        //vertexIndex = 0;
        //for(int z = 0; z < 2; ++z) {    //2 as there is only two layers in vertical axis
        //    for(int x = 0; x < height; ++x) {
        //        frontMeshData.vertices[vertexIndex] = z==0 ? bottomMeshData.vertices[height*x] : topMeshData.vertices[height*x];
        //        backMeshData .vertices[vertexIndex] = z==0 ? bottomMeshData.vertices[height*x + (width-1)*height] : topMeshData.vertices[height*x + (width-1)*height];
//
        //        frontMeshData.uvs[vertexIndex] = new Vector2((x/(float)width), z/(float)height);
        //        backMeshData .uvs[vertexIndex] = new Vector2((x/(float)width), z/(float)height);
//
        //        if(x < width-1 && z < 1) {
        //            frontMeshData.AddTriangle(vertexIndex,        vertexIndex+height, vertexIndex+1);
        //            frontMeshData.AddTriangle(vertexIndex+1,      vertexIndex+height, vertexIndex+height+1);
        //            backMeshData .AddTriangle(vertexIndex,        vertexIndex+1,      vertexIndex+height);
        //            backMeshData .AddTriangle(vertexIndex+height, vertexIndex+1,      vertexIndex+height+1);
        //        }
//
        //        ++vertexIndex;
        //    }
        //}

        //MeshData[] meshes = new MeshData[]{topperMeshData, bottomMeshData, leftMeshData, rightMeshData, frontMeshData, backMeshData};
        //CombineInstance[] combine = new CombineInstance[meshes.Length];
        //for(int i = 0; i < meshes.Length; ++i) {
        //    combine[i].mesh = meshes[i].CreateMesh();
        //    // combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        //}
        //Mesh resultMesh = new Mesh();
        //resultMesh.CombineMeshes(combine);

        return new MeshData[]{topMeshData, bottomMeshData, leftMeshData, rightMeshData};//, frontMeshData, bottomMeshData};
    }

}