#addin nuget:?package=Cake.Vagrant
var target = Argument("target", "BootstrapAndBuild");

var sourceDir = Argument("source-dir", "src");
var vagrantBuildDir = Argument("internal-build-dir", "/build");
var outputDir = Argument("output-dir", "output");

Task("Vagrant-Start").Does(() => 
{
    Information("Updating Vagrant environment");
    Vagrant.Box.Update();
    Information("Starting Vagrant environment");
    Vagrant.Up(s => s.UseProvider("virtualbox"));
});

Task("Build").Does(() => 
{
    Information("Copying src files");
    RunCommands($"sudo mkdir -p {vagrantBuildDir}",
                $"sudo rsync -av --delete --exclude tools /vagrant/{sourceDir}/ {vagrantBuildDir}/");

    Information("Starting build ");
    RunCommands($"cd {vagrantBuildDir}",
                $"sudo mkdir -p {outputDir}",
                $"sudo /cake/build.sh --verbosity=Verbose --settings_skipverification=true |& tee /vagrant/build-log.txt");
});

Task("Copy-Results").Does(() => 
{
    Information("Copying results");
    RunCommands($"sudo mkdir -p /vagrant/{outputDir}",
                $"sudo rsync -av --delete {vagrantBuildDir}/{outputDir}/ /vagrant/{outputDir}/");
});

private void RunCommands(params string[] commands)
{
    Vagrant.SSH(s => s.RunCommand(commands.Aggregate((a, b) =>  $"{a};{b}")));
}

Task("Vagrant-Destroy").Does(() => 
{
    Information("Destroying Vagrant environment");
    Vagrant.Destroy();
});

Task("BootstrapAndBuild")
    .IsDependentOn("Vagrant-Start")
    .IsDependentOn("Build")
    .IsDependentOn("Copy-Results")
    .IsDependentOn("Vagrant-Destroy");

RunTarget(target);