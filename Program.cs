using System.Runtime.InteropServices;

[DllImport("kernel32.dll", SetLastError = true)]
[return: MarshalAs(UnmanagedType.Bool)]
static extern bool AllocConsole();

if (args.Length < 3)
{
    AllocConsole();
    Console.Error.WriteLine("wsl-ssh-pageant-monitor.exe sock executable arguments");
    Console.Error.WriteLine("press any key to exit...");
    Console.ReadKey();
    return 1;
}

bool exiting = false;

AppDomain.CurrentDomain.DomainUnload += (object? sender, EventArgs _) =>
{
    Console.WriteLine("exiting because DomainUnload");
    exiting = true;
};

Console.CancelKeyPress += delegate
{
    Console.WriteLine("exiting because CancelKeyPress");
    exiting = true;
};

string sockPath = args[0];
string executable = args[1];
string arguments = args[2];

Console.WriteLine($"using sockPath {sockPath}");
Console.WriteLine($"using executable {executable}");
Console.WriteLine($"using arguments {arguments}");

if (!sockPath.EndsWith(".sock"))
{
    Console.Error.WriteLine("sock path does not end with .sock, exiting so we do not accidentally delete anything important");
    return 3;
}

void MonitorProcess()
{
    while (!exiting)
    {

        Console.WriteLine("starting process...");

        if (File.Exists(sockPath))
        {
            Console.WriteLine("sock file already exists, attempting to delete...");
            try
            {
                File.Delete(sockPath);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"failed to delete sock file: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("sock file does not exist, continuing");
        }

        System.Diagnostics.Process prc = new System.Diagnostics.Process();
        prc.StartInfo.FileName = executable;
        prc.StartInfo.Arguments = arguments;
        prc.Start();

        Console.WriteLine("waiting for exit...");
        prc.WaitForExit();

        if (!exiting)
        {
            Console.WriteLine("exited, sleeping...");
            System.Threading.Thread.Sleep(5000);
        }
    }

}

MonitorProcess();

return 0;
