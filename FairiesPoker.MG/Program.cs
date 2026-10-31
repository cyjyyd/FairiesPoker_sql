using var game = new FairiesPoker.MG.Game1();
foreach (string argument in args)
{
    if (argument == "--effects-preview") game.StartupView = "effects";
    if (argument == "--about") game.StartupView = "about";
}
game.Run();
