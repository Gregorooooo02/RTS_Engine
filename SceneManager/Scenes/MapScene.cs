using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ImGuiNET;
using RTS_Engine.Pathfinding;
using Point = System.Drawing.Point;

namespace RTS_Engine;

public class MapScene : Scene
{
    GameObject gameObject;
    GameObject worldObject;

    public override void Initialize()
    {
        Name = "MapScene";
        TempName = Name;
        SceneRoot = new GameObject();
        SceneRoot.Name = "Root";
        List<Point> finalPath = new List<Point>();
        
        gameObject = new GameObject();
        gameObject.Name = "MapTexture";
        gameObject.Transform.SetLocalScale(new Vector3(2, 2, 1));
        gameObject.Transform.SetLocalPosition(new Vector3(0, 0, 0));
        SceneRoot.AddChildObject(gameObject);
        
        // Pathfinding test
        gameObject.AddComponent<Pathfinder>();
        
        bool [,] test = new bool[10, 10];
        
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                test[i, j] = true;
            }
        }
        
        String [,] visualisation = new String[10, 10];
        
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                visualisation[i, j] = "0";
            }
        }
        
        SearchParameters parameters = new SearchParameters(new Point (6, 0), new Point(1, 6), test);
        finalPath = parameters.FindPath(gameObject.GetComponent<Pathfinder>().nodes);
        
        Console.WriteLine(finalPath.Count);
        foreach (var point in finalPath)
        {
            visualisation[point.X, point.Y] = "1";
        }

        foreach (var node in gameObject.GetComponent<Pathfinder>().nodes)
        {
            if (node.IsWalkable == false)
            {
                visualisation[node.Location.X, node.Location.Y] = "X";
            }
        }
        
        visualisation[parameters.StartLocation.X, parameters.StartLocation.Y] = "S";
        visualisation[parameters.EndLocation.X, parameters.EndLocation.Y] = "E";
        
        Console.WriteLine("Pathfinding visualisation");
        Console.WriteLine("Starting location: " + parameters.StartLocation);
        Console.WriteLine("Ending location: " + parameters.EndLocation);
        Console.WriteLine("X - unwalkable node");
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                Console.Write(visualisation[i, j] + " ");
            }
            Console.WriteLine();
        }
        // end of pathfinding test

        worldObject = new GameObject();
        worldObject.Name = "World";
        worldObject.Transform.SetLocalScale(new Vector3(1, 1, 1));
        worldObject.Transform.SetLocalPosition(new Vector3(-256, -100, -100));
        worldObject.AddComponent<WorldRenderer>();
        SceneRoot.AddChildObject(worldObject);
    }

    public override void Update(GameTime gameTime)
    {
        SceneRoot.Update();
    }

    public override void Activate()
    {
        SceneRoot.Active = true;
    }

    public override void Deactivate()
    {
        SceneRoot.Active = false;
    }

    public override void AddGameObject(GameObject gameObject)
    {
        SceneRoot.AddChildObject(gameObject);
    }

    public override void RemoveGameObject(GameObject gameObject)
    {
        SceneRoot.RemoveChildObject(gameObject);
    }
#if DEBUG
    public override void DrawHierarchy()
    {
        SceneRoot.DrawTree();
    }
#endif
    public override void SaveToFile()
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine(SceneRoot.SaveSceneToXml());
        XDocument scene = XDocument.Parse(builder.ToString());
#if _WINDOWS
        StreamWriter streamWriter = new StreamWriter("../../../Scenes/" + Name + ".xml");
#else
        StreamWriter streamWriter = new StreamWriter("Scenes/" + Name + ".xml");
#endif
        scene.Save(streamWriter);
        streamWriter.Close();
    }
}
