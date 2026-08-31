namespace RestoreConflicts
{
    using System.IO;
    using System.Text.RegularExpressions;

    internal class Program
    {
        private static string pattern = @"^(.+)(\-sync\-conflict\-\d{8}\-\d{6}\-\w{7})(\..*)$";
        private static Regex _regex = new Regex(pattern, RegexOptions.Compiled);
        private static string replacement = "$1$3";

        static void Main(string[] args)
        {
            Console.WriteLine("This program will try to resolve Syncthing conflicts by renaming all conflict files to their original name, unless that file already exists.");
            string folder = Environment.CurrentDirectory;
            if (args.Length > 0 ) 
                folder = args[0];

            Console.WriteLine($"Processing folder: {folder}... are you sure? (y/N)");
            if (Console.ReadLine()?.ToLower() != "y")
                return;
            
            try
            {
                Program p = new Program();
                p.ProcessFolder(folder);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }

        public void ProcessFolder(string folder)
        {
            Console.WriteLine($"Folder: {folder}");
            var files = Directory.GetFiles(folder, "*sync-conflict*.*");
            foreach (var file in files)
            {
                ProcessFile(file);
            }

            Console.WriteLine($"\nFinished with files in {folder}, recursing downwards...");
            var dirs = Directory.GetDirectories(folder);
            foreach (var dir in dirs)
            {
                ProcessFolder(dir);
            }
        }

        public void ProcessFile(string filepath)
        {
            var filename = Path.GetFileName(filepath);
            var foldername = Path.GetDirectoryName(filepath) ?? "";

            if (_regex.Match(filename).Success)
            {
                string origfile = _regex.Replace(filename, replacement);
                if (File.Exists(Path.Combine(foldername, origfile)))
                {
                    Console.WriteLine($"\nERROR: Filen {origfile} finns redan");
                }
                else
                {
                    //Console.WriteLine($"{filename} -> {origfile}");
                    Console.Write(".");
                    var source = Path.Combine(foldername, filename);
                    var dest = Path.Combine(foldername, origfile);
                    try
                    {
                        File.Move(source, dest);
                       
                    }
                    catch (Exception ex)
                    {
                        throw new ApplicationException($"Error renaming \n  {filename} \nto \n  {origfile} \nin folder \n  {foldername}", ex);
                    }
                }
            }

        }
    }
}
