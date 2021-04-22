using System.Collections.Generic;

namespace Boids;

public struct BoidData
{
	public BoidData( int id, BoidEntity entity )
	{
		Id = id;
		Entity = entity;
		Position = Vector3.Zero;
		Neighbours = new List<BoidData>();
	}

	public readonly int Id;
	public readonly BoidEntity Entity;
	public Vector3 Position;
	public List<BoidData> Neighbours;
}
public interface IFlockManager
{
	List<BoidData> Neighbours( int id );
	void UpdatePosition( int id, Vector3 pos );
}
