using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeMassPlacerOnTerrain : TreeMassPlacer {
    [SerializeField] private int _numberOfTerrainsToPlantTrees;
    [SerializeField] private List<Terrain> _mTerrains;
    [SerializeField] private Transform _terrains;
    public Transform terrains {
        get => _terrains;
        set => _terrains = value;
    }
    public int numberOfTerrainsToPlantTrees {
        get => _numberOfTerrainsToPlantTrees;
        set => _numberOfTerrainsToPlantTrees = value;
    }

    private void Awake() {
        int count = 0;
        foreach(Transform transform in _terrains) {
            if(_numberOfTerrainsToPlantTrees > count++)
                _mTerrains.Add(transform.GetComponent<Terrain>());
            else break;
        }

        repeatFor = _mTerrains.Count;
        for(int i = 0; i < _mTerrains.Count; ++i) {
            sizes.Add(_mTerrains[i].terrainData.size);
            positions.Add(_mTerrains[i].transform.position);
        }
    }
}
