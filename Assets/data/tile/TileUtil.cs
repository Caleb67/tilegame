
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileUtil
{
    public static (bool success, IEnumerable<TileLocation> locations) TryGetAllRelative(
        TileLocation location, Vector3Int[] relativePositions
    ) {
        var results = relativePositions
            .Select(pos => {
                var success = ChunkManager.s_instance.TryGetTileLocation(
                    location.positionInWorld + pos, 
                    location.isBackground,
                    out var relativeLocation
                );
                return new {
                    success, relativeLocation
                };
            })
            .Where(locationInfo => locationInfo.success)
            .Select(locationInfo => locationInfo.relativeLocation);
        
        var success = results.Count() == relativePositions.Count();
        return (success, results);
    }
}