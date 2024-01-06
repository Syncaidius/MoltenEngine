using Molten.Graphics;

namespace Molten.Examples;

public static class SampleVertexData
{
    /// <summary>
    /// Un-indexed vertex data for a textured cube.
    /// </summary>
    public static readonly VertexTexture[] TexturedCube = new VertexTexture[]{
            new VertexTexture(new Vector3F(-1,-1,-1), new Vector2F(0,1)), //front
            new VertexTexture(new Vector3F(-1,1,-1), new Vector2F(0,0)),
            new VertexTexture(new Vector3F(1,1,-1), new Vector2F(1,0)),
            new VertexTexture(new Vector3F(-1,-1,-1), new Vector2F(0,1)),
            new VertexTexture(new Vector3F(1,1,-1), new Vector2F(1, 0)),
            new VertexTexture(new Vector3F(1,-1,-1), new Vector2F(1,1)),

            new VertexTexture(new Vector3F(-1,-1,1), new Vector2F(1,0)), //back
            new VertexTexture(new Vector3F(1,1,1), new Vector2F(0,1)),
            new VertexTexture(new Vector3F(-1,1,1), new Vector2F(1,1)),
            new VertexTexture(new Vector3F(-1,-1,1), new Vector2F(1,0)),
            new VertexTexture(new Vector3F(1,-1,1), new Vector2F(0, 0)),
            new VertexTexture(new Vector3F(1,1,1), new Vector2F(0,1)),

            new VertexTexture(new Vector3F(-1,1,-1), new Vector2F(0,1)), //top
            new VertexTexture(new Vector3F(-1,1,1), new Vector2F(0,0)),
            new VertexTexture(new Vector3F(1,1,1), new Vector2F(1,0)),
            new VertexTexture(new Vector3F(-1,1,-1), new Vector2F(0,1)),
            new VertexTexture(new Vector3F(1,1,1), new Vector2F(1, 0)),
            new VertexTexture(new Vector3F(1,1,-1), new Vector2F(1,1)),

            new VertexTexture(new Vector3F(-1,-1,-1), new Vector2F(1,0)), //bottom
            new VertexTexture(new Vector3F(1,-1,1), new Vector2F(0,1)),
            new VertexTexture(new Vector3F(-1,-1,1), new Vector2F(1,1)),
            new VertexTexture(new Vector3F(-1,-1,-1), new Vector2F(1,0)),
            new VertexTexture(new Vector3F(1,-1,-1), new Vector2F(0, 0)),
            new VertexTexture(new Vector3F(1,-1,1), new Vector2F(0,1)),

            new VertexTexture(new Vector3F(-1,-1,-1), new Vector2F(0,1)), //left
            new VertexTexture(new Vector3F(-1,-1,1), new Vector2F(0,0)),
            new VertexTexture(new Vector3F(-1,1,1), new Vector2F(1,0)),
            new VertexTexture(new Vector3F(-1,-1,-1), new Vector2F(0,1)),
            new VertexTexture(new Vector3F(-1,1,1), new Vector2F(1, 0)),
            new VertexTexture(new Vector3F(-1,1,-1), new Vector2F(1,1)),

            new VertexTexture(new Vector3F(1,-1,-1), new Vector2F(1,0)), //right
            new VertexTexture(new Vector3F(1,1,1), new Vector2F(0,1)),
            new VertexTexture(new Vector3F(1,-1,1), new Vector2F(1,1)),
            new VertexTexture(new Vector3F(1,-1,-1), new Vector2F(1,0)),
            new VertexTexture(new Vector3F(1,1,-1), new Vector2F(0, 0)),
            new VertexTexture(new Vector3F(1,1,1), new Vector2F(0,1)),
        };

