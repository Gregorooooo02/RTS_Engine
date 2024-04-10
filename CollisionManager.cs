using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RTS_Engine;

public class CollisionManager
{
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
                if (gameObject.GetComponent<Collider>() == null || otherGameObject.GetComponent<Collider>() == null) continue;
                
                if (gameObject.GetComponent<Collider>().sphere.Intersects(otherGameObject.GetComponent<Collider>().sphere))
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
}