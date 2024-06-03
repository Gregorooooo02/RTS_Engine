﻿using System.Collections.Generic;
using RTS_Engine.Components.AI;

namespace RTS_Engine;

public class AgentsManager
{
    public readonly PatrolPathManager PatrolManager = new();
    
    public List<Agent> Units = new();
    public List<Agent> Enemies = new();
}