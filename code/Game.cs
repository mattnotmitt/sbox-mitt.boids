using System;
using Sandbox;

//
// You don't need to put things in a namespace, but it doesn't hurt.
//
namespace Boids;

/// <summary>
///     This is your game class. This is an entity that is created serverside when
///     the game starts, and is replicated to the client.
///     You can use this to create things like HUDs and declare which player class
///     to use for spawned players.
/// </summary>
[Library( "boids", Title = "Boids" )]
public class Game : Sandbox.Game
{
	public readonly FlockManager FlockMgr;
	public static new Game Current { get; protected set; }

	public Game()
	{
		Current = this;
		if ( IsServer )
		{
			Log.Info( "My Gamemode Has Created Serverside!" );

			// Create a HUD entity. This entity is globally networked
			// and when it is created clientside it creates the actual
			// UI panels. You don't have to create your HUD via an entity,
			// this just feels like a nice neat way to do it.
			new MinimalHudEntity();
			FlockMgr = new FlockManager();
		}

		if ( IsClient )
		{
			Log.Info( "My Gamemode Has Created Clientside!" );
		}
	}

	/// <summary>
	///     A client has joined the server. Make them a pawn to play with
	/// </summary>
	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		var player = new Player();
		client.Pawn = player;

		player.Respawn();
		RefreshFlock();
	}

	[AdminCmd( "refresh_flock" )]
	public static void RefreshFlock()
	{
		if ( !Current.IsServer )
		{
			return;
		}

		Current.FlockMgr.DeleteAll();

		for ( var i = 0; i < 249; i++ )
		{
			var boid = Current.FlockMgr.NewBoid();
			boid.Position = new Vector3( Rand.Float( -300f, 300f ), Rand.Float( -300f, 300f ), 0 );
			var phi = 2 * Math.PI * Rand.Float( 0, 1 );
			boid.PhysicsGroup.Velocity =
				new Vector3( (float)Math.Cos( phi ) * 500f, (float)Math.Sin( phi ) * 500f );
		}

		Current.FlockMgr.GetBoid( 0 ).RenderColor = Color.Red;
	}
}
