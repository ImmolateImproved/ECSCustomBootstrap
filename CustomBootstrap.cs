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

        AddSystemManaged<ManagedSystem>(initGroup);
        AddSystem<UnmanagedSystem>(initGroup);

        initGroup.SortSystems();
    }

    private void AddSystemsToSimulationGroup()
    {
        var simultationGroup = world.GetOrCreateSystemManaged<SimulationSystemGroup>();

        AddSystemManaged<ManagedSystem>(simultationGroup);
        AddSystem<UnmanagedSystem>(simultationGroup);

        simultationGroup.SortSystems();

        var lateSimulation = world.GetOrCreateSystemManaged<LateSimulationSystemGroup>();

        AddSystemManaged<ManagedSystem>(lateSimulation);
        AddSystem<UnmanagedSystem>(lateSimulation);

        lateSimulation.SortSystems();
    }

    private void AddSystemsToPhysicsGroup()
    {
        var physicsGroup = world.GetOrCreateSystemManaged<PhysicsSystemGroup>();

        AddSystemManaged<ManagedSystem>(physicsGroup);
        AddSystem<UnmanagedSystem>(physicsGroup);

        physicsGroup.SortSystems();
    }

    private void AddSystemsToPresentationGroup()
    {
        var presentationGroup = world.GetOrCreateSystemManaged<PresentationSystemGroup>();

        AddSystemManaged<ManagedSystem>(presentationGroup);
        AddSystem<UnmanagedSystem>(presentationGroup);

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
