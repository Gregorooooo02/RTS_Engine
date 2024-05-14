using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace RTS_Engine;

public class ThirdScene : Scene
{
    CollisionManager _sceneCollisionManager;
    
    public override void Initialize()
    {
        Name = "ThirdScene";
        SceneRoot = new GameObject();

        GameObject gameObject = new GameObject();
        GameObject gameObject2 = new GameObject();
        GameObject gameObject3 = new GameObject();
        GameObject[] gameObjects = {gameObject, gameObject2};
        List<GameObject> gameObjectsList = new List<GameObject>(gameObjects);
        CollisionManager collisionManager = new CollisionManager(gameObjectsList);
        _sceneCollisionManager = collisionManager;
    
        gameObject.AddComponent<MeshRenderer>();
        gameObject.GetComponent<MeshRenderer>().LoadModel("monke");
        gameObject.AddComponent<Collider>();
        
        gameObject2.AddComponent<MeshRenderer>();
        gameObject2.GetComponent<MeshRenderer>().LoadModel("monke"); 
        gameObject2.AddComponent<Collider>();
        
        gameObject3.AddComponent<Camera>();
        gameObject3.AddComponent<Layer>();
        gameObject3.GetComponent<Layer>().SetLayerType("UI");
        
        gameObject.Transform.SetLocalPosition(new Vector3(-20.0f, 0, 0));
        gameObject.Transform.SetLocalScale(new Vector3(1.0f, 1.0f, 1.0f));
        gameObject3.Transform.SetLocalPosition(new Vector3(0.0f, 25.0f, 25.0f));
        
        gameObject2.Transform.SetLocalPosition(new Vector3(0, 0, 0));
        gameObject2.Transform.SetLocalScale(new Vector3(1.0f, 1.0f, 1.0f));
        SceneRoot.AddChildObject(gameObject);
        SceneRoot.AddChildObject(gameObject2);
        SceneRoot.AddChildObject(gameObject3);
    }
    
    public override void Update(GameTime gameTime)
    {
        SceneRoot.Update();
        _sceneCollisionManager.CheckColissions();
        
        SceneRoot.Children[0].Transform.Move(new Vector3(0.05f, 0, 0));
        SceneRoot.Children[1].Transform.Move(new Vector3(0.0f, 0, 0));
        
        // Collision detection
        if (SceneRoot.Children[0].GetComponent<Collider>().isColliding == true)
        {
            SceneRoot.Children[0].Transform.Move(new Vector3(-0.05f, 0f, 0f));
            SceneRoot.Children[1].Transform.Move(new Vector3(0.05f, 0f, 0f));
        }
        
        // Inputs for moving the camera
        if (InputManager.Instance.GetAction(GameAction.FORWARD)?.state == ActionState.PRESSED ||
            InputManager.Instance.MousePosition.Y < 20.0f)
        {
            SceneRoot.Children[2].Transform.Move(new Vector3(0.0f, 0.0f, -0.15f));
        }
        
        if (InputManager.Instance.GetAction(GameAction.BACKWARD)?.state == ActionState.PRESSED ||
            InputManager.Instance.MousePosition.Y > 880.0f )
        {
            SceneRoot.Children[2].Transform.Move(new Vector3(0.0f, 0.0f, 0.15f));
        }
        
        if (InputManager.Instance.GetAction(GameAction.LEFT)?.state == ActionState.PRESSED ||
            InputManager.Instance.MousePosition.X < 20.0f)
        {
            SceneRoot.Children[2].Transform.Move(new Vector3(-0.15f, 0.0f, 0.0f));
        }
        
        if (InputManager.Instance.GetAction(GameAction.RIGHT)?.state == ActionState.PRESSED ||
            InputManager.Instance.MousePosition.X > 1420.0f)
        {
            SceneRoot.Children[2].Transform.Move(new Vector3(0.15f, 0.0f, 0.0f));
        }
        
        if (InputManager.Instance.GetAction(GameAction.UP)?.state == ActionState.PRESSED)
        {
            SceneRoot.Children[2].GetComponent<Camera>().fovDegrees += -0.5f;
        }
        
        if (InputManager.Instance.GetAction(GameAction.DOWN)?.state == ActionState.PRESSED)
        {
            SceneRoot.Children[2].GetComponent<Camera>().fovDegrees += 0.5f;
        }
        
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

    public override void DrawHierarchy()
    {
        SceneRoot.DrawTree();
    }

    public override void SaveToFile()
    {
        StringBuilder builder = new StringBuilder();
        // Append scene metadata here if necessary

        builder.Append(SceneRoot.SaveSceneToXml());
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