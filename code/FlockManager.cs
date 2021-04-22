using System.Collections.Generic;
using System.Linq;

namespace Boids;
public class FlockManager : IFlockManager
{
	private const float MaxDistance = 20;
	private List<BoidData> _flock;
	private int _lastId;

	public FlockManager()
	{
		_flock = new List<BoidData>();
		_lastId = 0;
	}

	public void UpdatePosition( int id, Vector3 pos )
	{
		var boidData = _flock[id];
		boidData.Position = pos;
		_flock[id] = boidData;
		var nbIds = new List<int>();
		var tmpNbs = new List<BoidData>( boidData.Neighbours );
		foreach ( var nb in tmpNbs )
		{
			nbIds.Add( nb.Id );
			if ( Vector3.DistanceBetween( pos, nb.Position ) > MaxDistance )
			{
				boidData.Neighbours.Remove( nb );
			}
		}

		foreach ( (var boid, var i) in _flock.Select( ( value, i ) => (value, i) ) )
		{
			if ( i == id || nbIds.Contains( id ) )
			{
				continue;
			}

			var distance = Vector3.DistanceBetween( pos, boid.Position );
			if ( distance >= MaxDistance )
			{
				continue;
			}

			boidData.Neighbours.Add( boid );
			boid.Neighbours.Add( boidData );
		}
	}

	public List<BoidData> Neighbours( int id )
	{
		return _flock[id].Neighbours;
	}

	public BoidEntity NewBoid()
	{
		var boid = new BoidEntity( this, _lastId );
		_flock.Add( new BoidData( _lastId, boid ) );
		_lastId++;
		return boid;
	}

	public void DeleteAll()
	{
		if ( _flock.Count == 0 )
		{
			return;
		}

		foreach ( var boid in _flock )
		{
			boid.Entity.Delete();
		}

		_flock = new List<BoidData>();
		_lastId = 0;
	}

	public BoidEntity GetBoid( int id )
	{
		return id > _lastId ? null : _flock[id].Entity;
	}
}
