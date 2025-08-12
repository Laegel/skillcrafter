using System;
using Godot;

public class Commander
{
    public static bool ExecuteCommand(string rawCommand)
    {
        if (!rawCommand.EndsWith(";"))
        {
            return false;
        }
        var parsed = rawCommand[..(rawCommand.Length - 1)].Split(' ');
        var command = parsed[0].ToLowerInvariant();
        
        var args = parsed[1..];


        GD.Print($"Executing command: {command} with args: {Json.Stringify(args)}");

        switch (command)
        {
            case "open_menu":
                OpenMenu(args);
                return true;
            case "close_menu":
                CloseMenu();
                return true;
            case "show_message":
                ShowMessage(args);
                return true;
            default:
                GD.PrintErr($"Unknown command: {command}");
                break;
        }
        return false;
    }

    // open_menu 2;
    private static void OpenMenu(object[] args)
    {
        GD.Print("Opening menu with value: ", args[0]);

        var menu = Enum.Parse<Menus>((string)args[0]);
        var menuState = ServiceStorage.Resolve<MenuState>();
        menuState.currentMenu.Value = menu;
    }

    private static void CloseMenu()
    {
        GD.Print("Closing current menu");
        var menuState = ServiceStorage.Resolve<MenuState>();
        menuState.currentMenu.Value = Menus.None;
    }

    private static void ShowMessage(object[] args)
    {
        // Logic to show a message
        GD.Print("Showing message: ", args);
    }
}