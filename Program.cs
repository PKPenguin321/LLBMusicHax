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
                ) {
                    Console.WriteLine(" Attempting inject");
                    try {
                        /* Here the injection method provided by asm.cs isn't enough,
                         * so i made a new one below, i' ll explain the differences there */                        
                        InjectCallToMethodAtPosition(
                            aDef, // The LLB dll definition
                            "LLHandlers.AudioHandler", // The class you want to inject stuff in
                            "PlayMusic", // The name of method you want to inject into
                            /* The ID of the method, this is used when there's multiple declaration of one method.
                             * To find it, I increment it until it lands on the right one, though it was 0 this time */                           
                            0,
                            modDef, // The mod dll definition
                            "MusicHax", // The class that holds the method you want to inject
                            "PlayHaxedMusic", // The method you want to inject
                            /* There's a lot you can do with Flags, but the documentation went offline.
                             * You can still check the code, which is well documented, here: 
                             * https://github.com/ghorsington/Mono.Cecil.Inject/blob/master/Mono.Cecil.Inject/InjectFlags.cs */                           
                            InjectFlags.PassLocals,
                            11 // The instruction after which you want to inject your call
                        );
                    } catch { }

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

        /* This is a copy paste of InjectCallToMethod, with a few changes. First is the position argument */
        public static void InjectCallToMethodAtPosition(AssemblyDefinition _gameAdef, string _injectClass, string _injectMethod, int _entryNr, AssemblyDefinition _modAdef, string _modClass, string _modMethod, InjectFlags _flag, int position)
        {
            TypeDefinition gameClass = _gameAdef.MainModule.GetType(_injectClass);
            MethodDefinition gameMethod = null;

            if (gameClass != null)
            {
                var i = 0;
                foreach (MethodDefinition method in gameClass.Methods)
                {
                    if (method.Name == _injectMethod)
                    {
                        if (i == _entryNr)
                        {
                            gameMethod = method;
                            break;
                        }
                        else i++;
                    }
                }
            }
            else
            {
                Console.WriteLine("Could not find game class " + _injectClass);
                return;
            }


            if (gameMethod == null)
            {
                Console.WriteLine("Could not find game method " + _injectMethod);
                return;
            }

            MethodDefinition modMethod = null;

            foreach (ModuleDefinition module in _modAdef.Modules)
            {
                foreach (TypeDefinition type in module.Types)
                {
                    foreach (MethodDefinition method in type.Methods)
                    {
                        if (method.Name == _modMethod)
                        {
                            modMethod = method;
                        }
                    }
                }
            }

            if (modMethod == null)
            {
                Console.WriteLine("Could not find mod method " + _modMethod);
                return;
            }

            Console.WriteLine("Defining injector");
            InjectionDefinition injector = new InjectionDefinition(
                gameMethod, 
                modMethod, 
                _flag,
                /* This is the ID of the local variables you want to pass to your injected method.
                 * It is required for the InjectFlags.PassLocals flag, and i was too lazy to put conditionnals around.
                 * Same thing than for the method ID, trial and error until you find the right ID */
                new int[] { 1 } 
            );

            try
            {
                /* Finding the right position, again, is done through trial and error.
                 * You can get a rough idea of what it needs to be by counting every 
                 * instructions (assignements, operations, method calls, etc) to your target position,
                 * trying, and repeting until you land where you need to */
                injector.Inject(position, null, InjectDirection.After);
                Console.WriteLine("Injection into " + gameMethod.ToString() + " OK ");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    public enum ChangeType
    {
        Public = 0,
        Virtual = 1,
        Abstract = 2
    }
}
