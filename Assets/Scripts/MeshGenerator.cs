using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator {
    public const int TOP_INDEX       = 0;
    public const int BOTTOM_INDEX    = 1;
    public const int LEFT_INDEX      = 2;
    public const int RIGHT_INDEX     = 3;
    public const int FRONT_INDEX     = 4;
    public const int BACK_INDEX      = 5;
    
    public class MeshData {
        public Vector3[] vertices;
        public int[] triangles;
        public Vector2[] uvs;

        public int width, height;

        int triangleIndex;

        public MeshData(int meshWidth, int meshHeight) {
            width = meshWidth;
            height = meshHeight;
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

    public static MeshData[] GenerateTerrainChunk(
        float[,] heightMap, 
        float multiplier, 
        float gain
    ) {
        int width      = heightMap.GetLength(0);
        int height     = heightMap.GetLength(1);

        MeshData topMeshData = new MeshData(width, height);
        MeshData bottomMeshData = new MeshData(width, height);
        MeshData leftMeshData = new MeshData(width, 2);
        MeshData rightMeshData = new MeshData(width, 2);
        MeshData frontMeshData = new MeshData(width, 2);
        MeshData backMeshData = new MeshData(width, 2);

        GenerateTop(topMeshData, heightMap, multiplier, gain);
        GenerateBottom(bottomMeshData);
        GenerateSides(
            topMeshData,    bottomMeshData, 
            leftMeshData,   rightMeshData, 
            frontMeshData,  backMeshData
        );

        return new MeshData[] {
            topMeshData,    bottomMeshData,
            leftMeshData,   rightMeshData,
            frontMeshData,  backMeshData
        };
    }

    private static void GenerateTop(MeshData data, float[,] heightMap, float multiplier, float gain) {
        int width      = data.width;
        int height     = data.height;
        float topLeftX = (width-1)/-2f;
        float topLeftZ = (height-1)/2f;

        int vertexIndex = 0;
        for(int z = 0; z < height; ++z) {
            for(int x = 0; x < width; ++x) {
                data.vertices[vertexIndex] = new Vector3(
                    x: topLeftX + x,
                    y: heightMap[x,z] * multiplier + gain,
                    z: topLeftZ - z
                );
                data.uvs[vertexIndex] = new Vector2((x/(float)width), z/(float)height);  //망가지면 여기 width 뒤에 /2 없앰 아래도 똑같음

                if(x < width-1 && z < height-1) {
                    data.AddTriangle(vertexIndex, vertexIndex+1, vertexIndex+width);
                    data.AddTriangle(vertexIndex+width, vertexIndex+1, vertexIndex+width+1);
                }

                ++vertexIndex;
            }
        }
    }

    private static void GenerateBottom(MeshData data) {
        int width      = data.width;
        int height     = data.height;
        float topLeftX = (width-1)/-2f;
        float topLeftZ = (height-1)/2f;

        int vertexIndex = 0;
        for(int z = 0; z < height; ++z) {
            for(int x = 0; x < width; ++x) {
                data.vertices[vertexIndex] = new Vector3(
                    x: topLeftX + x,
                    y: 0,
                    z: topLeftZ - z
                );

                data.uvs[vertexIndex] = new Vector2((x/(float)width), z/(float)height);

                if(x < width-1 && z < height-1) {
                    data.AddTriangle(vertexIndex,   vertexIndex+width, vertexIndex+1);
                    data.AddTriangle(vertexIndex+1, vertexIndex+width, vertexIndex+width+1);
                }

                ++vertexIndex;
            }
        }
    }

    private static void GenerateSides(MeshData topData, MeshData bottomData, MeshData leftData, MeshData rightData, MeshData frontData, MeshData backData) {
        int width = leftData.width;
        int height = width;

        int vertexIndex = 0;
        for(int z = 0; z < 2; ++z) {
            for(int x = 0; x < width; ++x) {
                leftData .vertices[vertexIndex] = z==0 ? bottomData.vertices[x]                    : topData.vertices[x];
                rightData.vertices[vertexIndex] = z==0 ? bottomData.vertices[x + width*(height-1)] : topData.vertices[x + width*(height-1)];

                leftData .uvs[vertexIndex] = new Vector2((x/(float)width), z/(float)height);
                rightData.uvs[vertexIndex] = new Vector2((x/(float)width), z/(float)height);

                if(x < width-1 && z < 1) {
                    leftData .AddTriangle(vertexIndex,       vertexIndex+1,     vertexIndex+width);
                    leftData .AddTriangle(vertexIndex+width, vertexIndex+1,     vertexIndex+width+1);
                    rightData.AddTriangle(vertexIndex,       vertexIndex+width, vertexIndex+1);
                    rightData.AddTriangle(vertexIndex+1,     vertexIndex+width, vertexIndex+width+1);
                }

                ++vertexIndex;
            }
        }

        vertexIndex = 0;
        for(int z = 0; z < 2; ++z) {
           for(int x = 0; x < height; ++x) {
               frontData.vertices[vertexIndex] = z==0 ? bottomData.vertices[height*x]             : topData.vertices[height*x];
               backData .vertices[vertexIndex] = z==0 ? bottomData.vertices[height*x + (width-1)] : topData.vertices[height*x + (width-1)];

               frontData.uvs[vertexIndex] = new Vector2((x/(float)width), z/(float)height);
               backData .uvs[vertexIndex] = new Vector2((x/(float)width), z/(float)height);

               if(x < width-1 && z < 1) {
                   frontData.AddTriangle(vertexIndex,        vertexIndex+height,   vertexIndex+1);
                   frontData.AddTriangle(vertexIndex+1,      vertexIndex+height,   vertexIndex+height+1);
                   backData .AddTriangle(vertexIndex,        vertexIndex+1,      vertexIndex+height);
                   backData .AddTriangle(vertexIndex+height, vertexIndex+1,      vertexIndex+height+1);
               }

               ++vertexIndex;
           }
        }
    }
}