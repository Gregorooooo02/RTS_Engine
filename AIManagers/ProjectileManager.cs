using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RTS_Engine.Components.AI;

namespace RTS_Engine;

public class ProjectileManager
{
    public enum ProjectileType
    {
        Arrow,
        Fireball,
        Cupboard
    }
    
    public List<Projectile> ActiveProjectiles = new();

    private Queue<GameObject> _queuedProjectiles = new();
    
    public void AddProjectile(ProjectileType type, Agent target, float speed, float minDist, float dmg, Vector3 startingPos)
    {
        GameObject newProjectile = new GameObject();
        MeshRenderer meshRenderer = new MeshRenderer(newProjectile);
        switch (type)
        {
            case ProjectileType.Arrow:
                meshRenderer.LoadModel("arrowProjectile");
                break;
            case ProjectileType.Fireball:
                meshRenderer.LoadModel("fireballProjectile");
                break;
            case ProjectileType.Cupboard:
                meshRenderer.LoadModel("cupboardProjectile");
                break;
        }
        newProjectile.AddComponent(meshRenderer);
        Projectile projectile = new Projectile(newProjectile, target, speed, dmg, minDist, startingPos);
        ActiveProjectiles.Add(projectile);
        newProjectile.AddComponent(projectile);
        _queuedProjectiles.Enqueue(newProjectile);
        
    }

    public void UpdateProjectiles()
    {
        for (int i = ActiveProjectiles.Count - 1; i >= 0 ; i--)
        {
            Projectile projectile = ActiveProjectiles[i];
            if (!projectile.Delete) continue;
            ActiveProjectiles.Remove(projectile);
            GameObject.ClearObject(projectile.ParentObject);
        }

        while (_queuedProjectiles.Count != 0)
        {
            SceneManager.Instance.CurrentScene.SceneRoot.AddChildObject(_queuedProjectiles.Dequeue());
        }
    }
}