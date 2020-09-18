using Godot;
using System;

public class LoginManager : Node
{
	private void _on_loginBtn()
	{
		Client theClient = GetTree().Root.GetNode<Client>("Game/Client");
		theClient.ConnectToAuthenticationServer();
	}
}


