using System;
using System.IO;

namespace Dreamland
{
    static class Extensions
    {
        /// <summary>
        /// Reads a null-terminated string from a binary file
        /// </summary>
        /// <param name="stream">The reader of which to read from</param>
        /// <returns>The null-terminated string</returns>
        public static string ReadNullTerminatedString(this BinaryReader stream)
        {
            // Base
            string str = "";
            // Buffer
            char ch;
            // Loop until null
            while ((int)(ch = stream.ReadChar()) != 0)
            {
                // Append char
                str = str + ch;
            }
            // Return result
            return str;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ERROR] Missing input .pak file.");
                Console.ResetColor();
                Console.WriteLine("Usage: Dreamland PakFile.pak");
                Console.WriteLine("\nPress any Key to exit...");
                Console.ReadKey();
                return;
            }

            using (var Reader = new BinaryReader(File.OpenRead(args[0])))
            {
                // Skip magic and version
                Reader.BaseStream.Position += 8;

                // Standard counts
                var FileCount = Reader.ReadUInt32();
                var Alignment = Reader.ReadUInt32();
                var FilesOffset = Reader.ReadUInt32();

                // Create Export Directory
                string currentPath = Directory.GetCurrentDirectory();
                if (!Directory.Exists(Path.Combine(currentPath, "export")))
                    Directory.CreateDirectory(Path.Combine(currentPath, "export"));

                // Loop and read
                for (uint i = 0; i < FileCount; i++)
                {
                    var Hash = Reader.ReadUInt32();
                    var Offset = Reader.ReadUInt32();

                    var FileDataOffset = (Offset * Alignment + FilesOffset);

                    var Size = Reader.ReadUInt32();

                    var JumpBack = Reader.BaseStream.Position;
                    Reader.BaseStream.Position = FileDataOffset;

                    // Export to DSP - Wii Soundfile Format
                    File.WriteAllBytes(string.Format("export/sound_{0}.dsp", FileDataOffset.ToString("X")), Reader.ReadBytes((int)Size));

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(string.Format("[SUCCESS] Exported sound_{0}.dsp", FileDataOffset.ToString("X")));

                    Reader.BaseStream.Position = JumpBack;
                }

                // To Do: Convert DSP to GENH
                // Deinterleave Stream (Interleave of 0x8000 if Stereo)
                // Reinterleave Stream with INTSIZE 8
                // Export to GENH with proper variables and coefficients

                // To Do: Convert GENH to WAV
                // FFMPEG?

                Console.ResetColor();
                Console.WriteLine("\nComplete!");
                Console.WriteLine("Press any Key to exit...");
                Console.ReadKey();
            }
        }
    }
}