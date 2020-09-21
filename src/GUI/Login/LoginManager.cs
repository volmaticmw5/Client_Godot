using Godot;
using System;

public class LoginManager : Node
{
	private void _on_loginBtn()
	{
		Client.instance.ConnectToAuthenticationServer();
	}
}


