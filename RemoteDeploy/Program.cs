using System.Diagnostics;

namespace RemoteDeploy;
internal class Program
{
    static void Main(string[] args)
    {
        string mainProjectName = "HAL";

        string[] extensionsToDelete = { "",
                                        ".deps.json",
                                        ".runtimeconfig.json",
                                        ".pdb",
                                        ".dll",
                                      };

        // Getting project names in the local directory
        // If folder name starts with a dot, it is not a project, skip it
        string[] projects = Directory.GetDirectories(".");
        for (int i = 0; i < projects.Length; i++)
            projects[i] = projects[i].Split('\\').Last();
        projects = projects.Where(x => !x.StartsWith(".")).ToArray();

        // Creating remove command for old files
        string removeCommand = "rm";
        foreach (string project in projects)
            foreach (string file in extensionsToDelete)
                removeCommand += $" debug/{project}{file}";

        Console.WriteLine("Removing previous build artifacts");

        // Removing previous files
        var proc = Process.Start("cmd.exe", $"/c \"ssh root@192.168.3.6 {removeCommand}\"");
        proc.WaitForExit();

        Console.WriteLine("Deploying new files");

        // Publishing new files
        foreach (string project in projects)
        {
            foreach (string file in extensionsToDelete)
            {
                Console.WriteLine($"/c \"scp {project}\\bin\\Debug\\net8.0\\linux-arm64\\publish\\{project}{file} root@192.168.3.6:debug\"");
                Process.Start("cmd.exe", $"/c \"scp {project}\\bin\\Debug\\net8.0\\linux-arm64\\publish\\{project}{file} root@192.168.3.6:debug\"").WaitForExit();
            }
        }

        // Adding execution permission to the startup file
        proc = Process.Start("cmd.exe", $"/c ssh root@192.168.3.6 \"chmod +x debug/{mainProjectName} && pkill -f {mainProjectName}\"");

        proc.WaitForExit();

    }
}