    public static readonly VertexColor[] IndexedColorCubeVertices = new VertexColor[]{
            new VertexColor(new Vector3F(-1,-1,-1), Color.Red), //front
            new VertexColor(new Vector3F(-1,1,-1), Color.Red),
            new VertexColor(new Vector3F(1,1,-1), Color.Red),
            new VertexColor(new Vector3F(1,-1,-1), Color.Red),

            new VertexColor(new Vector3F(-1,-1,1), Color.Green), //back
            new VertexColor(new Vector3F(1,1,1), Color.Green),
            new VertexColor(new Vector3F(-1,1,1), Color.Green),
            new VertexColor(new Vector3F(1,-1,1), Color.Green),

            new VertexColor(new Vector3F(-1,1,-1), Color.Blue), //top
            new VertexColor(new Vector3F(-1,1,1), Color.Blue),
            new VertexColor(new Vector3F(1,1,1), Color.Blue),
            new VertexColor(new Vector3F(1,1,-1), Color.Blue),

            new VertexColor(new Vector3F(-1,-1,-1), Color.Yellow), //bottom
            new VertexColor(new Vector3F(1,-1,1), Color.Yellow),
            new VertexColor(new Vector3F(-1,-1,1), Color.Yellow),
            new VertexColor(new Vector3F(1,-1,-1), Color.Yellow),

            new VertexColor(new Vector3F(-1,-1,-1), Color.Purple), //left
            new VertexColor(new Vector3F(-1,-1,1), Color.Purple),
            new VertexColor(new Vector3F(-1,1,1), Color.Purple),
            new VertexColor(new Vector3F(-1,1,-1), Color.Purple),

            new VertexColor(new Vector3F(1,-1,-1), Color.White), //right
            new VertexColor(new Vector3F(1,1,1), Color.White),
            new VertexColor(new Vector3F(1,-1,1), Color.White),
            new VertexColor(new Vector3F(1,1,-1), Color.White),
        };

    public static readonly uint[] CubeIndices = new uint[]{
            0, 1, 2, 0, 2, 3,
            4, 5, 6, 4, 7, 5,
            8, 9, 10, 8, 10, 11,
            12, 13, 14, 12, 15, 13,
            16,17,18, 16, 18, 19,
            20, 21, 22, 20, 23, 21,
        };

    /// <summary>
    /// Vertex data for a cube that is textured using a texture array. Each side uses a different texture 
    /// (between array element 0 and 2) in the appled texture array for demonstration purposes.
    /// </summary>
    public static readonly CubeArrayVertex[] TextureArrayCubeVertices = new CubeArrayVertex[]{
           new CubeArrayVertex(new Vector3F(-1,-1,-1), new Vector3F(0,1,0)), //front
           new CubeArrayVertex(new Vector3F(-1,1,-1), new Vector3F(0,0,0)),
           new CubeArrayVertex(new Vector3F(1,1,-1), new Vector3F(1,0,0)),
           new CubeArrayVertex(new Vector3F(-1,-1,-1), new Vector3F(0,1,0)),
           new CubeArrayVertex(new Vector3F(1,1,-1), new Vector3F(1, 0,0)),
           new CubeArrayVertex(new Vector3F(1,-1,-1), new Vector3F(1,1,0)),

           new CubeArrayVertex(new Vector3F(-1,-1,1), new Vector3F(1,0,1)), //back
           new CubeArrayVertex(new Vector3F(1,1,1), new Vector3F(0,1,1)),
           new CubeArrayVertex(new Vector3F(-1,1,1), new Vector3F(1,1,1)),
           new CubeArrayVertex(new Vector3F(-1,-1,1), new Vector3F(1,0,1)),
           new CubeArrayVertex(new Vector3F(1,-1,1), new Vector3F(0, 0,1)),
           new CubeArrayVertex(new Vector3F(1,1,1), new Vector3F(0,1,1)),

           new CubeArrayVertex(new Vector3F(-1,1,-1), new Vector3F(0,1,2)), //top
           new CubeArrayVertex(new Vector3F(-1,1,1), new Vector3F(0,0,2)),
           new CubeArrayVertex(new Vector3F(1,1,1), new Vector3F(1,0,2)),
           new CubeArrayVertex(new Vector3F(-1,1,-1), new Vector3F(0,1,2)),
           new CubeArrayVertex(new Vector3F(1,1,1), new Vector3F(1, 0,2)),
           new CubeArrayVertex(new Vector3F(1,1,-1), new Vector3F(1,1,2)),

           new CubeArrayVertex(new Vector3F(-1,-1,-1), new Vector3F(1,0,0)), //bottom
           new CubeArrayVertex(new Vector3F(1,-1,1), new Vector3F(0,1,0)),
           new CubeArrayVertex(new Vector3F(-1,-1,1), new Vector3F(1,1,0)),
           new CubeArrayVertex(new Vector3F(-1,-1,-1), new Vector3F(1,0,0)),
           new CubeArrayVertex(new Vector3F(1,-1,-1), new Vector3F(0, 0,0)),
           new CubeArrayVertex(new Vector3F(1,-1,1), new Vector3F(0,1,0)),

           new CubeArrayVertex(new Vector3F(-1,-1,-1), new Vector3F(0,1,1)), //left
           new CubeArrayVertex(new Vector3F(-1,-1,1), new Vector3F(0,0,1)),
           new CubeArrayVertex(new Vector3F(-1,1,1), new Vector3F(1,0,1)),
           new CubeArrayVertex(new Vector3F(-1,-1,-1), new Vector3F(0,1,1)),
           new CubeArrayVertex(new Vector3F(-1,1,1), new Vector3F(1, 0,1)),
           new CubeArrayVertex(new Vector3F(-1,1,-1), new Vector3F(1,1,1)),

           new CubeArrayVertex(new Vector3F(1,-1,-1), new Vector3F(1,0,2)), //right
           new CubeArrayVertex(new Vector3F(1,1,1), new Vector3F(0,1,2)),
           new CubeArrayVertex(new Vector3F(1,-1,1), new Vector3F(1,1,2)),
           new CubeArrayVertex(new Vector3F(1,-1,-1), new Vector3F(1,0,2)),
           new CubeArrayVertex(new Vector3F(1,1,-1), new Vector3F(0, 0,2)),
           new CubeArrayVertex(new Vector3F(1,1,1), new Vector3F(0,1,2)),
        };

