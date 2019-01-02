#load "nuget:?package=Cake.LycheeOS.Scripts"

var target = Argument("target", "Build");

Setup(context =>
{
    InitialiseLycheeTools();
});

Task("Clean").Does(() =>
{
    CleanWorkspace("workspace");
});

Task("Create-Workspace").Does(() =>
{
    CreateWorkspace("workspace", "modules");
});

Task("Apply-Changes").Does(() =>
{
    RunInWorkspace("workspace", "useradd devuser -u 3000 -s /bin/bash -p \"$(openssl passwd -1 devuser)\" -m -g sudo");
});

Task("Export-Workspace").Does(() =>
{
    CleanWorkspaceContent("workspace");
    var stats = ExportWorkspace("workspace", "image", "modifications.json");
    Information($"Deletions: {stats.DeletionCount}");
    Information($"Additions: {stats.AdditionCount}");
    Information($"Modifications: {stats.ModificationCount}");
});

Task("Package-Module").Does(() =>
{
    RSync("module.json", "image/module.json", new RSyncSettings { Verbose = true, Archive = true});
    var moduleFile =  ParseJsonFromFile("module.json");
    StartProcess("mksquashfs", $"image output/{moduleFile["id"]}_{moduleFile["version"]}.lmod -b 1048576");
});


Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Create-Workspace")
    .IsDependentOn("Apply-Changes")
    .IsDependentOn("Export-Workspace")
    .IsDependentOn("Package-Module");

RunTarget(target);

