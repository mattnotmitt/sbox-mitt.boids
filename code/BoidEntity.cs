using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Sandbox;

namespace Boids;

struct NeighbourStats
{
	public Vector3 AvAlignment;
	public Vector3 AvPosition;
	public Vector3 Avoidance;

	public NeighbourStats( Vector3 align, Vector3 pos, Vector3 avoid )
	{
		AvAlignment = align;
		AvPosition = pos;
		Avoidance = avoid;
	}
}

public class BoidEntity : ModelEntity
{
	private readonly int _id;
	private readonly IFlockManager _mgr;

	public override Vector3 Velocity
	{
		get
		{
			return base.Velocity;
		}
		set
		{
			Rotation = Rotation.LookAt( value ).RotateAroundAxis( Vector3.OneY, 90 );
			base.Velocity = value;
		}
	}

	public BoidEntity()
	{
	}
	
	public BoidEntity( IFlockManager mgr, int id )
	{
		_mgr = mgr;
		_id = id;
	}

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizen_props/roadcone01.vmdl" );
		//RenderColor = Color.Black;
		Scale = 0.5f;

		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );
		PhysicsBody.GravityEnabled = false;
		EnableAllCollisions = false;
	}

	[Event( "server.tick" )]
	public void Behaviour()
	{
		var nbs = _mgr.Neighbours( _id );
		KeepConstrained();
		_mgr.UpdatePosition( _id, Position );
		// Get resultant vectors from each of the rules
		// ChangeColorBasedOnNeighbours(nbs);
		var tmpVel = Velocity;
		var accel = Vector3.Zero;
		var nbStats = GetNeighbourStats( nbs );
		accel += SteerTowards(nbStats.AvAlignment);
		accel += SteerTowards( nbStats.AvPosition/nbs.Count - Position);
		accel += SteerTowards( nbStats.Avoidance );
		tmpVel += accel * Time.Delta;
		var speed = tmpVel.Length;
		var dir = tmpVel / speed;
		Velocity = dir * Math.Clamp(speed, 150, 400);
	}

	private void ChangeColorBasedOnNeighbours(ICollection nbs)
	{
		var numNbs = nbs.Count;
		var colorScale = Math.Clamp( numNbs * 20, 0, 255 );
		RenderColor = new Color32( (byte) colorScale, (byte) (colorScale / 2), (byte) (colorScale / 2) ).ToColor();
	}

	private void KeepConstrained()
	{
		if ( Position.x > 512f )
		{
			var tempPos = Position;
			tempPos.x = -480f;
			Position = tempPos;
		}
		else if ( Position.y > 512f )
		{
			var tempPos = Position;
			tempPos.y = -480f;
			Position = tempPos;
		}
		else if ( Position.x < -512f )
		{
			var tempPos = Position;
			tempPos.x = 480f;
			Position = tempPos;
		}
		else if ( Position.y < -512f )
		{
			var tempPos = Position;
			tempPos.y = 480f;
			Position = tempPos;
		}
	}

	private NeighbourStats GetNeighbourStats( IReadOnlyCollection<BoidData> nbs )
	{
		var nbStats = new NeighbourStats( Vector3.Zero, Vector3.Zero, Vector3.Zero );
		foreach ( var nb in nbs )
		{
			var offset = Position - nb.Position;
			var dstSqr = offset.LengthSquared;
			nbStats.AvAlignment += nb.Entity.Velocity;
			nbStats.AvPosition += nb.Position;
			nbStats.Avoidance -= dstSqr < 10f * 10f ? offset / dstSqr : Vector3.Zero;
		}

		return nbStats;
	}
	
	private Vector3 SteerTowards (Vector3 vector) {
		var v = vector.Normal * 400 - Velocity;
		return v.ClampLength(400);
	}
}
