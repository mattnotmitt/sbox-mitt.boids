using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Sandbox;
using Sandbox.Internal;

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
	private const float MaxSpeed = 500f;

	public override Vector3 Velocity
	{
		get
		{
			return base.Velocity;
		}
		set
		{
			Rotation = Rotation.LookAt(value).RotateAroundAxis(Vector3.OneY, 90);
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
		//Scale = 0.5f;

		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );
		PhysicsBody.GravityEnabled = false;
		EnableAllCollisions = false;
		CollisionGroup = CollisionGroup.Never;
		SetInteractsExclude( CollisionLayer.All );
	}

	[Event( "server.tick" )]
	public void Behaviour()
	{
		var nbs = _mgr.Neighbours( _id );
		// DebugOverlay.Line( Position, Position + (Velocity * 0.1f), Color.Magenta, Time.Delta );
		// DebugOverlay.Sphere( Position, 50f, Color.Green.WithAlpha(0.5f), duration: Time.Delta );
		// DebugOverlay.Sphere( Position, 20f, Color.Cyan.WithAlpha(0.5f), duration: Time.Delta );
		if ( nbs.Count > 0 )
		{

			// Get resultant vectors from each of the rules
			// ChangeColorBasedOnNeighbours(nbs);
			var tmpVel = Velocity;
			var accel = Vector3.Zero;
			var nbStats = GetNeighbourStats( nbs );
			// DebugOverlay.Line( Position, Position + (nbStats.AvAlignment.Normal * 50f), Color.Red, Time.Delta);
			// DebugOverlay.Line( Position, nbStats.AvPosition / nbs.Count, Color.Orange, Time.Delta );
			// DebugOverlay.Line( Position, Position + (nbStats.Avoidance.Normal * 50f), Color.Black, Time.Delta);
			accel += SteerTowards( nbStats.AvAlignment );
			accel += SteerTowards( nbStats.AvPosition / nbs.Count - Position );
			accel += SteerTowards( nbStats.Avoidance ) * 1.3f;
			tmpVel += accel * Time.Delta;
			var speed = tmpVel.Length;
			var dir = tmpVel / speed;
			Velocity = dir * Math.Clamp( speed, 200, MaxSpeed );
		}

		KeepConstrained();
		_mgr.UpdatePosition( _id, Position );
	}

	private void ChangeColorBasedOnNeighbours(ICollection nbs)
	{
		var numNbs = nbs.Count;
		var colorScale = Math.Clamp( numNbs * 20, 0, 255 );
		RenderColor = new Color32( (byte) colorScale, (byte) (colorScale / 2), (byte) (colorScale / 2) ).ToColor();
	}

	private void KeepConstrained()
	{
		var tempPos = Position;
		if ( Position.x > 512f )
		{
			tempPos.x = -480f;
		}
		else if ( Position.y > 512f )
		{
			tempPos.y = -480f;
		}
		else if ( Position.x < -512f )
		{
			tempPos.x = 480f;
		}
		else if ( Position.y < -512f )
		{
			tempPos.y = 480f;
		}
		else if ( Position.z < 0f )
		{
			tempPos.z = 988f;
		}
		else if ( Position.z > 1024f )
		{
			tempPos.z = 36f;
		}
		Position = tempPos;
		ResetInterpolation();
	}

	private NeighbourStats GetNeighbourStats( IReadOnlyCollection<BoidData> nbs )
	{
		var nbStats = new NeighbourStats( Vector3.Zero, Vector3.Zero, Vector3.Zero );
		foreach ( var nb in nbs )
		{
			var offset = nb.Position - Position;
			var dstSqr = offset.LengthSquared;
			nbStats.AvAlignment += nb.Entity.Velocity;
			nbStats.AvPosition += nb.Position;
			nbStats.Avoidance -= dstSqr < (35f * 35f) ? offset / dstSqr : Vector3.Zero;
		}

		return nbStats;
	}
	
	private Vector3 SteerTowards (Vector3 vector) {
		var v = vector.Normal * MaxSpeed - Velocity;
		return v.ClampLength(MaxSpeed);
	}
}
