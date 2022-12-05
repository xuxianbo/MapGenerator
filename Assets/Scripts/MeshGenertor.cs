    using UnityEngine;

    public static class MeshGenertor
    {
        public static MeshData GeneratorTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve meshHeightCurve, int levelOfDetail)
        {
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);

            float topLeftX = (width - 1) / -2f;
            float topLeftZ = (height - 1) / 2f;

            int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
            
            //===================================================
            // https://www.youtube.com/watch?v=417kJGPKwDg
            // 第5分46秒有解释
            // 0， 1， 2， 3， 4， 5， 6，7，8
            // 如果levelOfDetail=2    （W-1）/levelOfDetail 来到4时 只有五个顶点 不能构成2个三角形
            // （0，1，2）一个三角形  （3， 4， 5）一个三角形  要6个顶点才能构成余下的三角形  所有后面要+1   
            // 公式中的 （w-1）/ 等级 + 1
            
            // 顶点行数
            int vertivesPerLine = (width - 1) / meshSimplificationIncrement + 1;
            
            MeshData meshData = new MeshData(vertivesPerLine, vertivesPerLine);
            int vertexIndex = 0;
            
            for (int y = 0; y < height; y+= meshSimplificationIncrement)
            {
                for (int x = 0; x < width; x+=meshSimplificationIncrement)
                {
                    meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, meshHeightCurve.Evaluate(heightMap[x, y]) *heightMultiplier, topLeftZ - y);
                    meshData.uvs[vertexIndex] = new Vector2(x / (float) width, y / (float) height);
                    if (x < width-1 && y<height-1)
                    {
                        //============================
                        // 绘制两个三角形， 第一个三角形顶点索引(i, i+w+1, i+w)
                        //                第二个三角形顶点缩影(i+w+1, i, i+1)
                        // i   __ i+1
                        //    |\ |
                        // i+w|_\|i+w+1
                        //============================
                        meshData.AddTriangle(vertexIndex, vertexIndex+vertivesPerLine+1, vertexIndex+vertivesPerLine);
                        meshData.AddTriangle(vertexIndex+vertivesPerLine+1, vertexIndex, vertexIndex+1);
                    }
                    vertexIndex++;
                }
            }

            return meshData;
        }
    }
    
            
    public class MeshData
    {
        public Vector3[] vertices;
        public int[] triangles;
        public Vector2[] uvs;
        private int trianglesIndex;
        public MeshData(int meshWidth, int meshHeight)
        {
            vertices = new Vector3[meshWidth * meshHeight];
            uvs = new Vector2[meshWidth * meshHeight];
            triangles = new int[(meshWidth-1)*(meshHeight-1)*6];
        }

        public void AddTriangle(int a, int b, int c)
        { 
            triangles[trianglesIndex] = a;
            triangles[trianglesIndex+1] = b;
            triangles[trianglesIndex+2] = c;
                
            trianglesIndex += 3;
        }

        public Mesh createMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            return mesh;
        }

    }

