using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Physics.Systems;
using UnityEngine;

public class CustomBootstrap : ICustomBootstrap
{
    private World world;

    public bool Initialize(string defaultWorldName)
    {
        world = new World(defaultWorldName);
        World.DefaultGameObjectInjectionWorld = world;

        var systems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default);
        DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, systems);

        AddSystemsToInitializationGroup();
        AddSystemsToSimulationGroup();
        AddSystemsToPhysicsGroup();
        AddSystemsToPresentationGroup();

        ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(world);

        return true;
    }

    private void AddSystemsToInitializationGroup()
    {
        var initGroup = world.GetOrCreateSystemManaged<InitializationSystemGroup>();

        AddSystemManaged<GridInputSystem>(initGroup);
        AddSystem<BuildGridSystem>(initGroup);
        AddSystem<GridCleanupSystem>(initGroup);

        initGroup.SortSystems();
    }

    private void AddSystemsToSimulationGroup()
    {
        var simultationGroup = world.GetOrCreateSystemManaged<SimulationSystemGroup>();

        AddSystem<GridEventProviderSystem>(simultationGroup);
        AddSystem<TileSpawnerSystem>(simultationGroup);
        AddSystemManaged<TileTextSpawnerSystem>(simultationGroup);

        simultationGroup.SortSystems();

        var lateSimulation = world.GetOrCreateSystemManaged<LateSimulationSystemGroup>();

        AddSystem<GridEventCleanupSystem>(lateSimulation);

        lateSimulation.SortSystems();
    }

    private void AddSystemsToPhysicsGroup()
    {
        var physicsGroup = world.GetOrCreateSystemManaged<PhysicsSystemGroup>();

        AddSystem<SpringSystem>(physicsGroup);
        AddSystem<MoveTowardsSystem>(physicsGroup);
        AddSystem<HoverBoardSystem>(physicsGroup);

        physicsGroup.SortSystems();
    }

    private void AddSystemsToPresentationGroup()
    {
        var presentationGroup = world.GetOrCreateSystemManaged<PresentationSystemGroup>();

        presentationGroup.SortSystems();
    }

    private void AddSystem<T>(ComponentSystemGroup systemGroup) where T : unmanaged, ISystem
    {
        var system = world.CreateSystem<T>();
        systemGroup.AddSystemToUpdateList(system);
    }

    private void AddSystemManaged<T>(ComponentSystemGroup systemGroup) where T : ComponentSystemBase, new()
    {
        var system = world.CreateSystem<T>();
        systemGroup.AddSystemToUpdateList(system);
    }
}
