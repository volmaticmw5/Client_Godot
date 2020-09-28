using Godot;
using System;

public class LoginManager : Node
{
	[Export] public NodePath usernameNode;
	[Export] public NodePath passwordNode;

	public static string CurrentUsername;
	public static string CurrentPassword;

	private void autoLogin()
	{
		CurrentUsername = "teste";
		CurrentPassword = "teste";
		Client.instance.ConnectToAuthenticationServer();
	}

	private void _on_loginBtn()
	{
		CurrentUsername = GetNode<LineEdit>(usernameNode).Text;
		CurrentPassword = GetNode<LineEdit>(passwordNode).Text;

		Client.instance.ConnectToAuthenticationServer();
	}
}
