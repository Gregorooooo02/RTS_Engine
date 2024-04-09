using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RTS_Engine;

public class CollisionManager
{
    private static AssetManager _instance;
    private List<GameObject> _gameObjects;
    public CollisionManager(List<GameObject> gameObjects)
    {
        _gameObjects = gameObjects;
    }
    
    public void CheckColissions()
    {
        foreach (GameObject gameObject in _gameObjects)
        {
            if (!gameObject.Active) continue;
            foreach (GameObject otherGameObject in _gameObjects)
            {
                if (!otherGameObject.Active) continue;
                if (gameObject == otherGameObject) continue;
                if (IsCollision(gameObject, otherGameObject, gameObject.Transform.ModelMatrix, otherGameObject.Transform.ModelMatrix))
                {
                    gameObject.GetComponent<Collider>().isColliding = true;
                    otherGameObject.GetComponent<Collider>().isColliding = true;  
                }
                else
                {
                    gameObject.GetComponent<Collider>().isColliding = false;
                    otherGameObject.GetComponent<Collider>().isColliding = false;
                }
                
            }
        }
    }
    
    public bool IsCollision(GameObject gameObject, GameObject otherGameObject, Matrix model1, Matrix model2)
    {
        gameObject.GetComponent<MeshRenderer>().GetModel();
        for (int meshIndex1 = 0; meshIndex1 < gameObject.GetComponent<MeshRenderer>().GetModel().Meshes.Count; meshIndex1++)
        {
            BoundingSphere sphere1 = gameObject.GetComponent<MeshRenderer>().GetModel().Meshes[meshIndex1].BoundingSphere;
            sphere1 = sphere1.Transform(model1);
 
            for (int meshIndex2 = 0; meshIndex2 < otherGameObject.GetComponent<MeshRenderer>().GetModel().Meshes.Count; meshIndex2++)
            {
                BoundingSphere sphere2 = otherGameObject.GetComponent<MeshRenderer>().GetModel().Meshes[meshIndex2].BoundingSphere;
                sphere2 = sphere2.Transform(model2);
 
                if (sphere1.Intersects(sphere2))
                    return true;
            }
        }
        return false;
    }
    
}