using Sandbox;

namespace Boids;

public class Player : Sandbox.Player
{
	public Player()
	{
		Camera = new FirstPersonCamera();
		Controller = new NoclipController();
		Transmit = TransmitType.Always;
	}
}
