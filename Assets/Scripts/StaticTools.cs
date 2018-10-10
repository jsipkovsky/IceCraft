using UnityEngine;

// Class with static Game data & methods

public static class StaticTools  {

    //--------------------------------------

    public static bool LoadedGame;   // Determine if New Game/Load Game should started
    public static GameData GameData; // Loaded Game data

    static Material lineMaterial;    // Material for drawing new Player Cube preview

    //--------------------------------------

    // Draws preview of possile new Cube

    public static void DrawSquare(Vector3 position, float size, Color color)
    {
        position -= new Vector3(0.5f, 0.5f, 0.5f); // draw starting at Cube.transform.position
        CreateLineMaterial();
        lineMaterial.SetPass(0);
        GL.PushMatrix();
        GL.Begin(GL.LINES);

        GL.Color(color);

        //-----------------------
        // Draw all Cube lines

        DrawLine(position, new Vector3(position.x + size, position.y, position.z), color);

        DrawLine(position,new Vector3(position.x, position.y, position.z + size), color);

        DrawLine(new Vector3(position.x + size, position.y, position.z),
                    new Vector3(position.x + size, position.y, position.z + size),
                    color);

        DrawLine(new Vector3(position.x, position.y, position.z + size),
                    new Vector3(position.x + size, position.y, position.z + size),
                    color);

        DrawLine(position, new Vector3(position.x, position.y + size, position.z), color);

        DrawLine(new Vector3(position.x, position.y + size, position.z),
            new Vector3(position.x + size, position.y + size, position.z),
            color);

        DrawLine(new Vector3(position.x + size, position.y + size, position.z),
            new Vector3(position.x + size, position.y, position.z),
            color);

        DrawLine(new Vector3(position.x, position.y + size, position.z),
            new Vector3(position.x, position.y + size, position.z + size),
            color);

        DrawLine(new Vector3(position.x, position.y + size, position.z + size),
            new Vector3(position.x, position.y, position.z + size),
            color);

        DrawLine(new Vector3(position.x, position.y + size, position.z + size),
            new Vector3(position.x, position.y + size, position.z + size),
            color);

        DrawLine(new Vector3(position.x, position.y + size, position.z + size),
            new Vector3(position.x + size, position.y + size, position.z + size),
            color);

        DrawLine(new Vector3(position.x + size, position.y + size, position.z + size),
            new Vector3(position.x + size, position.y, position.z + size),
            color);

        DrawLine(new Vector3(position.x + size, position.y + size, position.z + size),
            new Vector3(position.x + size, position.y + size, position.z),
            color);
     
        GL.End();
        GL.PopMatrix();
    }

    // Draws one each line of Cube geometry

    public static void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        GL.Vertex3(start.x, start.y, start.z);
        GL.Vertex3(end.x, end.y, end.z);
    }

    // Create Line material

    static void CreateLineMaterial()
    {
        if (!lineMaterial) 
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            //// Turn on alpha blending
            //lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            //lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            //// Turn backface culling off
            //lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            //// Turn off depth writes
            //lineMaterial.SetInt("_ZWrite", 0);
        }
    }
}
