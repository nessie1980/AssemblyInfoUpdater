using System;
using System.IO;
using System.Text;

namespace AssemblyInfoUpdater
{
    /// <summary>
    /// Summary description for AssemblyInfoUpdater.
    /// </summary>
    class AssemblyInfoUpdater
    {
        public enum VersionType
        {
            Invalid = -1,
            Major = 0,
            Minor = 1,
            Build = 2,
            Revision = 3,
        }

        private static string _fileName = "";

        private static string _oldVersion;

        private static string _newVersion;

        private static VersionType _versionType = VersionType.Invalid;

        private static string _major = "MAJOR";

        private static string _minor = "MINOR";

        private static string _build = "BUILD";

        private static string _revision = "REVISION";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            var assApp = typeof(AssemblyInfoUpdater).Assembly;
            var assAppName = assApp.GetName();
            var verApp = assAppName.Version;

            var caption = @"   Increase version ( ";
            caption += verApp + " )";

            var captionLine = @" -=";
            for (var i = 0; i <= caption.Length - 5; i++)
                captionLine += "=";
            captionLine += "=-";

            Console.WriteLine("");
            Console.WriteLine(caption);
            Console.WriteLine(captionLine);
            Console.WriteLine("");

            foreach (var argument in args)
            {
                if (argument.StartsWith(_major))
                {
                    _versionType = VersionType.Major;
                }
                if (argument.StartsWith(_minor))
                {
                    _versionType = VersionType.Minor;
                }
                if (argument.StartsWith(_build))
                {
                    _versionType = VersionType.Build;
                }
                if (argument.StartsWith(_revision))
                {
                    _versionType = VersionType.Revision;
                }
                else
                    _fileName = argument;
            }

            if (_fileName == "" || _versionType == VersionType.Invalid)
            {
                Console.WriteLine("Usage: AssemblyInfoUpdater < path to AssemblyInfo.cs > [options]");
                Console.WriteLine("Options: ");
                Console.WriteLine("  {0}  - increases the major index and resets the other", _major);
                Console.WriteLine("  {0}  - increases the minor index and resets the build and revision", _minor);
                Console.WriteLine("  {0}  - increases the build index and resets the revision", _build);
                Console.WriteLine("  {0}  - increases the revision index", _revision);
                Console.WriteLine("");

                return;
            }

            if (!File.Exists(_fileName))
            {
                Console.WriteLine("Error: Can not find file \"" + _fileName + "\"");
                Console.WriteLine("");
                return;
            }

            Console.WriteLine(" Processing \"" + _fileName + "\"...");
            Console.WriteLine("");

            var reader = new StreamReader(_fileName);
            var writer = new StreamWriter(_fileName + ".out");
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                line = ProcessLine(line);
                writer.WriteLine(line);
            }
            reader.Close();
            writer.Close();

            File.Delete(_fileName);
            File.Move(_fileName + ".out", _fileName);

            Console.WriteLine(@" Version part: {0}", _versionType.ToString());
            Console.WriteLine(@"  Old version: {0}", _oldVersion);
            Console.WriteLine(@"  New version: {0}", _newVersion);
            Console.WriteLine("");
            Console.WriteLine(" Done!");
            Console.WriteLine("");
        }

        private static string ProcessLine(string line)
        {
            line = ProcessLinePart(line, "[assembly: AssemblyVersion(\"");
            line = ProcessLinePart(line, "[assembly: AssemblyFileVersion(\"");

            return line;
        }

        private static string ProcessLinePart(string line, string part)
        {
            var stringPos = line.IndexOf(part, StringComparison.Ordinal);

            if (stringPos < 0) return line;

            var versionStart = stringPos + part.Length;
            var versionEnd = line.IndexOf('"', versionStart);
            var oldVersion = line.Substring(versionStart, versionEnd - versionStart);
            var newVersion = "";
            var performChange = false;

            if (_versionType != VersionType.Invalid)
            {
                var nums = oldVersion.Split('.');

                switch (_versionType)
                {
                    case VersionType.Revision:
                    {
                        if ((int) VersionType.Revision + 1 <= nums.Length)
                        {
                            if (nums[(int) VersionType.Revision] != "*")
                            {
                                var val = long.Parse(nums[(int) VersionType.Revision]);
                                val++;
                                nums[(int) VersionType.Revision] = val.ToString();

                                performChange = true;
                            }
                        }
                    }
                    break;

                    case VersionType.Build:
                    {
                        if ((int) VersionType.Build + 1 <= nums.Length)
                        {
                            if (nums[(int) VersionType.Build] != "*")
                            {
                                var val = long.Parse(nums[(int) VersionType.Build]);
                                val++;
                                nums[(int) VersionType.Build] = val.ToString();

                                if ((int) VersionType.Revision + 1 <= nums.Length)
                                {
                                   if (nums[(int)VersionType.Revision] != "*")
                                      nums[(int) VersionType.Revision] = @"0";
                                   else
                                        nums[(int)VersionType.Revision] = @"*";
                                }

                                performChange = true;
                            }
                        }
                    }
                    break;

                    case VersionType.Minor:
                    {
                        if (nums[(int)VersionType.Minor] != "*")
                        {
                            var val = long.Parse(nums[(int)VersionType.Minor]);
                            val++;
                            nums[(int)VersionType.Minor] = val.ToString();

                            if ((int) VersionType.Build + 1 <= nums.Length)
                            {
                                if (nums[(int)VersionType.Build] != @"*")
                                    nums[(int) VersionType.Build] = @"0";
                                else
                                    nums[(int)VersionType.Build] = @"*";
                            }

                            if ((int) VersionType.Revision + 1 <= nums.Length)
                            {
                                if (nums[(int)VersionType.Revision] != @"*")
                                    nums[(int) VersionType.Revision] = @"0";
                                else
                                    nums[(int)VersionType.Revision] = @"0";
                            }

                            performChange = true;
                        }
                    }
                    break;

                    case VersionType.Major:
                    {
                        if (nums[(int)VersionType.Major] != "*")
                        {
                            var val = long.Parse(nums[(int)VersionType.Major]);
                            val++;
                            nums[(int)VersionType.Major] = val.ToString();
                            if ((int) VersionType.Minor + 1 <= nums.Length)
                            {
                                if (nums[(int)VersionType.Minor] != @"*")
                                    nums[(int)VersionType.Minor] = @"0";
                                else
                                    nums[(int)VersionType.Minor] = @"*";
                            }

                                if ((int) VersionType.Build + 1 <= nums.Length)
                            {
                                if(nums[(int)VersionType.Build] != @"*")
                                    nums[(int) VersionType.Build] = @"0";
                                else
                                    nums[(int)VersionType.Build] = @"*";
                            }

                            if ((int) VersionType.Revision + 1 <= nums.Length)
                            {
                                if (nums[(int)VersionType.Revision] != @"*")
                                    nums[(int) VersionType.Revision] = @"0";
                                else
                                    nums[(int)VersionType.Revision] = @"0";
                            }

                            performChange = true;
                        }
                    }
                    break;
                }
 
                newVersion = nums[0];
                for (var i = 1; i < nums.Length; i++)
                {
                    newVersion += "." + nums[i];
                }
            }

            _oldVersion = oldVersion;
            _newVersion = newVersion;

            if (!performChange) return line;

            var str = new StringBuilder(line);
            str.Remove(versionStart, versionEnd - versionStart);
            str.Insert(versionStart, newVersion);
            line = str.ToString();

            return line;
        }
    }
}