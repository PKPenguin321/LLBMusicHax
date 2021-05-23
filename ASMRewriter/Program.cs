using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Inject;

namespace ASMRewriter
{
    class Program
    {
        private static readonly string ASMName = "Assembly-CSharp.dll";
        private static readonly string ModName = "MusicHax";

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                var finalASMPath = args[0].Replace("%20", " ");
                Directory.SetCurrentDirectory(finalASMPath);
            }

            Console.WriteLine("Reading assembly...");
            // Here Path.Combine is required for ASMR to run under linux
            AssemblyDefinition modDef = AssemblyDefinition.ReadAssembly(
                Path.Combine(Directory.GetCurrentDirectory(), ModName + ".dll"),
                new ReaderParameters { ReadWrite = true }
            );
            try
            {
                // Same thing here
                using (AssemblyDefinition aDef = AssemblyDefinition.ReadAssembly(
                    Path.Combine(Directory.GetCurrentDirectory(), ASMName),
                    new ReaderParameters { ReadWrite = true })
                )
                {
                    Console.WriteLine(" Attempting inject");
                    try
                    {
                        /* Here the injection method provided by asm.cs isn't enough,
                         * so i made a new one below, i' ll explain the differences there */
                        asm.InjectCallToMethod(
                            aDef, // The LLB dll definition
                            "LLHandlers.AudioHandler", // The class you want to inject stuff in
                            "PlayMusic", // The name of method you want to inject into
                                         /* The ID of the method, this is used when there's multiple declaration of one method.
                                          * To find it, I increment it until it lands on the right one, though it was 0 this time */
                            1,
                            modDef, // The mod dll definition
                            "CecilInjectPatches", // The class that holds the method you want to inject
                            "PlayHaxedMusic", // The method you want to inject
                                              /* There's a lot you can do with Flags, but the documentation went offline.
                                               * You can still check the code, which is well documented, here: 
                                               * https://github.com/ghorsington/Mono.Cecil.Inject/blob/master/Mono.Cecil.Inject/InjectFlags.cs */
                            InjectFlags.PassParametersRef
                        );
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception caught: ");
                        Console.WriteLine(e);
                    }
                    try { aDef.Write(); } catch (Exception ex) { Console.WriteLine(ex); }
                    aDef.Dispose();
                }
            }
            catch
            {
                Console.WriteLine("Could not open assembly, press Enter to exit...");
                Console.ReadLine();
            }

            modDef.Dispose();
        }
    }

    public enum ChangeType
    {
        Public = 0,
        Virtual = 1,
        Abstract = 2
    }
}