    /// <summary>
    /// Vertex data for an untextured, vertex-coloured cube.
    /// </summary>
    public static readonly VertexColor[] ColoredCube = new VertexColor[]{
            new VertexColor(new Vector3F(-1,-1,-1), Color.Red), //front
            new VertexColor(new Vector3F(-1,1,-1), Color.Red),
            new VertexColor(new Vector3F(1,1,-1), Color.Red),
            new VertexColor(new Vector3F(-1,-1,-1), Color.Red),
            new VertexColor(new Vector3F(1,1,-1), Color.Red),
            new VertexColor(new Vector3F(1,-1,-1), Color.Red),

            new VertexColor(new Vector3F(-1,-1,1), Color.Blue), //back
            new VertexColor(new Vector3F(1,1,1), Color.Blue),
            new VertexColor(new Vector3F(-1,1,1), Color.Blue),
            new VertexColor(new Vector3F(-1,-1,1),Color.Blue),
            new VertexColor(new Vector3F(1,-1,1), Color.Blue),
            new VertexColor(new Vector3F(1,1,1), Color.Blue),

            new VertexColor(new Vector3F(-1,1,-1), Color.Yellow), //top
            new VertexColor(new Vector3F(-1,1,1), Color.Yellow),
            new VertexColor(new Vector3F(1,1,1), Color.Yellow),
            new VertexColor(new Vector3F(-1,1,-1), Color.Yellow),
            new VertexColor(new Vector3F(1,1,1), Color.Yellow),
            new VertexColor(new Vector3F(1,1,-1), Color.Yellow),

            new VertexColor(new Vector3F(-1,-1,-1), Color.Purple), //bottom
            new VertexColor(new Vector3F(1,-1,1), Color.Purple),
            new VertexColor(new Vector3F(-1,-1,1), Color.Purple),
            new VertexColor(new Vector3F(-1,-1,-1), Color.Purple),
            new VertexColor(new Vector3F(1,-1,-1), Color.Purple),
            new VertexColor(new Vector3F(1,-1,1), Color.Purple),

            new VertexColor(new Vector3F(-1,-1,-1), Color.Green), //left
            new VertexColor(new Vector3F(-1,-1,1), Color.Green),
            new VertexColor(new Vector3F(-1,1,1), Color.Green),
            new VertexColor(new Vector3F(-1,-1,-1), Color.Green),
            new VertexColor(new Vector3F(-1,1,1), Color.Green),
            new VertexColor(new Vector3F(-1,1,-1), Color.Green),

            new VertexColor(new Vector3F(1,-1,-1), Color.White), //right
            new VertexColor(new Vector3F(1,1,1), Color.White),
            new VertexColor(new Vector3F(1,-1,1), Color.White),
            new VertexColor(new Vector3F(1,-1,-1), Color.White),
            new VertexColor(new Vector3F(1,1,-1), Color.White),
            new VertexColor(new Vector3F(1,1,1), Color.White),
        };
}
